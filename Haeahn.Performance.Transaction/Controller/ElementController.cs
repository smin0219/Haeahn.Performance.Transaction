using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Transaction
{
    internal class ElementController
    {
        internal Autodesk.Revit.DB.ElementFilter GetElementFilter()
        {
            List<Autodesk.Revit.DB.ElementFilter> elementFilters = new List<Autodesk.Revit.DB.ElementFilter>();
            Autodesk.Revit.DB.Categories categories = ExternalApplication.rvt_doc.Settings.Categories;

            foreach (Autodesk.Revit.DB.Category category in categories)
            {
                if (category.CategoryType == Autodesk.Revit.DB.CategoryType.Model || category.CategoryType == Autodesk.Revit.DB.CategoryType.Annotation)
                {
                    elementFilters.Add(new Autodesk.Revit.DB.ElementCategoryFilter(category.Id));
                }
            }
            return new Autodesk.Revit.DB.LogicalOrFilter(elementFilters);
        }
    }
}
