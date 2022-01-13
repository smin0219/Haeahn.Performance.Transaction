using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    internal class TransactionController
    {
        internal Transaction GetTransaction(Element element, string difference, EventType eventType)
        {
            Transaction transaction = new Transaction();
            transaction.ElementId = element.Id.ToString();
            transaction.ElementName = element.Name.ToString();
            transaction.ProjectCode = ExternalApplication.projectCode;
            transaction.CategoryType = element.CategoryType;
            transaction.Difference = difference;
            transaction.EventType = eventType.ToString();
            transaction.EventDateTime = DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"));

            return transaction;
        }
    }
}
