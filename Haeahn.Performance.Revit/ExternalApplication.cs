using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Haeahn.Performance.Revit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    internal class ExternalApplication : IExternalApplication
    {
        public static Autodesk.Revit.ApplicationServices.Application rvt_app;
        public static Autodesk.Revit.UI.UIDocument rvt_uidoc;
        public static Autodesk.Revit.UI.UIApplication rvt_uiapp;
        public static Autodesk.Revit.DB.Document rvt_doc;
        public static string projectCode = null;
        public static Employee employee;

        internal DAO dao = new DAO();
        internal List<Element> allElements = new List<Element>();
        internal ElementController elementController = new ElementController();
        
        //레빗 프로그램 시작 시 발생하는 이벤트.
        public Result OnStartup(Autodesk.Revit.UI.UIControlledApplication application)
        {
            try
            {
                Autodesk.Revit.ApplicationServices.ControlledApplication controlledApplication = application.ControlledApplication;

                //사용자 정보 생성
                employee = new Employee("20210916");

                application.Idling += new EventHandler<Autodesk.Revit.UI.Events.IdlingEventArgs>(OnIdlingEvent);
                controlledApplication.DocumentOpened += new EventHandler<Autodesk.Revit.DB.Events.DocumentOpenedEventArgs>(OnDocumentOpenedEvent);
                controlledApplication.DocumentChanged += new EventHandler<Autodesk.Revit.DB.Events.DocumentChangedEventArgs>(OnDocumentChangedEvent);
                controlledApplication.DocumentClosed += new EventHandler<Autodesk.Revit.DB.Events.DocumentClosedEventArgs>(OnDocumentClosedEvent);
                controlledApplication.DocumentCreated += new EventHandler<Autodesk.Revit.DB.Events.DocumentCreatedEventArgs>(OnDocumentCreatedEvent);
                controlledApplication.DocumentSaved += new EventHandler<Autodesk.Revit.DB.Events.DocumentSavedEventArgs>(OnDocumentSavedEvent);
                controlledApplication.DocumentSavedAs += new EventHandler<Autodesk.Revit.DB.Events.DocumentSavedAsEventArgs>(OnDocumentSavedAsEvent);
                controlledApplication.DocumentSynchronizedWithCentral += new EventHandler<Autodesk.Revit.DB.Events.DocumentSynchronizedWithCentralEventArgs>(OnDocumentSynchronizedWithCentralEvent);
                controlledApplication.DocumentReloadedLatest += new EventHandler<Autodesk.Revit.DB.Events.DocumentReloadedLatestEventArgs>(OnDocumentReloadedLatestEvent);
                controlledApplication.DocumentPrinted += new EventHandler<Autodesk.Revit.DB.Events.DocumentPrintedEventArgs>(OnDocumentPrintedEvent);
                controlledApplication.FamilyLoadedIntoDocument += new EventHandler<Autodesk.Revit.DB.Events.FamilyLoadedIntoDocumentEventArgs>(OnFamilyLoadedIntoDocumentEvent);
                controlledApplication.WorksharedOperationProgressChanged += new EventHandler<Autodesk.Revit.DB.Events.WorksharedOperationProgressChangedEventArgs>(OnWorksharedOperationProgressChangedEvent);
                controlledApplication.ViewPrinted += new EventHandler<Autodesk.Revit.DB.Events.ViewPrintedEventArgs>(OnViewPrintedEvent);
                controlledApplication.FileExported += new EventHandler<Autodesk.Revit.DB.Events.FileExportedEventArgs>(OnFileExportedEvent);
                controlledApplication.FileImported += new EventHandler<Autodesk.Revit.DB.Events.FileImportedEventArgs>(OnFileImportedEvent);
                controlledApplication.ElementTypeDuplicated += new EventHandler<Autodesk.Revit.DB.Events.ElementTypeDuplicatedEventArgs>(OnElementTypeDuplicatedEvent);
                controlledApplication.FailuresProcessing += new EventHandler<FailuresProcessingEventArgs>(OnFailuresProcessingEvent);
                controlledApplication.LinkedResourceOpened += new EventHandler<LinkedResourceOpenedEventArgs>(OnLinkedResourceOpenedEvent);
                controlledApplication.ProgressChanged += new EventHandler<ProgressChangedEventArgs>(OnProgressChangedEvent);

                rvt_uiapp.ViewActivated += new EventHandler<Autodesk.Revit.UI.Events.ViewActivatedEventArgs>(OnViewActivatedEvent);
                rvt_uiapp.ApplicationClosing += new EventHandler<Autodesk.Revit.UI.Events.ApplicationClosingEventArgs>(OnApplicationClosingEvent);

                //rvt_doc.doc
                //Autodesk.Revit.DB.Events.DocumentSaveToCentralProgressChangedEventArgs;
                //Autodesk.Revit.DB.Events.DocumentSaveToLocalProgressChangedEventArgs;
                //Autodesk.Revit.DB.Events.PostDocEventArgs;
                //Autodesk.Revit.DB.Events.PostEventArgs;
                //Autodesk.Revit.DB.Events.PreDocEventArgs;
                //Autodesk.Revit.DB.Events.PreEventArgs;
                //Autodesk.Revit.DB.Events.RevitEventArgs;

                //Autodesk.Revit.DB.Events.RevitAPIEventArgs;
                //Autodesk.Revit.DB.Events.ViewExportedEventArgs;

                return Result.Succeeded;
            }  
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
                Log.WriteToFile(ex.ToString());
                return Result.Failed;
            }
        }

        //레빗 프로그램 종료 시 발생하는 이벤트.
        public Result OnShutdown(Autodesk.Revit.UI.UIControlledApplication application)
        {
            return Result.Succeeded;
        }
        public void OnDocumentCreatedEvent(object sender, Autodesk.Revit.DB.Events.DocumentCreatedEventArgs args)
        {
            try
            {
                rvt_uidoc = rvt_uiapp.ActiveUIDocument;
                rvt_doc = rvt_uidoc.Document;

                ProjectInfo projectInformation = rvt_doc.ProjectInformation;
                Project project = (projectInformation == null) ? null : new Project(projectInformation);
                Session session = new Session(employee, project);

                string currentDateTime = DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"));
                session.StartTime = currentDateTime;

                projectCode = dao.SelectProjectCode(project);

                //Project Code가 없으면 기존에 DB 삽입된 적이 없는 Project 데이터이다.
                //Project 정보와 Element 정보를 DB에 새로 삽입한다.
                if (projectCode == null)
                {
                    projectCode = project.Code;
                    dao.InsertProject(project);
                    //var allRevitElements = elementController.GetAllRevitElements(rvt_doc).ToList();

                    //foreach (var rvt_element in allRevitElements)
                    //{
                    //    allElements.Add(new Element(rvt_element));
                    //}
                    //모든객체(Element)정보를 DB에 입력
                    dao.InsertElements(allElements);
                    Log.WriteToFile(string.Format("All elements of the project({0}) have been inserted into Performance DB -- {1}", projectCode, currentDateTime));
                }
                else
                {
                    var message = "기존에 존재하는 Project Code 입니다. Project Code를 변경하고 다시 시도하십시오.";
                    Debug.Assert(false, message);
                    Log.WriteToFile(message);
                }

                //프로젝트 문서를 열람한 사람과 해당 프로젝트 정보 시간을 로그파일에 기록.
                var logMessage = string.Format("Project {0}({1}) has been created by {2} -- {3}", project.Name, project.Code, employee.Name, currentDateTime);
                Log.WriteToFile(logMessage);
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
                Log.WriteToFile(ex.ToString());
            }
        }

        public void OnDocumentSavedEvent(object sender, Autodesk.Revit.DB.Events.DocumentSavedEventArgs args)
        {
        }

        public void OnDocumentSavedAsEvent(object sender, Autodesk.Revit.DB.Events.DocumentSavedAsEventArgs args)
        {
        }

        public void OnDocumentSynchronizedWithCentralEvent(object sedner, Autodesk.Revit.DB.Events.DocumentSynchronizedWithCentralEventArgs args)
        {
        }

        public void OnDocumentReloadedLatestEvent(object sedner, Autodesk.Revit.DB.Events.DocumentReloadedLatestEventArgs args)
        {
        }

        public void OnDocumentPrintedEvent(object sedner, Autodesk.Revit.DB.Events.DocumentPrintedEventArgs args)
        {
        }

        public void OnFamilyLoadedIntoDocumentEvent(object sender, Autodesk.Revit.DB.Events.FamilyLoadedIntoDocumentEventArgs args)
        {
        }

        public void OnDocumentSaveToCentralProgressChangedEvent(object sender, Autodesk.Revit.DB.Events.DocumentSaveToCentralProgressChangedEventArgs args)
        {
        }

        public void OnDocumentSaveToLocalProgressChangedEvent(object sender, Autodesk.Revit.DB.Events.DocumentSaveToLocalProgressChangedEventArgs args)
        {
        }

        public void OnElementTypeDuplicatedEvent(object sender, Autodesk.Revit.DB.Events.ElementTypeDuplicatedEventArgs args)
        {
        }

        public void OnFailuresProcessingEvent(object sender, Autodesk.Revit.DB.Events.FailuresProcessingEventArgs args)
        {
        }

        public void OnFileExportedEvent(object sender, Autodesk.Revit.DB.Events.FileExportedEventArgs args)
        {
        }

        public void OnFileImportedEvent(object sender, Autodesk.Revit.DB.Events.FileImportedEventArgs args)
        {
        }

        public void OnLinkedResourceOpenedEvent(object sender, Autodesk.Revit.DB.Events.LinkedResourceOpenedEventArgs args)
        {
        }

        public void OnPostDocEvent(object sender, Autodesk.Revit.DB.Events.PostDocEventArgs args)
        {
        }

        public void OnPostEvent(object sender, Autodesk.Revit.DB.Events.PostEventArgs args)
        {
        }

        public void OnPreDocEvent(object sender, Autodesk.Revit.DB.Events.PreDocEventArgs args)
        {
        }

        public void OnPreEvent(object sender, Autodesk.Revit.DB.Events.PreEventArgs args)
        {
        }

        public void OnProgressChangedEvent(object sender, Autodesk.Revit.DB.Events.ProgressChangedEventArgs args)
        {
        }

        public void OnRevitEvent(object sender, Autodesk.Revit.DB.Events.RevitEventArgs args)
        {
        }

        public void OnRevitAPIEvent(object sender, Autodesk.Revit.DB.Events.RevitAPIEventArgs args)
        {
        }

        public void OnViewExportedEvent(object sender, Autodesk.Revit.DB.Events.ViewExportedEventArgs args)
        {
        }

        public void OnViewPrintedEvent(object sender, Autodesk.Revit.DB.Events.ViewPrintedEventArgs args)
        {
        }

        public void OnWorksharedOperationProgressChangedEvent(object sender, Autodesk.Revit.DB.Events.WorksharedOperationProgressChangedEventArgs args)
        {
        }

        
        //대기 상태일때 실시간으로 발생하는 이벤트.
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
        public void OnDocumentOpenedEvent(object sender, Autodesk.Revit.DB.Events.DocumentOpenedEventArgs args)
        {
            try
            {
                rvt_uidoc = rvt_uiapp.ActiveUIDocument;
                rvt_doc = rvt_uidoc.Document;

                ProjectInfo projectInformation = rvt_doc.ProjectInformation;
                Project project = (projectInformation == null) ? null : new Project(projectInformation);
                Session session = new Session(employee, project);

                string currentDateTime = DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"));
                session.StartTime = currentDateTime;

                projectCode = dao.SelectProjectCode(project);

                //Project Code가 없으면 기존에 DB 삽입된 적이 없는 Project 데이터이다.
                //Project 정보와 Element 정보를 DB에 새로 삽입한다.
                if (projectCode == null)
                {
                    projectCode = project.Code;
                    dao.InsertProject(project);
                    var allRevitElements = elementController.GetAllRevitElements(rvt_doc).ToList();

                    foreach (var rvt_element in allRevitElements)
                    {
                        allElements.Add(new Element(rvt_element));
                    }   
                    //모든객체(Element)정보를 DB에 입력
                    dao.InsertElements(allElements);
                    Log.WriteToFile(string.Format("All elements of the project({0}) have been inserted into Performance DB -- {1}", projectCode, currentDateTime));
                }
                else
                {
                    allElements = dao.SelectAllElements(project).ToList();
                }

                //프로젝트 문서를 열람한 사람과 해당 프로젝트 정보 시간을 로그파일에 기록.
                var logMessage = string.Format("Project {0}({1}) has been opened by {2} -- {3}", project.Name, project.Code, employee.Name, currentDateTime);
                Log.WriteToFile(logMessage);
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
                Log.WriteToFile(ex.ToString());
            }
        }
        //현재 작업중인 문서의 객체가 추가/제거/변경 되었을경우 작동하는 이벤트핸들러.
        public void OnDocumentChangedEvent(object sender, Autodesk.Revit.DB.Events.DocumentChangedEventArgs args)
        {
            try
            {
                //추가/제거/변경된 객체 ID 수집.
                var addedElementIds = args.GetAddedElementIds();
                var deletedElementIds = args.GetDeletedElementIds();
                var modifiedElementIds = args.GetModifiedElementIds();

                string currentDateTime = DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"));

                ElementController elementController = new ElementController();
                DAO dao = new DAO();

                #region ADDED ELEMENTS
                //추가된 객체 정보 기록.
                if (addedElementIds.Count > 0)
                {
                    List<Element> addedElements = new List<Element>();
                    List<Transaction> transactions = new List<Transaction>();
                    foreach (var elementId in addedElementIds)
                    {
                        Autodesk.Revit.DB.Element rvt_element = rvt_doc.GetElement(elementId);
                        Element element = new Element(rvt_element);
                        addedElements.Add(element);
                        allElements.Add(element);
                        transactions.Add(new Transaction(element, null, EventType.Added));
                        Log.WriteToFile(string.Format(" {0}({1}) has been added -- {2}", element.Name, element.Id, currentDateTime));
                    }

                    dao.InsertElements(addedElements);
                    dao.InsertTransactions(transactions);
                }
                #endregion

                #region DELETED ELEMENTS
                if (deletedElementIds.Count > 0)
                {
                    List<Element> deletedElements = new List<Element>();
                    List<Transaction> transactions = new List<Transaction>();

                    foreach (var elementId in deletedElementIds)
                    {
                        Element element = (allElements.Find(x => x.Id == elementId.ToString()));
                        //TEST - 기존의 존재하지 않는 객체도 있어서 확인후 조정
                        if (element != null)
                        {
                            transactions.Add(new Transaction(element, null, EventType.Deleted));
                            allElements.Remove(allElements.Find(x => x.Id == elementId.ToString()));
                            Log.WriteToFile(string.Format("element({0}) has been deleted -- {1}", elementId, currentDateTime));
                        }
                    }

                    if(deletedElements.Count > 0)
                    {
                        dao.DeleteElements(deletedElementIds.Select(x => x.ToString()));
                        dao.InsertTransactions(transactions);
                    }
                }
                #endregion

                #region MODIFIED ELEMENTS
                ////변경된 객체 정보 기록.
                if (modifiedElementIds.Count > 0)
                {
                    List<Element> modifiedElements = new List<Element>();
                    List<Transaction> transactions = new List<Transaction>();

                    foreach (var elementId in modifiedElementIds)
                    {
                        Autodesk.Revit.DB.Element rvt_element = rvt_doc.GetElement(elementId);
                        Element element = (allElements.Find(x => x.Id == elementId.ToString()));

                        //TEST - 기존의 존재하지 않는 객체도 있어서 확인후 조정
                        if(element != null)
                        {
                            modifiedElements.Add(element);
                            var differences = element.CompareTo(new Element(rvt_element));
                            transactions.Add(new Transaction(element, differences, EventType.Modified));
                            Log.WriteToFile(string.Format("element({0}) has been modified -- {1}", elementId.ToString(), currentDateTime));
                        }
                    }

                    if(modifiedElements.Count > 0)
                    {
                        dao.UpdateElements(modifiedElements);
                        dao.InsertTransactions(transactions);
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
        //문서를 닫을때 발생하는 이벤트.
        public void OnDocumentClosedEvent(object sender, Autodesk.Revit.DB.Events.DocumentClosedEventArgs args)
        {
            try
            {
                Console.WriteLine("Test Line");
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
                Log.WriteToFile(ex.ToString());
            }
        }

        public void OnViewActivatedEvent(object sender, Autodesk.Revit.UI.Events.ViewActivatedEventArgs args)
        {

        }

        public void OnApplicationClosingEvent(object sender, Autodesk.Revit.UI.Events.ApplicationClosingEventArgs args)
        {

        }
    }
}
