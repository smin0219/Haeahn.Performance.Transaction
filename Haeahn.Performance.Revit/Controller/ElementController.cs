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
        
        //레벳의 객체를 필요한 정보만 따로 정리한 Element로 만들어서 반환한다.
        internal Element ConvertToElement(Autodesk.Revit.DB.Element rvt_element)
        {
            try
            {
                var rvt_doc = ExternalApplication.rvt_doc;
                var projectInformation = rvt_doc.ProjectInformation;

                Element element = new Element();

                element.Id = rvt_element.Id.ToString();
                element.Name = rvt_element.Name;
                element.ProjectName = (projectInformation == null) ? null : projectInformation.Name;
                element.ProjectCode = (projectInformation == null) ? null : projectInformation.Number.ToString();
                element.CategoryName = (rvt_element.Category == null) ? string.Empty : rvt_element.Category.Name;

                Utilities utils = new Utilities();

                element.Location = utils.LocationToString(rvt_element.Location);

                Options options = new Options();
                GeometryController geometryController = new GeometryController();

                GeometryElement geometryElement = rvt_element.get_Geometry(options);

                if(geometryElement != null)
                {
                    Solid solid = geometryController.GetSolid(geometryElement);
                    element.Geometry = JsonConvert.SerializeObject(solid);
                }

                ElementType elementType = rvt_doc.GetElement(rvt_element.GetTypeId()) as ElementType;
                BoundingBoxXYZ boundingBox = rvt_element.get_BoundingBox(null);

                ParameterController parameterController = new ParameterController();
                element.InstanceParameter = JsonConvert.SerializeObject(parameterController.GetParameters(rvt_element));

                //if (elementType != null)
                //{
                //    elementState.FamilyName = elementType.FamilyName;
                //    elementState.TypeName = elementType.Name;
                //    elementState.TypeParameter = JsonConvert.SerializeObject(parameterManager.GetParameters(elementType));
                //}
                //if (!(element is FamilyInstance) && boundingBox != null)
                //{
                //    elementState.BoundingBox = JsonConvert.SerializeObject(boundingBox);
                //    elementState.Verticies = utils.PointArrayToString(utils.GetCanonicVerticies(element));
                //}

                return element;
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

        internal IEnumerable<Autodesk.Revit.DB.Element> AddElement(ref List<Autodesk.Revit.DB.Element> elements, Autodesk.Revit.DB.Element element)
        {
            try
            {
                elements.ToList().Add(element);
                return elements;
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
