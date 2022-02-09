using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Transaction
{
    internal class ExternalApplication : IExternalApplication
    {
        internal static Autodesk.Revit.ApplicationServices.Application rvt_app;
        internal static Autodesk.Revit.UI.UIDocument rvt_uidoc;
        internal static Autodesk.Revit.UI.UIApplication rvt_uiapp;
        internal static Autodesk.Revit.DB.Document rvt_doc;
        internal static ICollection<ElementId> selectedElementIds = null;
        internal static ICollection<Element> selectedElements = null;

        private Employee employee = null;
        private Project project = null;

        public Result OnStartup(Autodesk.Revit.UI.UIControlledApplication application)
        {
            //사용자 정보 생성
            employee = new Employee("20210916");
            Autodesk.Revit.ApplicationServices.ControlledApplication controlledApplication = application.ControlledApplication;

            application.Idling += new EventHandler<Autodesk.Revit.UI.Events.IdlingEventArgs>(OnIdlingEvent);
            controlledApplication.DocumentChanged += new EventHandler<Autodesk.Revit.DB.Events.DocumentChangedEventArgs>(OnDocumentChanged);

            RevitCommandId deleteCommandId = RevitCommandId.LookupPostableCommandId(PostableCommand.Delete);
            AddInCommandBinding deletecommandBinding = application.CreateAddInCommandBinding(deleteCommandId);
            deletecommandBinding.Executed += OnElementDeleted;

            return Result.Succeeded;
        }

        private void OnElementDeleted(object sender, ExecutedEventArgs args)
        {
            selectedElements = GetSelectedElements();
            UIApplication uiapp = sender as UIApplication;
            Document doc = uiapp.ActiveUIDocument.Document;
            using (Autodesk.Revit.DB.Transaction transaction = new Autodesk.Revit.DB.Transaction(doc, "Delete Selection"))
            {
                if (transaction.Start() == TransactionStatus.Started)
                {
                    doc.Delete(uiapp.ActiveUIDocument.Selection.GetElementIds());
                    transaction.Commit();
                }
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            throw new NotImplementedException();
        }
        private void OnIdlingEvent(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs args)
        {
            //현재 열려있는 앱의 정보를 가져온다.
            if (rvt_uiapp == null)
            {
                rvt_uiapp = sender as Autodesk.Revit.UI.UIApplication;
                rvt_app = rvt_uiapp.Application;

            }
            else
            {
                rvt_uidoc = rvt_uiapp.ActiveUIDocument;
                rvt_doc = (rvt_uidoc != null) ? rvt_uidoc.Document : null;

                if (rvt_doc != null)
                {
                    //패밀리 파일
                    if (rvt_doc.IsFamilyDocument)
                    {
                        if (project == null)
                        {
                            project = new Project("RFA", "RFA", "RFA");

                        }
                        project.Name = "RFA";
                        project.Code = "RFA";
                        project.Type = "RFA";
                    }
                    //프로젝트 파일
                    else
                    {
                        ProjectInfo projectInformation = rvt_doc.ProjectInformation;
                        if (projectInformation != null)
                        {
                            if (project == null)
                            {
                                project = new Project(projectInformation);
                            }
                            else
                            {
                                project = project.SetProject(projectInformation);
                            }
                        }
                        else
                        {
                            project = new Project("TBD", "TBD", "TBD");
                        }
                    }
                }
            }
        }
        public void OnDocumentChanged(object sender, Autodesk.Revit.DB.Events.DocumentChangedEventArgs args)
        {
            try
            {
                List<Autodesk.Revit.DB.CategoryType> categoryTypes = new List<Autodesk.Revit.DB.CategoryType>
                {
                    Autodesk.Revit.DB.CategoryType.Model,
                    Autodesk.Revit.DB.CategoryType.Annotation
                };

                Element element = new Element();
                ElementFilter elementFilter = element.GetElementFilterByCategoryTypes(categoryTypes);

                //추가/제거/변경된 객체 ID 수집.
                var addedElementIds = args.GetAddedElementIds(elementFilter);
                var modifiedElementIds = args.GetModifiedElementIds(elementFilter);
                var deletedElementIds = args.GetDeletedElementIds();
                var transactionNames = args.GetTransactionNames();

                DAO dao = new DAO();
                TransactionLog transactionLog = new TransactionLog();

                #region ADDED ELEMENTS
                if (addedElementIds.Count > 0)
                {
                    List<TransactionLog> transactionLogs = transactionLog.GetTransactionLogs(addedElementIds, project, employee, EventType.Added);
                    if (transactionLogs.Count > 0)
                    {
                        transactionLogs.ForEach(x => x.TransactionName = transactionNames.First());
                        transactionLogs.Select(x => x.ViewType != null);
                        dao.InsertTransactionLogs(transactionLogs);
                    }
                }
                #endregion

                #region DELETED ELEMENTS
                if (deletedElementIds.Count > 0)
                {
                    List<TransactionLog> transactionLogs = transactionLog.GetTransactionLogs(deletedElementIds, project, employee, EventType.Deleted);
                    if (transactionLogs.Count > 0)
                    {
                        transactionLogs.ForEach(x => x.TransactionName = transactionNames.First());
                        transactionLogs = transactionLogs.Where(x => x.TransactionName != "Load Family").ToList();
                        dao.InsertTransactionLogs(transactionLogs);
                    }
                }
                #endregion

                //객체 붙여넣기를 할 경우에는 관련된 모든 객체가 Modified 되는 현상으로 인해 제외.
                //추후 추가적인 확인 필요
                #region MODIFIED ELEMENTS
                if (modifiedElementIds.Count > 0 && !transactionNames.Contains("Paste"))
                {
                    //Idling 시에 선택된 객체정보를 수집하는데, 드래그해서 위치 변경시 Idling 상태를 스킵하기 때문에 선택된 객체가 지정이 안되서
                    //선택된 객체 정보를 다시 한번 수집.
                    selectedElements = GetSelectedElements();

                    List<TransactionLog> transactionLogs = transactionLog.GetTransactionLogs(modifiedElementIds, project, employee, EventType.Modified);
                    if (transactionLogs.Count > 0)
                    {
                        transactionLogs.ForEach(x => x.TransactionName = transactionNames.First());
                        dao.InsertTransactionLogs(transactionLogs);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
                Log.WriteToFile(ex.ToString());
            }
        }

        public List<Element> GetSelectedElements()
        {
            var selectedElements = new List<Element>();

            selection = rvt_uidoc.Selection;
            selectedElementIds = rvt_uidoc.Selection.GetElementIds();

            if (selectedElementIds.Count != 0)
            {
                selectedElements = new List<Element>();
                foreach (ElementId elementId in selectedElementIds)
                {
                    selectedElements.Add(new Element(rvt_doc.GetElement(elementId)));
                }
            }

            return selectedElements;
        }

        public bool IsElementExist(Element element)
        {
            return rvt_doc.GetElement(element.Id) == null ? true : false;
        }
    }
}