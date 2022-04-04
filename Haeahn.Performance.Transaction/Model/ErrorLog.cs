using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Transaction.Model
{
    internal class ErrorLog
    {
        internal ErrorLog() { }
        internal ErrorLog(string projectCode, string employeeId, string description, string occurredOn)
        {
            this.ProjectCode = projectCode;
            this.EmployeeId = employeeId;
            this.Description = description;
            this.OccurredOn = occurredOn;
        }
        internal string ProjectCode { get;set; }
        internal string EmployeeId { get; set; }
        internal string Description { get; set; }
        internal string OccurredOn { get; set;  }
    }
}
