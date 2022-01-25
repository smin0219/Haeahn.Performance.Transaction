using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Transaction
{
    class Element
    {
        internal Element() { }
        internal Element(Autodesk.Revit.DB.Element rvt_element)
        {
            CreateElement(rvt_element);
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string CategoryName { get; set; }
        public string CategoryType { get; set; }
        public string FamilyName { get; set; }
        public string TypeName { get; set; }
        public string Location { get; set; }
        public string LevelId { get; set; }
        public string MaterialIds { get; set; }
        public string Verticies { get; set; }

        //레벳의 객체를 필요한 정보만 따로 정리한 Element로 만들어서 반환한다.
        internal Element CreateElement(Autodesk.Revit.DB.Element rvt_element)
        {
            try
            {
                var rvt_doc = ExternalApplication.rvt_doc;
                var projectInformation = rvt_doc.ProjectInformation;

                this.Id = rvt_element.Id.ToString();
                this.Name = rvt_element.Name;
                this.ProjectName = (projectInformation == null) ? null : projectInformation.Name;
                this.ProjectCode = (projectInformation == null) ? null : projectInformation.Number.ToString();
                this.CategoryName = (rvt_element.Category == null) ? string.Empty : rvt_element.Category.Name;
                this.CategoryType = (rvt_element.Category == null) ? string.Empty : rvt_element.Category.CategoryType.ToString();

                Options options = new Options();
                GeometryElement geometryElement = rvt_element.get_Geometry(options);
                BoundingBoxXYZ boundingBox = rvt_element.get_BoundingBox(null);
                FamilyInstance familyInstance = rvt_element as FamilyInstance;
                ElementType type = rvt_doc.GetElement(rvt_element.GetTypeId()) as ElementType;

                return this;
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
