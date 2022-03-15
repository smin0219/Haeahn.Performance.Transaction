using Haeahn.Performance.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Evaluation
{
    internal class Evaluator
    {
        public static void Main(string[] args)
        {
            DAO dao = new DAO(); 
            var employeeId = "20210916";
            List<TransactionLog> transactionLogs = dao.SelectTransactionLogs(employeeId);
            Evaluator evaluator = new Evaluator();
            evaluator.Evaluate(transactionLogs);
        }

        public void Evaluate(List<TransactionLog> transactionLogs)
        {
        }
    }
}
