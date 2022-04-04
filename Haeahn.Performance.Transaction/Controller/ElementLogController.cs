using Haeahn.Performance.Transaction.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Transaction.Controller
{
    //Model과 Annotation 객체 정보 수집을 위한 컨트롤러.
    internal class ElementLogController 
    {
        internal List<ElementLog> GetElementLogs(ICollection<Autodesk.Revit.DB.ElementId> elementIds, Project project, Employee employee, EventType eventType)
        {
            var currentDateTime = DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"));
            List<ElementLog> elementLogs = new List<ElementLog>();

            Autodesk.Revit.DB.Element rvt_element = null;
            Element element = null;

            var view = new Haeahn.Performance.Transaction.Model.View();
            var viewType = view.GetViewType(ExternalApplication.rvt_doc.ActiveView);

            foreach (var elementId in elementIds)
            {
                if (eventType == Haeahn.Performance.Transaction.EventType.Added)
                {
                    rvt_element = ExternalApplication.rvt_doc.GetElement(elementId);
                    if (rvt_element != null)
                    {
                        element = new Element(rvt_element);
                        elementLogs.Add(new ElementLog(project, employee, viewType, eventType, element));
                    }
                }
                else
                {
                    if (ExternalApplication.rvt_selectedElementIds.Contains(elementId))
                    {
                        element = ExternalApplication.selectedElements.Where(el => el.Id == elementId.ToString()).First();
                        elementLogs.Add(new ElementLog(project, employee, viewType, eventType, element));
                    }
                }
            }

            return elementLogs;
        }

        internal Autodesk.Revit.DB.ElementFilter GetElementFilterByCategoryTypes(List<Autodesk.Revit.DB.CategoryType> categoryTypes)
        {
            List<Autodesk.Revit.DB.ElementFilter> elementFilters = new List<Autodesk.Revit.DB.ElementFilter>();
            Autodesk.Revit.DB.Categories categories = ExternalApplication.rvt_doc.Settings.Categories;

            foreach (Autodesk.Revit.DB.Category category in categories)
            {
                if (categoryTypes.Contains(category.CategoryType))
                {
                    elementFilters.Add(new Autodesk.Revit.DB.ElementCategoryFilter(category.Id));
                }
            }
            return new Autodesk.Revit.DB.LogicalOrFilter(elementFilters);
        }
    }
}
