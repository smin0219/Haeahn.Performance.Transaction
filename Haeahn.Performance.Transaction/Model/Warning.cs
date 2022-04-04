using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Transaction.Model
{
    internal class Warning
    {
        internal Warning() { }
        internal Warning(string projectCode,string employeeId, string description, string failureDefinitionId, string severity, string elementIds)
        {
            this.ProjectCode = projectCode;
            this.EmployeeId = employeeId;
            this.Description = description;
            this.Severity = severity;
            this.ElementIds = elementIds;
            this.OccurredOn = DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"));
        }
        internal string ProjectCode { get; set; }
        internal string EmployeeId { get; set; }
        internal string Description { get; set; }
        internal string Severity { get; set; }
        internal string ElementIds { get; set; }
        internal string OccurredOn { get; set; }
    }
}
