using Autodesk.Revit.DB;
using Newtonsoft.Json;
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
        //오픈된 레빗 프로젝트의 모든 객체 정보를 받아온다.
        internal IEnumerable<Autodesk.Revit.DB.Element> GetAllElements(Autodesk.Revit.DB.Document doc)
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
                return new FilteredElementCollector(doc)
                    .WherePasses(filter)
                    .WhereElementIsNotElementType()
                    .WhereElementIsViewIndependent();
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
                Log.WriteToFile(ex.ToString());
                return null;
            }
        }
        
        //레벳의 객체를 필요한 정보만 따로 정리한 Element로 만들어서 반환한다.
        internal ElementState TypeConversion(Autodesk.Revit.DB.Element element)
        {
            try
            {
                var rvt_doc = ExternalApplication.rvt_doc;
                var projectInformation = rvt_doc.ProjectInformation;

                ElementState elementState = new ElementState();
                elementState.ProjectCode = projectInformation.Id.ToString();
                elementState.ProjectName = projectInformation.Name;
                elementState.CategoryName = (element.Category == null) ? string.Empty : element.Category.Name;

                Utilities utils = new Utilities();
                elementState.Location = utils.LocationToString(element.Location);

                Options options = new Options();
                elementState.Geometry = JsonConvert.SerializeObject(element.get_Geometry(options));

                ElementType elementType = rvt_doc.GetElement(element.GetTypeId()) as ElementType;
                BoundingBoxXYZ boundingBox = element.get_BoundingBox(null);

                List<Material> materials = new List<Material>();
                var materialIds = element.GetMaterialIds(false);

                foreach (var materialId in materialIds)
                {
                    materials.Add(rvt_doc.GetElement(materialId) as Material);
                }

                if(materials.Count != 0)
                {
                    elementState.Material = JsonConvert.SerializeObject(materials);
                }

                //ParameterManager parameterManager = new ParameterManager();
                //elementState.InstanceParameter = JsonConvert.SerializeObject(parameterManager.GetParameters(element));

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

                return elementState;
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
        internal IEnumerable<Autodesk.Revit.DB.Element> GetAddedElements(IEnumerable<Autodesk.Revit.DB.Element> allElements, ICollection<Autodesk.Revit.DB.ElementId> elementIds)
        {
            try
            {
                //추가된 객체 정보.
                IEnumerable< Autodesk.Revit.DB.Element> addedElements = allElements.Where(element => elementIds.Contains(element.Id));
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
