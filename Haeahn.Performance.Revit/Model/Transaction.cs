using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    internal class Transaction
    {
        internal Transaction(Element element, string difference, EventType eventType)
        {
            CreateTransaction(element, difference, eventType);
        }
        internal string ElementId { get; set; }
        internal string ElementName { get; set; }
        internal string ProjectCode { get; set; }
        internal string CategoryType { get; set; }
        internal string Difference { get; set; }
        internal string EmployeeId { get; set; }
        internal string EmployeeName { get; set; }
        internal string EventType { get; set; }
        internal string EventDateTime { get; set; }

        internal Transaction CreateTransaction(Element element, string difference, EventType eventType)
        {
            this.ElementId = element.Id.ToString();
            this.ElementName = element.Name.ToString();
            this.ProjectCode = ExternalApplication.projectCode;
            this.CategoryType = element.CategoryType;
            this.Difference = difference;
            this.EmployeeId = ExternalApplication.employee.Id;
            this.EmployeeName = ExternalApplication.employee.Name;
            this.EventType = eventType.ToString();
            this.EventDateTime = DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"));

            return this;
        }
    }
}