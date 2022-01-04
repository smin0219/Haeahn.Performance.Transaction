using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    internal class Transaction
    {
        internal string EmployeeId { get; set; }
        internal string EmployeeName { get; set; }
        internal string ProjectCode { get; set; }
        internal string ElementId { get; set; }
        internal string ViewType { get; set; }
        internal string CategoryName { get; set; }
        internal string CategoryType { get; set; }
        internal string FamilyName { get; set; }
        public string TypeName { get; set; }
        public string Location { get; set; }
        public string Geometry { get; set; }
        public string Verticies { get; set; }
        internal string Difference { get; set; }
        internal string Event { get; set; }
        internal string EventDateTime { get; set; }

        internal Transaction GetTransaction(ElementId elementId, EventType eventType)
        {
            Employee employee = ExternalApplication.employee;
            Document rvt_doc = ExternalApplication.rvt_doc;
            Element element = rvt_doc.GetElement(elementId);
            ElementType elementType = rvt_doc.GetElement(element.GetTypeId()) as ElementType;
            ElementManager elementManager = new ElementManager();

            Transaction transaction = new Transaction();

            transaction.EmployeeId = (employee.EmployeeId == null) ? string.Empty : employee.EmployeeId;
            transaction.EmployeeName = (employee.EmployeeName == null) ? string.Empty : employee.EmployeeName;
            transaction.ProjectCode = (rvt_doc.ProjectInformation.Id == null) ? string.Empty : rvt_doc.ProjectInformation.Id.ToString();
            transaction.ElementId = (element.Id == null) ? string.Empty : element.Id.ToString();
            transaction.CategoryName = (element.Category == null) ? string.Empty : element.Category.Name;
            transaction.CategoryType = (elementManager.GetCategoryType(element) == null) ? string.Empty : elementManager.GetCategoryType(element).ToString();
            transaction.ViewType = (elementManager.GetViewType(rvt_doc.ActiveView) == null) ? string.Empty : elementManager.GetViewType(rvt_doc.ActiveView).ToString();
            transaction.FamilyName = (elementType == null) ? string.Empty : elementType.FamilyName;

            
            return transaction;
        }
    }
}
internal string OriginalState { get; set; }
internal string ModifiedState { get; set; }
internal string Difference { get; set; }
internal string Event { get; set; }
internal string EventDateTime { get; set; }