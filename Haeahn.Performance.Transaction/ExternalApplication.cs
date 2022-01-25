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

            controlledApplication.DocumentOpened += new EventHandler<Autodesk.Revit.DB.Events.DocumentOpenedEventArgs>(OnDocumentOpened);
            controlledApplication.DocumentCreated += new EventHandler<Autodesk.Revit.DB.Events.DocumentCreatedEventArgs>(OnDocumentCreated);
            controlledApplication.DocumentChanged += new EventHandler<Autodesk.Revit.DB.Events.DocumentChangedEventArgs>(OnDocumentChanged);
            controlledApplication.FamilyLoadedIntoDocument += new EventHandler<Autodesk.Revit.DB.Events.FamilyLoadedIntoDocumentEventArgs>(OnFamilyLoadedIntoDocument);

            return Result.Succeeded;
        }

        public void OnIdlingEvent(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs args)
        {
            //현재 열려있는 앱의 정보를 가져온다.
            if (rvt_uiapp == null)
            {
                rvt_uiapp = sender as Autodesk.Revit.UI.UIApplication;
                rvt_app = rvt_uiapp.Application;
            }
        }

        //문서를 열었을때 발생하는 이벤트.
        public void OnDocumentOpened(object sender, Autodesk.Revit.DB.Events.DocumentOpenedEventArgs args)
        {
            try
            {
                //오픈된 문서 정보 수집
                rvt_uidoc = rvt_uiapp.ActiveUIDocument;
                rvt_doc = rvt_uidoc.Document;

                //프로젝트 정보 수집
                ProjectInfo projectInformation = rvt_doc.ProjectInformation;
                project = (projectInformation == null) ? null : new Project(projectInformation);

            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
                Log.WriteToFile(ex.ToString());
            }
        }

        public void OnDocumentCreated(object sender, Autodesk.Revit.DB.Events.DocumentCreatedEventArgs args)
        {
            try
            {
                //오픈된 문서 정보 수집
                rvt_uidoc = rvt_uiapp.ActiveUIDocument;
                rvt_doc = rvt_uidoc.Document;

                //프로젝트 정보 수집
                ProjectInfo projectInformation = rvt_doc.ProjectInformation;
                Project project = (projectInformation == null) ? null : new Project(projectInformation);

            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
                Log.WriteToFile(ex.ToString());
            }
        }

        public void OnDocumentChanged(object sender, Autodesk.Revit.DB.Events.DocumentChangedEventArgs args)
        {
            try
            {
                ElementController elementController = new ElementController();
                ElementFilter elementFilter = elementController.GetElementFilter();

                //추가/제거/변경된 객체 ID 수집.
                var addedElementIds = args.GetAddedElementIds(elementFilter);
                var modifiedElementIds = args.GetModifiedElementIds(elementFilter);
                var deletedElementIds = args.GetDeletedElementIds();
                var transactionNames = args.GetTransactionNames();
                
                DAO dao = new DAO();
                var collector = new FilteredElementCollector(rvt_doc).WherePasses(elementFilter);
                TransactionController transactionController = new TransactionController();

                #region ADDED ELEMENTS
                if (addedElementIds.Count > 0)
                {
                    List<TransactionLog> transactionLogs = transactionController.GetTransactionLogs(addedElementIds, project, employee, EventType.Added);
                    transactionLogs.ForEach(x => x.Transaction = transactionNames.First());
                    transactionLogs.Select(x => x.ViewType != null);
                    dao.InsertTransactionLogs(transactionLogs);
                }
                #endregion

                //객체 붙여넣기를 할 경우에는 관련된 모든 객체가 Modified 되는 현상으로 인해 제외.
                //추후 추가적인 확인 필요
                #region MODIFIED ELEMENTS
                if (modifiedElementIds.Count > 0 && !transactionNames.Contains("Paste"))
                {
                    List<TransactionLog> transactionLogs = transactionController.GetTransactionLogs(modifiedElementIds, project, employee, EventType.Modified);
                    transactionLogs.ForEach(x => x.Transaction = transactionNames.First());
                    dao.InsertTransactionLogs(transactionLogs);
                }
                #endregion

                #region DELETED ELEMENTS
                if (deletedElementIds.Count > 0)
                {
                    List<TransactionLog> transactionLogs = transactionController.GetTransactionLogs(deletedElementIds, project, employee, EventType.Deleted);
                    transactionLogs.ForEach(x => x.Transaction = transactionNames.First());
                    dao.InsertTransactionLogs(transactionLogs);
                }
                #endregion

            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
                Log.WriteToFile(ex.ToString());
            }
        }
        public void OnFamilyLoadedIntoDocument(object sender, Autodesk.Revit.DB.Events.FamilyLoadedIntoDocumentEventArgs args)
        {
            //오픈된 문서 정보 수집
            rvt_uidoc = rvt_uiapp.ActiveUIDocument;
            rvt_doc = rvt_uidoc.Document;

            //프로젝트 정보 수집
            ProjectInfo projectInformation = rvt_doc.ProjectInformation;
            project = (projectInformation == null) ? null : new Project(projectInformation);
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            throw new NotImplementedException();
        }

        #region Autodesk.Revit.DB.Events

        #endregion
    }
}