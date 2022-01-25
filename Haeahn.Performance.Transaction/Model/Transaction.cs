using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Transaction
{
    internal class TransactionLog
    {
        internal TransactionLog(Project project, Employee employee, EventType eventType)
        {
            CreateTransactionLog(project, employee, eventType);
        }

        internal TransactionLog(Element element, Project project, Employee employee, ViewType viewType, EventType eventType)
        {
            CreateTransactionLog(element, project, employee, viewType, eventType);
        }

        internal string ProjectCode { get; set; }
        internal string ProjectName { get; set; }
        internal string ProjectType { get; set; }
        internal string ElementId { get; set; }
        internal string ElementName { get; set; }
        internal string CategoryType { get; set; }
        internal string CategoryName { get; set; }
        internal string ViewType { get; set; }
        internal string FamilyName { get; set; }
        internal string TypeName { get; set; }
        internal string Transaction { get; set; }
        internal string EmployeeId { get; set; }
        internal string EmployeeName { get; set; }
        internal string Department { get; set; }
        internal string EventType { get; set; }
        internal string OccurredOn { get; set; }

        internal TransactionLog CreateTransactionLog(Project project, Employee employee, EventType eventType)
        {
            this.ProjectCode = (project == null) ? "RFA" : project.Code;    
            this.ProjectName = (project == null) ? "RFA" : project.Name;
            this.ProjectType = (project == null) ? "TBD" : project.Type;
            this.ElementId = null;
            this.ElementName = null;
            this.CategoryType = null;
            this.CategoryName = null;
            this.ViewType = null;
            this.FamilyName = null;
            this.TypeName = null;
            this.EmployeeId = employee.Id;
            this.EmployeeName = employee.Name;
            this.Department = employee.Department;
            this.EventType = eventType.ToString();
            this.OccurredOn = DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"));

            return this;
        }
        internal TransactionLog CreateTransactionLog(Element element, Project project, Employee employee, ViewType viewType, EventType eventType)
        {
            this.ProjectCode = (project == null) ? "RFA" : project.Code;
            this.ProjectName = (project == null) ? "RFA" : project.Name;
            this.ProjectType = (project == null) ? "TBD" : project.Type;
            this.ElementId = element.Id.ToString();
            this.ElementName = element.Name.ToString();
            this.CategoryType = element.CategoryType;
            this.CategoryName = element.CategoryName;
            this.ViewType = viewType.ToString();
            this.FamilyName = element.FamilyName;
            this.TypeName = element.TypeName;
            this.EmployeeId = employee.Id;
            this.EmployeeName = employee.Name;
            this.Department = employee.Department;
            this.EventType = eventType.ToString();
            this.OccurredOn = DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"));

            return this;
        }
    }
}
