using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    internal class TransactionManager
    {
        internal Transaction GetTransaction(Autodesk.Revit.DB.ElementId elementId, EventType eventType)
        {
            Transaction transaction = new Transaction();
            transaction.ElementId = elementId.ToString();
            //transaction.ProjectCode = 

            return transaction;
        }
    }
}
