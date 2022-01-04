using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    internal class ElementManager
    {
        internal IEnumerable<Autodesk.Revit.DB.Element> GetTrackedElements(Autodesk.Revit.DB.Document doc)
        {
            Categories categories = doc.Settings.Categories;
            List<ElementFilter> elementFilters = new List<ElementFilter>();

            foreach (Category c in categories)
            {
                elementFilters.Add(new ElementCategoryFilter(c.Id));
            }

            ElementFilter filter = new LogicalOrFilter(elementFilters);
            return new FilteredElementCollector(doc).WherePasses(filter);
        }
        //element id를 이용하여 해당 element의 task가 model인지 annotation인지 판단해서 반환한다.
        internal string GetCategoryType(Element element)
        {
            switch (element.Category.CategoryType)
            {
                case Autodesk.Revit.DB.CategoryType.Model: return "Model";
                case Autodesk.Revit.DB.CategoryType.Annotation: return "Annotation";
                default: return "Others";
            }
        }
        internal string GetViewType(View activeView)
        {
            string viewType = default;

            if (activeView is Autodesk.Revit.DB.View3D)
            {
                viewType = "3D View";
            }
            else if (activeView is Autodesk.Revit.DB.ViewSection)
            {
                viewType = "Section View";
            }
            else if (activeView is Autodesk.Revit.DB.ViewSheet)
            {
                viewType = "Sheet View";
            }
            else if (activeView is Autodesk.Revit.DB.ViewDrafting)
            {
                viewType = "Drafting View";
            }
            else
            {
                viewType = "Normal View";
            }

            return viewType;
        }
        internal IEnumerable<Element> GetAddedElements(ICollection<ElementId> elementIds)
        {
            try
            {
                //추가된 객체 정보.
                IEnumerable<Element> addedElements = ExternalApplication.totalElements.Where(element => elementIds.Contains(element.Id));
                return addedElements;
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
                Log.WriteToFile(ex.ToString());
                return null;
            }
        }
    }
}
