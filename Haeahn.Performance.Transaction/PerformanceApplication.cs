using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
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
using Microsoft.Toolkit.Uwp.Notifications;


namespace Haeahn.Performance.Transaction
{
    public class PerformanceApplication : IExternalApplication
    {
        public static Autodesk.Revit.ApplicationServices.Application rvt_app;
        public static Autodesk.Revit.ApplicationServices.ControlledApplication rvt_controlledApp;
        public static Autodesk.Revit.UI.UIDocument rvt_uidoc;
        public static Autodesk.Revit.UI.UIApplication rvt_uiapp;
        public static Autodesk.Revit.DB.Document rvt_doc;
        public static ICollection<Autodesk.Revit.DB.ElementId> rvt_selectedElementIds = null;
        public static ICollection<Haeahn.Performance.Transaction.Model.Element> selectedElements = null;
        public static ICollection<Haeahn.Performance.Transaction.Model.Element> deletedElements = null;
        public static Employee employee = null;
        public static Project project = null;

        public Result OnStartup(Autodesk.Revit.UI.UIControlledApplication application)
        {
            //사용자 정보 생성
            rvt_controlledApp = application.ControlledApplication;

            //사용자 정보 확인
            CheckAuthentication();

            //프로그램 버전 정보 확인
            CheckVersion();

            //ELEMENT_LOG DB
            application.Idling += new EventHandler<Autodesk.Revit.UI.Events.IdlingEventArgs>(OnIdlingEvent);
            rvt_controlledApp.DocumentChanged += new EventHandler<Autodesk.Revit.DB.Events.DocumentChangedEventArgs>(OnDocumentChanged);

            //TRANSACTION_LOG DB
            rvt_controlledApp.DocumentSynchronizedWithCentral += new EventHandler<DocumentSynchronizedWithCentralEventArgs>(OnDocumentSynchronizedWithCentral);
            rvt_controlledApp.DocumentOpened += new EventHandler<DocumentOpenedEventArgs>(OnDocumentOpened);
            rvt_controlledApp.DocumentClosed += new EventHandler<DocumentClosedEventArgs>(OnDocumentClosed);
            rvt_controlledApp.DocumentCreated += new EventHandler<DocumentCreatedEventArgs>(OnDocumentCreated);
            rvt_controlledApp.DocumentSaved += new EventHandler<DocumentSavedEventArgs>(OnDocumentSaved);
            rvt_controlledApp.DocumentSavedAs += new EventHandler<DocumentSavedAsEventArgs>(OnDocumentSavesAs);
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
            //var warnings = rvt_doc.GetWarnings();
            //InsertWarningsToDB(warnings);
            //InsertTransactionLogToDB(new TransactionLog(project.Code, employee.Id, "Revit Shut Down", DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
            return Result.Succeeded;
        }

