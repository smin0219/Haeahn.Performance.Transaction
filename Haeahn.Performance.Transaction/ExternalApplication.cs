using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.UI.Selection;
using Haeahn.Performance.Transaction.Controller;
using Haeahn.Performance.Transaction.Data;
using Haeahn.Performance.Transaction.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Haeahn.Performance.Transaction
{
    internal class ExternalApplication : IExternalApplication
    {
        internal static Autodesk.Revit.ApplicationServices.Application rvt_app;
        internal static Autodesk.Revit.UI.UIDocument rvt_uidoc;
        internal static Autodesk.Revit.UI.UIApplication rvt_uiapp;
        internal static Autodesk.Revit.DB.Document rvt_doc;
        internal static ICollection<Autodesk.Revit.DB.ElementId> rvt_selectedElementIds = null;
        internal static ICollection<Haeahn.Performance.Transaction.Model.Element> selectedElements = null;

        internal static Employee employee = null;
        internal static Project project = null;

        public Result OnStartup(Autodesk.Revit.UI.UIControlledApplication application)
        {
            Authentication auth = new Authentication();

            while (true)
            {
                var loginResult = auth.Login();

                if(loginResult.Item1 == "failed")
                {
                    continue;
                }
                else if(loginResult.Item1 == "cancelled")
                {
                    Environment.Exit(0);
                }
                else
                {
                    employee = loginResult.Item2;
                    break;
                }
            }

            //사용자 정보 생성
            Autodesk.Revit.ApplicationServices.ControlledApplication controlledApplication = application.ControlledApplication;

            //ELEMENT_LOG DB
            application.Idling += new EventHandler<Autodesk.Revit.UI.Events.IdlingEventArgs>(OnIdlingEvent);
            controlledApplication.DocumentChanged += new EventHandler<Autodesk.Revit.DB.Events.DocumentChangedEventArgs>(OnDocumentChanged);

            //TRANSACTION_LOG DB
            controlledApplication.DocumentSynchronizedWithCentral += new EventHandler<DocumentSynchronizedWithCentralEventArgs>(OnDocumentSynchronizedWithCentral);
            controlledApplication.DocumentOpened += new EventHandler<DocumentOpenedEventArgs>(OnDocumentOpened);
            controlledApplication.DocumentClosed += new EventHandler<DocumentClosedEventArgs>(OnDocumentClosed);
            controlledApplication.DocumentCreated += new EventHandler<DocumentCreatedEventArgs>(OnDocumentCreated);
            controlledApplication.DocumentSaved += new EventHandler<DocumentSavedEventArgs>(OnDocumentSaved);
            //controlledApplication.FamilyLoadedIntoDocument
            //controlledApplication.FileExported
            //controlledApplication.FileImported

            RevitCommandId deleteCommandId = RevitCommandId.LookupPostableCommandId(PostableCommand.Delete);
            AddInCommandBinding deletecommandBinding = application.CreateAddInCommandBinding(deleteCommandId);
            deletecommandBinding.Executed += OnElementDeleted;

            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {
            var warnings = rvt_doc.GetWarnings();
            InsertWarningsToDB(warnings);
            InsertTransactionLogToDB(new TransactionLog(project.Code, employee.Id, "Revit Shut Down", DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
            return Result.Succeeded;
        }

        public void OnDocumentClosed(object sender, DocumentClosedEventArgs args)
        {
            InsertTransactionLogToDB(new TransactionLog(project.Code, employee.Id, "Document Closed", DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
            var warnings = rvt_doc.GetWarnings();
            InsertWarningsToDB(warnings);
        }
        private void OnDocumentCreated(object sender, DocumentCreatedEventArgs args)
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
                    //var modelPath = rvt_doc.GetWorksharingCentralModelPath();
                    //if (!modelPath.ServerPath)

                    if (project == null || project.Name != rvt_doc.ProjectInformation.Name)
                    {
                        var isCentral = IsCentral(rvt_doc);
                        var centralFilePath = GetCentralFilePath(rvt_doc);
                        project = new Project(rvt_doc, isCentral, centralFilePath);
                    }
                }
            }

            InsertTransactionLogToDB(new TransactionLog(project.Code, employee.Id, "Document Created", DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));

            var warnings = rvt_doc.GetWarnings();
            InsertWarningsToDB(warnings);
        }
        private void OnDocumentSaved(object sender, DocumentSavedEventArgs args)
        {
            InsertTransactionLogToDB(new TransactionLog(project.Code, employee.Id, "Document Saved", DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
            var warnings = rvt_doc.GetWarnings();
            InsertWarningsToDB(warnings);
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
                    if(project == null || project.Name != rvt_doc.ProjectInformation.Name)
                    {
                        var isCentral = IsCentral(rvt_doc);
                        var centralFilePath = GetCentralFilePath(rvt_doc);
                        project = new Project(rvt_doc, isCentral, centralFilePath);
                    }
                }
            }
        }
        public void OnDocumentChanged(object sender, Autodesk.Revit.DB.Events.DocumentChangedEventArgs args)
        {
            try
            {
                DAO dao = new DAO();

                List<Autodesk.Revit.DB.CategoryType> categoryTypes = new List<Autodesk.Revit.DB.CategoryType>
                {
                    Autodesk.Revit.DB.CategoryType.Model,
                    Autodesk.Revit.DB.CategoryType.Annotation
                };

                ElementLogController elementController = new ElementLogController();
                ElementFilter elementFilter = elementController.GetElementFilterByCategoryTypes(categoryTypes);

                //추가/제거/변경된 객체 ID 수집.
                var addedElementIds = args.GetAddedElementIds(elementFilter);
                var modifiedElementIds = args.GetModifiedElementIds(elementFilter);
                var deletedElementIds = args.GetDeletedElementIds();
                var transactions = args.GetTransactionNames();

                #region ADDED ELEMENTS
                if (addedElementIds.Count > 0)
                {
                    List<ElementLog> elementLogs = elementController.GetElementLogs(addedElementIds, project, employee, EventType.Added);
                    if (elementLogs.Count > 0)
                    {
                        elementLogs.ForEach(x => x.TransactionName = transactions.First());
                        elementLogs.Select(x => x.ViewType != null);
                        dao.InsertElementLogs(elementLogs);
                    }
                }
                #endregion

                #region DELETED ELEMENTS
                if (deletedElementIds.Count > 0)
                {
                    List<ElementLog> elementLogs = elementController.GetElementLogs(deletedElementIds, project, employee, EventType.Deleted);
                    if (elementLogs.Count > 0)
                    {
                        elementLogs.ForEach(x => x.TransactionName = transactions.First());
                        elementLogs = elementLogs.Where(x => x.TransactionName != "Load Family").ToList();
                        dao.InsertElementLogs(elementLogs);
                    }
                }
                #endregion

                //객체 붙여넣기를 할 경우에는 관련된 모든 객체가 Modified 되는 현상으로 인해 제외.
                //추후 추가적인 확인 필요
                #region MODIFIED ELEMENTS
                if (modifiedElementIds.Count > 0 && !transactions.Contains("Paste"))
                {
                    selectedElements = GetSelectedElements();

                    List<ElementLog> elementLogs = elementController.GetElementLogs(modifiedElementIds, project, employee, EventType.Modified);
                    if (elementLogs.Count > 0)
                    {
                        elementLogs.ForEach(x => x.TransactionName = transactions.First());
                        dao.InsertElementLogs(elementLogs);
                        var warnings = rvt_doc.GetWarnings();
                        InsertWarningsToDB(warnings);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                DAO dao = new DAO();
                dao.InsertErrorLog(new ErrorLog(ExternalApplication.project.Name, ExternalApplication.employee.Id, ex.Message, DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
            }
        }

        private void InsertWarningsToDB(IList<FailureMessage> warnings)
        {
            var warningList = new List<Warning>();

            if(warnings.Count > 0)
            {
                foreach (var warning in warnings)
                {
                    warningList.Add(
                        new Warning(
                            project.Code,
                            employee.Id,
                            warning.GetDescriptionText(),
                            warning.GetFailureDefinitionId().ToString(),
                            warning.GetSeverity().ToString(),
                            JsonConvert.SerializeObject(warning.GetFailingElements().Select(x => x.ToString()).ToList()))
                        );
                }
                DAO dao = new DAO();
                dao.InsertWarnings(warningList);
            }
        }

        private void InsertTransactionLogToDB(TransactionLog transactionLog)
        {
            DAO dao = new DAO();
            dao.InsertTransactionLog(transactionLog);
        }

        private void OnDocumentOpened(object sender, Autodesk.Revit.DB.Events.DocumentOpenedEventArgs args)
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
                    if (project == null || project.Name != rvt_doc.ProjectInformation.Name)
                    {
                        var isCentral = IsCentral(rvt_doc);
                        var centralFilePath = GetCentralFilePath(rvt_doc);
                        project = new Project(rvt_doc, isCentral, centralFilePath);
                    }
                }
            }

            InsertTransactionLogToDB(new TransactionLog(project.Code, employee.Id, "Document Opened", DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
            var warnings = rvt_doc.GetWarnings();
            InsertWarningsToDB(warnings);
        }

        private void OnDocumentSynchronizedWithCentral(object sender, Autodesk.Revit.DB.Events.DocumentSynchronizedWithCentralEventArgs args)
        {
            InsertTransactionLogToDB(new TransactionLog(project.Code, employee.Id, "Document Synchronized With Central", DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
            var warnings = rvt_doc.GetWarnings();
            InsertWarningsToDB(warnings);
        }


        public List<Haeahn.Performance.Transaction.Model.Element> GetSelectedElements()
        {
            var selectedElements = new List<Haeahn.Performance.Transaction.Model.Element>();

            rvt_selectedElementIds = rvt_uidoc.Selection.GetElementIds();

            if (rvt_selectedElementIds.Count != 0)
            {
                selectedElements = new List<Haeahn.Performance.Transaction.Model.Element>();
                foreach (ElementId elementId in rvt_selectedElementIds)
                {
                    selectedElements.Add(new Haeahn.Performance.Transaction.Model.Element(rvt_doc.GetElement(elementId)));
                }
            }

            return selectedElements;
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

        private bool IsCentral(Document doc)
        {
            BasicFileInfo basicFileInfo = BasicFileInfo.Extract(doc.PathName);

            if (basicFileInfo.IsCentral)
                return true;
            else
                return false;
        }

        private string GetCentralFilePath(Document doc)
        {
            BasicFileInfo basicFileInfo = BasicFileInfo.Extract(doc.PathName);
            return basicFileInfo.CentralPath == null ? "" : basicFileInfo.CentralPath;
        }
    }
}