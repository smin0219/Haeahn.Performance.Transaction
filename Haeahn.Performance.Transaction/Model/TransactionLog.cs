using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Transaction
{
    internal class TransactionLog
    {
        internal TransactionLog() { }
        internal TransactionLog(string projectCode, string employeeId, string transactionName, string occurredOn)
        {
            this.ProjectCode = projectCode;
            this.EmployeeId = employeeId;
            this.TransactionName = transactionName;
            this.OccurredOn = occurredOn;
        }

        public string ProjectCode { get; set; }
        public string EmployeeId { get; set; }
        public string TransactionName { get; set; }
        public string OccurredOn { get; set; }
    }
}
