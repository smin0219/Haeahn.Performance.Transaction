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

        //internal List<Autodesk.Revit.DB.Element> allRevitElements = new List<Autodesk.Revit.DB.Element>();
        internal List<Element> allElements = new List<Element>();

        internal DAO dao = new DAO();
        internal ElementController elementController = new ElementController();
        
        //레빗 프로그램 시작 시 발생하는 이벤트
        public Result OnStartup(Autodesk.Revit.UI.UIControlledApplication application)
        {
            try
            {
                Autodesk.Revit.ApplicationServices.ControlledApplication controlledApplication = application.ControlledApplication;

                //사용자 정보 생성
                employee = new Employee("20210916");

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
                    //allRevitElements = elementController.GetAllRevitElements(rvt_doc).ToList();
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
        public void OnDocumentChanged(object sender, Autodesk.Revit.DB.Events.DocumentChangedEventArgs args)
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
                        allElements.AddRange(addedElements);
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
                        transactions.Add(new Transaction(element, null, EventType.Deleted));
                        allElements.Remove(allElements.Find(x => x.Id == elementId.ToString()));
                        Log.WriteToFile(string.Format("element({0}) has been deleted -- {1}", elementId, currentDateTime));
                    }

                    dao.DeleteElements(deletedElementIds.Select(x => x.ToString()));
                    dao.InsertTransactions(transactions);
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
                        modifiedElements.Add(element);
                        var differences =  element.CompareTo(new Element(rvt_element));
                        transactions.Add(new Transaction(element, differences, EventType.Modified));
                        Log.WriteToFile(string.Format("element({0}) has been modified -- {1}", elementId.ToString(), currentDateTime));
                    }

                    dao.UpdateElements(modifiedElements);
                    dao.InsertTransactions(transactions);
                }
                #endregion
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
