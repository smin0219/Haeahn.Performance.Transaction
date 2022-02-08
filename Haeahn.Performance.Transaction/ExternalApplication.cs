using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
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
        public static Autodesk.Revit.ApplicationServices.Application rvt_app;
        public static Autodesk.Revit.UI.UIDocument rvt_uidoc;
        public static Autodesk.Revit.UI.UIApplication rvt_uiapp;
        public static Autodesk.Revit.DB.Document rvt_doc;

        internal Employee employee = null;
        internal Project project = null;    

        public Result OnStartup(Autodesk.Revit.UI.UIControlledApplication application)
        {
            //사용자 정보 생성
            employee = new Employee("20210916");
            Autodesk.Revit.ApplicationServices.ControlledApplication controlledApplication = application.ControlledApplication;

            application.Idling += new EventHandler<Autodesk.Revit.UI.Events.IdlingEventArgs>(OnIdlingEvent);
            controlledApplication.DocumentChanged += new EventHandler<Autodesk.Revit.DB.Events.DocumentChangedEventArgs>(OnDocumentChanged);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            throw new NotImplementedException();
        }

        public void OnIdlingEvent(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs args)
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

                if(rvt_doc != null)
                {
                    //패밀리 파일
                    if (rvt_doc.IsFamilyDocument)
                    {
                        if(project == null)
                        {
                            project = new Project("RFA", "RFA", "RFA");
                            
                        }
                        project.Name = "RFA";
                        project.Code = "RFA";
                        project.Type = "RFA";
                    }
                    //그외
                    else
                    {
                        ProjectInfo projectInformation = rvt_doc.ProjectInformation;
                        if (projectInformation != null)
                        {
                            if(project == null)
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
                    if(transactionLogs.Count > 0)
                    {
                        transactionLogs.ForEach(x => x.TransactionName = transactionNames.First());
                        transactionLogs.Select(x => x.ViewType != null);
                        dao.InsertTransactionLogs(transactionLogs);
                    }
                }
                #endregion

                //객체 붙여넣기를 할 경우에는 관련된 모든 객체가 Modified 되는 현상으로 인해 제외.
                //추후 추가적인 확인 필요
                #region MODIFIED ELEMENTS
                if (modifiedElementIds.Count > 0 && !transactionNames.Contains("Paste"))
                {
                    List<TransactionLog> transactionLogs = transactionLog.GetTransactionLogs(modifiedElementIds, project, employee, EventType.Modified);
                    if (transactionLogs.Count > 0)
                    {
                        transactionLogs.ForEach(x => x.TransactionName = transactionNames.First());
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

            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
                Log.WriteToFile(ex.ToString());
            }
        }
    }
}