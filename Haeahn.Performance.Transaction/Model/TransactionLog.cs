using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Transaction
{
    public class TransactionLog
    {
        public TransactionLog() { }
        //internal TransactionLog(Project project, Employee employee, Haeahn.Performance.Transaction.EventType eventType)
        //{
        //    SetTransactionLog(project, employee, eventType);
        //}
        internal TransactionLog(Project project, Employee employee, ViewType viewType, Haeahn.Performance.Transaction.EventType eventType, Element element = null)
        {
            SetTransactionLog(project, employee, element, viewType, eventType);
        }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string ProjectType { get; set; }
        public string ElementId { get; set; }
        public string ElementName { get; set; }
        public string CategoryType { get; set; }
        public string CategoryName { get; set; }
        public string ViewType { get; set; }
        public string FamilyName { get; set; }
        public string TypeName { get; set; }
        public string TransactionName { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public string EventType { get; set; }
        public string OccurredOn { get; set; }
        //internal TransactionLog SetTransactionLog(Project project, Employee employee, EventType eventType)
        //{
        //    this.ProjectCode = project.Code;    
        //    this.ProjectName = project.Name;
        //    this.ProjectType = project.Type;
        //    this.ElementId = null;
        //    this.ElementName = null;
        //    this.CategoryType = null;
        //    this.CategoryName = null;
        //    this.ViewType = null;
        //    this.FamilyName = null;
        //    this.TypeName = null;
        //    this.EmployeeId = employee.Id;
        //    this.EmployeeName = employee.Name;
        //    this.Department = employee.Department;
        //    this.EventType = eventType.ToString();
        //    this.OccurredOn = DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"));

        //    return this;
        //}
        internal TransactionLog SetTransactionLog(Project project, Employee employee, Element element, ViewType viewType, EventType eventType)
        {
            this.ProjectCode = project.Code;
            this.ProjectName = project.Name;
            this.ProjectType = project.Type;
            this.ElementId = (element != null) ? element.Id.ToString() : null;
            this.ElementName = (element != null) ? element.Name.ToString() : null;
            this.CategoryType = (element != null) ? element.CategoryType : null;
            this.CategoryName = (element != null) ? element.CategoryName : null;
            this.FamilyName = (element != null) ? element.FamilyName : null;
            this.TypeName = (element != null) ? element.TypeName : null;
            this.ViewType = viewType.ToString();
            this.EmployeeId = employee.Id;
            this.EmployeeName = employee.Name;
            this.Department = employee.Department;
            this.EventType = eventType.ToString();
            this.OccurredOn = DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"));

            return this;
        }
          
        internal List<TransactionLog> GetTransactionLogs(ICollection<Autodesk.Revit.DB.ElementId> elementIds, Project project, Employee employee, EventType eventType)
        {
            var currentDateTime = DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"));
            List<TransactionLog> transactionLogs = new List<TransactionLog>();

            Autodesk.Revit.DB.Element rvt_element = null;
            Element element = null;

            var view = new View();
            var viewType = view.GetViewType(ExternalApplication.rvt_doc.ActiveView);

            foreach (var elementId in elementIds)
            {
                if (eventType != Haeahn.Performance.Transaction.EventType.Deleted)
                {
                    rvt_element = ExternalApplication.rvt_doc.GetElement(elementId);
                    if(rvt_element != null)
                    {
                        element = new Element(rvt_element);
                        
                        transactionLogs.Add(new TransactionLog(project, employee, viewType, eventType, element));
                    }
                }
                else
                {
                    transactionLogs.Add(new TransactionLog(project, employee, viewType, Haeahn.Performance.Transaction.EventType.Deleted));
                    transactionLogs.Where(x => x.EventType == Haeahn.Performance.Transaction.EventType.Deleted.ToString()).Last().ElementId = elementId.ToString();
                }
            }

            return transactionLogs;
        }
    }
}
