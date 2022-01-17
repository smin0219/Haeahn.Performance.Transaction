using Autodesk.Revit.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    internal class ElementController
    {
        //오픈된 레빗 프로젝트의 모든 객체 정보를 받아온다.
        internal IEnumerable<Autodesk.Revit.DB.Element> GetAllRevitElements(Autodesk.Revit.DB.Document doc)
        {
            try
            {
                Categories categories = doc.Settings.Categories;
                List<ElementFilter> elementFilters = new List<ElementFilter>();

                foreach (Category c in categories)
                {
                    elementFilters.Add(new ElementCategoryFilter(c.Id));
                }

                ElementFilter filter = new LogicalOrFilter(elementFilters);
                return new FilteredElementCollector(doc).WherePasses(filter).Where(x => x.Category != null);
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
                Log.WriteToFile(ex.ToString());
                return null;
            }
        }
        
        internal string GetCategoryType(Autodesk.Revit.DB.Element element)
        {
            return element.Category.CategoryType.ToString();
        }
        internal IEnumerable<Autodesk.Revit.DB.Element> GetAddedElements(IEnumerable<Autodesk.Revit.DB.Element> AllElements, ICollection<Autodesk.Revit.DB.ElementId> elementIds)
        {
            try
            {
                //추가된 객체 정보.
                IEnumerable< Autodesk.Revit.DB.Element> addedElements = AllElements.Where(element => elementIds.Contains(element.Id));
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
