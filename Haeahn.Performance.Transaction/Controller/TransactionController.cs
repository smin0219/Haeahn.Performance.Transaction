using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Transaction
{
    internal class TransactionController
    {
        internal List<TransactionLog> GetTransactionLogs(ICollection<Autodesk.Revit.DB.ElementId> elementIds, Project project, Employee employee, EventType eventType)
        {
            var currentDateTime = DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"));
            List<TransactionLog> transactionLogs = new List<TransactionLog>();

            Autodesk.Revit.DB.Element rvt_element = null;
            Element element = null;

            foreach (var elementId in elementIds){
                if(eventType != EventType.Deleted)
                {
                    rvt_element = ExternalApplication.rvt_doc.GetElement(elementId);
                    element = new Element(rvt_element);
                    var view = new View();
                    var viewType = view.GetViewType(ExternalApplication.rvt_doc.ActiveView);
                    transactionLogs.Add(new TransactionLog(element, project, employee, viewType, eventType));
                }
                else
                {
                    transactionLogs.Add(new TransactionLog(project, employee, EventType.Deleted));
                    transactionLogs.Where(x => x.EventType == EventType.Deleted.ToString()).Last().ElementId = elementId.ToString();
                }
            }

            return transactionLogs;
        }
    }
}