        #region EVENT
        private void OnIdlingEvent(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs args)
        {
            try
            {
                SetApplication(sender);
            }
            catch (Exception ex)
            {
                DAO dao = new DAO();
                var errorMsg = ex.Message + '\n' + ex.StackTrace;
                dao.InsertErrorLog(new ErrorLog(PerformanceApplication.project.Name, PerformanceApplication.employee.Id, errorMsg, DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
            }
        }
        public void OnDocumentClosed(object sender, DocumentClosedEventArgs args)
        {
            try
            {
                InsertTransactionLogToDB(new TransactionLog(project.Code, employee.Id, "Document Closed", DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
                var warnings = rvt_doc.GetWarnings();
                InsertWarningsToDB(warnings);
            }
            catch (Exception ex)
            {
                DAO dao = new DAO();
                var errorMsg = ex.Message + '\n' + ex.StackTrace;
                dao.InsertErrorLog(new ErrorLog(PerformanceApplication.project.Name, PerformanceApplication.employee.Id, errorMsg, DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
            }
        }
        public void OnDocumentCreated(object sender, DocumentCreatedEventArgs args)
        {
            try
            {
                SetApplication(sender, true);

                InsertTransactionLogToDB(new TransactionLog(project.Code, employee.Id, "Document Created", DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));

                var warnings = rvt_doc.GetWarnings();
                InsertWarningsToDB(warnings);
            }
            catch (Exception ex)
            {
                DAO dao = new DAO();
                var errorMsg = ex.Message + '\n' + ex.StackTrace;
                dao.InsertErrorLog(new ErrorLog(PerformanceApplication.project.Name, PerformanceApplication.employee.Id, errorMsg, DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
            }
        }
        public void OnDocumentSaved(object sender, DocumentSavedEventArgs args)
        {
            InsertTransactionLogToDB(new TransactionLog(project.Code, employee.Id, "Document Saved", DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
            var warnings = rvt_doc.GetWarnings();
            InsertWarningsToDB(warnings);
            CheckVersion();
        }
        public void OnDocumentSavesAs(object sender, DocumentSavedAsEventArgs args)
        {
            InsertTransactionLogToDB(new TransactionLog(project.Code, employee.Id, "Document Saved As", DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
            var warnings = rvt_doc.GetWarnings();
            InsertWarningsToDB(warnings);
            CheckVersion();
        }
        public void OnDocumentChanged(object sender, Autodesk.Revit.DB.Events.DocumentChangedEventArgs args)
        {
            try
            {
                DAO dao = new DAO();

                //selectedElements = GetSelectedElements();

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
                    List<ElementLog> elementLogs = elementController.GetElementLogs(addedElementIds, project, employee, EventType.Added, false);
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
                    List<ElementLog> elementLogs = elementController.GetElementLogs(deletedElementIds, project, employee, EventType.Deleted, true);
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
                        
                    }
                }
                var warnings = rvt_doc.GetWarnings();
                InsertWarningsToDB(warnings);
                #endregion
            }
            catch (Exception ex)
            {
                DAO dao = new DAO();
                var errorMsg = ex.Message + '\n' + ex.StackTrace;
                dao.InsertErrorLog(new ErrorLog(PerformanceApplication.project.Name, PerformanceApplication.employee.Id, errorMsg, DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
            }
        }
        private void OnDocumentOpened(object sender, Autodesk.Revit.DB.Events.DocumentOpenedEventArgs args)
        {
            try
            {
                SetApplication(sender);

                InsertTransactionLogToDB(new TransactionLog(project.Code, employee.Id, "Document Opened", DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
                var warnings = rvt_doc.GetWarnings();
                InsertWarningsToDB(warnings);
            }
            catch (Exception ex)
            {
                DAO dao = new DAO();
                var errorMsg = ex.Message + '\n' + ex.StackTrace;
                dao.InsertErrorLog(new ErrorLog(PerformanceApplication.project.Name, PerformanceApplication.employee.Id, errorMsg, DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
            }
            
        }
        private void OnDocumentSynchronizedWithCentral(object sender, Autodesk.Revit.DB.Events.DocumentSynchronizedWithCentralEventArgs args)
        {
            try
            {
                InsertTransactionLogToDB(new TransactionLog(project.Code, employee.Id, "Document Synchronized With Central", DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
                var warnings = rvt_doc.GetWarnings();
                InsertWarningsToDB(warnings);
                CheckVersion();
            }
            catch (Exception ex)
            {
                DAO dao = new DAO();
                var errorMsg = ex.Message + '\n' + ex.StackTrace;
                dao.InsertErrorLog(new ErrorLog(PerformanceApplication.project.Name, PerformanceApplication.employee.Id, errorMsg, DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
            }
        }
        private void OnElementDeleted(object sender, ExecutedEventArgs args)
        {
            try
            {
                deletedElements = GetSelectedElements();
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
            catch (Exception ex)
            {
                DAO dao = new DAO();
                var errorMsg = ex.Message + '\n' + ex.StackTrace;
                dao.InsertErrorLog(new ErrorLog(PerformanceApplication.project.Name, PerformanceApplication.employee.Id, errorMsg, DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
            }
        }
        #endregion

        public List<Haeahn.Performance.Transaction.Model.Element> GetSelectedElements()
        {
            var selectedElements = new List<Haeahn.Performance.Transaction.Model.Element>();

            try
            {
                rvt_selectedElementIds = rvt_uidoc.Selection.GetElementIds();

                if (rvt_selectedElementIds.Count != 0)
                {
                    selectedElements = new List<Haeahn.Performance.Transaction.Model.Element>();
                    foreach (ElementId elementId in rvt_selectedElementIds)
                    {
                        selectedElements.Add(new Haeahn.Performance.Transaction.Model.Element(rvt_doc.GetElement(elementId)));
                    }
                }
            }
            catch (Exception ex)
            {
                DAO dao = new DAO();
                var errorMsg = ex.Message + '\n' + ex.StackTrace;
                dao.InsertErrorLog(new ErrorLog(PerformanceApplication.project.Name, PerformanceApplication.employee.Id, errorMsg, DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
              
            }

            return selectedElements;
        }
        private void SetApplication(object sender, bool? isCreate = false)
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
                    if(isCreate != true)
                    {
                        if (project == null || project.FilePath != rvt_doc.PathName)
                        {
                            var isCentral = IsCentral(rvt_doc);
                            var filePath = rvt_doc.PathName;
                            project = new Project(rvt_doc, isCentral, filePath);
                        }
                    }
                    else
                    {
                        project = new Project(rvt_doc, false, "");
                    }
                }
            }
        }

        private void InsertWarningsToDB(IList<FailureMessage> warnings)
        {
            try
            {
                var warningList = new List<Warning>();

                if (warnings.Count > 0)
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
            catch (Exception ex)
            {
                DAO dao = new DAO();
                var errorMsg = ex.Message + '\n' + ex.StackTrace;
                dao.InsertErrorLog(new ErrorLog(PerformanceApplication.project.Name, PerformanceApplication.employee.Id, errorMsg, DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
            }
        }
        private void InsertTransactionLogToDB(TransactionLog transactionLog)
        {
            try
            {
                DAO dao = new DAO();
                dao.InsertTransactionLog(transactionLog);
            }
            catch (Exception ex)
            {
                DAO dao = new DAO();
                var errorMsg = ex.Message + '\n' + ex.StackTrace;
                dao.InsertErrorLog(new ErrorLog(PerformanceApplication.project.Name, PerformanceApplication.employee.Id, errorMsg, DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
 
            }
        }
        private bool IsCentral(Document doc)
        {
            try
            {
                BasicFileInfo basicFileInfo = BasicFileInfo.Extract(doc.PathName);

                if (basicFileInfo.IsCentral)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                DAO dao = new DAO();
                var errorMsg = ex.Message + '\n' + ex.StackTrace;
                dao.InsertErrorLog(new ErrorLog(PerformanceApplication.project.Name, PerformanceApplication.employee.Id, errorMsg, DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
                return false;
            }
        }
        private string GetCentralFilePath(Document doc)
        {
            try
            {
                BasicFileInfo basicFileInfo = BasicFileInfo.Extract(doc.PathName);
                return basicFileInfo.CentralPath == null ? "" : basicFileInfo.CentralPath;
            }
            catch (Exception ex)
            {
                DAO dao = new DAO();
                var errorMsg = ex.Message + '\n' + ex.StackTrace;
                dao.InsertErrorLog(new ErrorLog(PerformanceApplication.project.Name, PerformanceApplication.employee.Id, errorMsg, DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
                return "";
            }
        }
        private void CheckAuthentication()
        {
            try
            {
                Authentication auth = new Authentication();

                while (true)
                {
                    var loginResult = auth.Login();

                    if (loginResult.Item1 == "failed")
                    {
                        continue;
                    }
                    else if (loginResult.Item1 == "cancelled")
                    {
                        Environment.Exit(0);
                    }
                    else
                    {
                        employee = loginResult.Item2;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                DAO dao = new DAO();
                var errorMsg = ex.Message + '\n' + ex.StackTrace;
                dao.InsertErrorLog(new ErrorLog(PerformanceApplication.project.Name, PerformanceApplication.employee.Id, errorMsg, DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
            }
        }
        private void CheckVersion()
        {
            try
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);


                DAO dao = new DAO();

                string dllVersion = String.Format("{0}", fvi.FileVersion);
                string latestDllVersion = dao.GetLatestVersionNumber();
                string rvtVersion = String.Format("{0}", rvt_controlledApp.VersionNumber);
                string fileLocation = String.Format("{0}", assembly.Location);

                if (dllVersion != latestDllVersion)
                {
                    var autoUpdateApp = Process.GetProcessesByName("WpfApp1");
                    if (autoUpdateApp.Length > 0)
                    {
           
                        return;
                    }

                    Process process = new Process();
                    process.StartInfo.FileName = Path.Combine(Path.GetDirectoryName(fileLocation), "WpfApp1.exe");
                    process.StartInfo.Arguments = String.Format("{0} {1} {2} {3}", CurrentUser.Instance.UserName, rvtVersion, dllVersion, fileLocation);
                    process.Start();

                }

            }
            catch (Exception ex)
            {
                DAO dao = new DAO();
                var errorMsg = ex.Message + '\n' + ex.StackTrace;
                dao.InsertErrorLog(new ErrorLog("", "", errorMsg, DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
            }
        }
    }
}