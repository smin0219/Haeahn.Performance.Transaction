﻿using Autodesk.Revit.DB;
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

        internal List<Autodesk.Revit.DB.Element> entireElements = new List<Autodesk.Revit.DB.Element>();
        internal List<ElementState> entireElementStates = new List<ElementState>();

        internal DAO dao = new DAO();

        internal ProjectManager projectManager = new ProjectManager();
        internal SessionManager sessionManager = new SessionManager();
        internal EmployeeManager employeeManager = new EmployeeManager();
        internal ElementManager elementManager = new ElementManager();
        internal TransactionManager transactionManager = new TransactionManager();

        //레빗 프로그램 시작 시 발생하는 이벤트
        public Result OnStartup(Autodesk.Revit.UI.UIControlledApplication application)
        {
            try
            {
                //사용자 정보 확인
                EmployeeManager employeeManager = new EmployeeManager();
                EmployeeManager.employee = employeeManager.GetEmployee("20210916");

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

                Employee employee = EmployeeManager.employee;
                var currentDateTime = DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"));

                var projectInformation = rvt_doc.ProjectInformation;
                var projectName = projectInformation.Name;
                var projectCode = projectInformation.Number.ToString();

                var project = projectManager.GetProject(projectName, projectCode);
                var session = sessionManager.GetSession(project, employee);
                session.StartTime = currentDateTime;

                //모든 객체 정보를 추출
                entireElements = elementManager.GetAllElements(rvt_doc).ToList();

                foreach(var element in entireElements)
                {
                    entireElementStates.Add(elementManager.TypeConversion(element));
                }

                dao.Insert(entireElementStates);

                //프로젝트 문서를 열람한 사람과 해당 프로젝트 정보 시간을 로그파일에 기록.
                var logMessage = string.Format("{0}({1}) has been opened by {2} -- {3}", project.Name, project.Code, employee.Name, currentDateTime);
                Log.WriteToFile(logMessage);
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
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

                ElementManager elementManager = new ElementManager();
                Transaction transaction = new Transaction();
                DAO dao = new DAO();

                var allRevitElements = elementManager.GetAllElements(rvt_doc);

                //추가된 객체 정보 기록.
                if (addedElementIds.Count > 0)
                {
                    List<Autodesk.Revit.DB.Element> addedElements = elementManager.GetAddedElements(allRevitElements, addedElementIds).ToList();
                    List<Transaction> transactions = new List<Transaction>();

                    foreach(var elementId in addedElementIds)
                    {
                        transactions.Add(transactionManager.GetTransaction(elementId, EventType.Added));
                    }

                    JsonConvert.SerializeObject(addedElements, Formatting.Indented).ToString();

                    //변경사항은 DB에 저장.
                    dao.Insert(transactions, EventType.Added);
                }

                ////삭제된 객체 정보 기록.
                //if(deletedElementIds.Count > 0)
                //{
                //    dao.Insert(addedElementIds, EventType.Deleted);
                //}

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
