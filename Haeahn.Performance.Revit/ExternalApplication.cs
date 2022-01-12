using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Haeahn.Performance.Revit;
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

namespace Haeahn.Performance.Revit
{
    internal class ExternalApplication : IExternalApplication
    {
        public static Autodesk.Revit.ApplicationServices.Application rvt_app;
        public static Autodesk.Revit.UI.UIDocument rvt_uidoc;
        public static Autodesk.Revit.UI.UIApplication rvt_uiapp;
        public static Autodesk.Revit.DB.Document rvt_doc;

        //internal List<Autodesk.Revit.DB.Element> allRevitElements = new List<Autodesk.Revit.DB.Element>();
        internal List<Element> allElements = new List<Element>();

        internal DAO dao = new DAO();

        internal ProjectController projectController = new ProjectController();
        internal SessionController sessionController = new SessionController();
        internal EmployeeController employeeController = new EmployeeController();
        internal ElementController elementController = new ElementController();
        internal TransactionController transactionController = new TransactionController();

        //레빗 프로그램 시작 시 발생하는 이벤트
        public Result OnStartup(Autodesk.Revit.UI.UIControlledApplication application)
        {
            try
            {
                //사용자 정보 확인
                EmployeeController employeeController = new EmployeeController();

                Autodesk.Revit.ApplicationServices.ControlledApplication controlledApplication = application.ControlledApplication;

                application.Idling += new EventHandler<Autodesk.Revit.UI.Events.IdlingEventArgs>(OnIdling);
                controlledApplication.DocumentOpened += new EventHandler<Autodesk.Revit.DB.Events.DocumentOpenedEventArgs>(OnDocumentOpened);
                controlledApplication.DocumentChanged += new EventHandler<Autodesk.Revit.DB.Events.DocumentChangedEventArgs>(OnDocumentChanged);
                controlledApplication.DocumentClosed += new EventHandler<Autodesk.Revit.DB.Events.DocumentClosedEventArgs>(OnDocumentClosed);

                return Result.Succeeded;
            }  
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
                Log.WriteToFile(ex.ToString());
                return Result.Failed;
            }
        }
        //레빗 프로그램 종료 시 발생하는 이벤트
        public Result OnShutdown(Autodesk.Revit.UI.UIControlledApplication application)
        {
            return Result.Succeeded;
        }
        //대기 상태일때 실시간으로 발생하는 이벤트
        public void OnIdling(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs args)
        {
            //현재 열려있는 앱의 정보를 가져온다.
            if (rvt_uiapp == null)
            {
                rvt_uiapp = sender as Autodesk.Revit.UI.UIApplication;
                rvt_app = rvt_uiapp.Application;
            }
        }
        //문서를 열었을때 발생하는 이벤트
        public void OnDocumentOpened(object sender, Autodesk.Revit.DB.Events.DocumentOpenedEventArgs args)
        {
            try
            {
                rvt_uidoc = rvt_uiapp.ActiveUIDocument;
                rvt_doc = rvt_uidoc.Document;

                Employee employee = employeeController.GetEmployee("20210916");
                ProjectInfo projectInformation = rvt_doc.ProjectInformation;
                Project project = (projectInformation == null) ? null : projectController.GetProject(projectInformation);
                Session session = sessionController.GetSession(employee, project);

                string currentDateTime = DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"));
                session.StartTime = currentDateTime;

                string projectCode = dao.SelectProjectCodeFromDB(project);

                //Project Code가 없으면 기존에 DB 삽입된 적이 없는 Project 데이터이다.
                //Project 정보와 Element 정보를 DB에 새로 삽입힌다.
                if (projectCode == null)
                {
                    dao.InsertProjectIntoDB(project);
                    var allRevitElements = elementController.GetAllRevitElements(rvt_doc).ToList();

                    foreach (var rvt_element in allRevitElements)
                    {
                        allElements.Add(elementController.ConvertToElement(rvt_element));
                    }
                    //모든객체(Element)정보 DB에 입력
                    dao.InsertElementsIntoDB(allElements);
                    Log.WriteToFile(string.Format("All elements has been inserted into Performance DB -- {0}", currentDateTime));
                }
                else
                {
                    allElements = dao.SelectAllElementsFromDB(project).ToList();
                    //allRevitElements = elementController.GetAllRevitElements(rvt_doc).ToList();
                }

                //프로젝트 문서를 열람한 사람과 해당 프로젝트 정보 시간을 로그파일에 기록.
                var logMessage = string.Format("{0}({1}) has been opened by {2} -- {3}", project.Name, project.Code, employee.Name, currentDateTime);
                Log.WriteToFile(logMessage);
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
                Log.WriteToFile(ex.ToString());
            }
        }
        //현재 작업중인 문서의 객체가 추가/제거/변경 되었을경우 작동하는 이벤트핸들러.
        public void OnDocumentChanged(object sender, Autodesk.Revit.DB.Events.DocumentChangedEventArgs args)
        {
            try
            {
                //추가/제거/변경된 객체 ID 수집.
                var addedElementIds = args.GetAddedElementIds();
                var deletedElementIds = args.GetDeletedElementIds();
                var modifiedElementIds = args.GetModifiedElementIds();

                ElementController elementController = new ElementController();
                Transaction transaction = new Transaction();
                DAO dao = new DAO();

                //추가된 객체 정보 기록.
                if (addedElementIds.Count > 0)
                {
                    List<Transaction> transactions = new List<Transaction>();
                    foreach(var elementId in addedElementIds)
                    {
                        Autodesk.Revit.DB.Element rvt_element = rvt_doc.GetElement(elementId);
                        //allRevitElements.ToList().Add(rvt_element);
                        allElements.Add(elementController.ConvertToElement(rvt_element));
                        transactions.Add(transactionController.GetTransaction(elementId, EventType.Added));
                    }
                }

                ////삭제된 객체 정보 기록.
                if (deletedElementIds.Count > 0)
                {
                    List<Transaction> transactions = new List<Transaction>();
                    foreach (var elementId in deletedElementIds)
                    {
                        Autodesk.Revit.DB.Element rvt_element = rvt_doc.GetElement(elementId);
                        //allRevitElements.Remove(allRevitElements.Find(x => x.Id == elementId));
                        allElements.Remove(allElements.Find(x => x.Id == elementId.ToString()));
                        transactions.Add(transactionController.GetTransaction(elementId, EventType.Deleted));
                    }
                }

                ////변경된 객체 정보 기록.
                //if (modifiedElementIds.Count > 0)
                //{
                //    dao.Insert(addedElementIds, EventType.Modified);
                //}
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
                Log.WriteToFile(ex.ToString());
            }
        }
        public void OnDocumentClosed(object sender, Autodesk.Revit.DB.Events.DocumentClosedEventArgs args)
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
    }
}
