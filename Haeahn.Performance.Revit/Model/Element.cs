using Autodesk.Revit.DB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
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
        //public string Geometry { get; set; }
        public string Verticies { get; set; }
        public Dictionary<string, string> BoundingBox { get; set; }
        public Dictionary<string, string> InstanceParameter { get; set; }
        public Dictionary<string, string> TypeParameter { get; set; }

        internal string CompareTo(Element element)
        {
            try
            {
                List<string> differences = new List<string>();
                var elementType = this.GetType();

                foreach (var property in elementType.GetProperties())
                {
                    var thisElementValue = property.GetValue(this, null);
                    var anotherElementValue = property.GetValue(element, null);

                    if (!object.Equals(thisElementValue, anotherElementValue))
                    {
                        if (property.Name == "BoundingBox")
                        {
                            List<string> differenceList = new List<string>();

                            foreach(var key in this.BoundingBox.Keys)
                            {
                                if(this.BoundingBox[key] != element.BoundingBox[key])
                                {
                                    differenceList.Add(string.Format("\"{0}\": \"{1} -> {2}\"", key.ToString(), this.BoundingBox[key], element.BoundingBox[key]));
                                }
                            }

                            if(differenceList.Count > 0)
                            {
                                differences.Add("\"Bounding Box\": {" + string.Join(",", differenceList) + "}");
                            }
                        }
                        else if (property.Name == "InstanceParameter")
                        {
                            List<string> differenceList = new List<string>();
                            foreach (var key in this.InstanceParameter.Keys)
                            {
                                if (this.InstanceParameter[key] != element.InstanceParameter[key])
                                {
                                    differenceList.Add(string.Format("\"{0}\": \"{1} -> {2}\"", key.ToString(), this.InstanceParameter[key], element.InstanceParameter[key]));
                                }
                            }
                            if(differenceList.Count > 0)
                            {
                                differences.Add("\"Instance Parameter\": {" + string.Join(",", differenceList) + "}");
                            }
                        }
                        else if (property.Name == "TypeParameter")
                        {
                            List<string> differenceList = new List<string>();
                            foreach (var key in this.TypeParameter.Keys)
                            {
                                if (this.TypeParameter[key] != element.TypeParameter[key])
                                {
                                    differenceList.Add(string.Format("\"{0}\": \"{1} -> {2}\"", key.ToString(), this.TypeParameter[key], element.TypeParameter[key]));
                                }
                            }
                            if(differenceList.Count > 0)
                            {
                                differences.Add("\"Type Parameter\": {" + string.Join(",", differenceList) + "}");
                            }
                        }
                        else
                        {
                            differences.Add(string.Format("\"{0}\": \"{1} -> {2}\"", property.Name, thisElementValue, anotherElementValue));
                        }
                    }
                }
                return "{" + string.Join(",",differences) + "}";
            }
            catch(Exception ex)
            {
                Debug.Assert(false, ex.ToString());
                Log.WriteToFile(ex.ToString());
                return null;
            }
            
        }
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

                Utilities utils = new Utilities();

                Options options = new Options();

                GeometryElement geometryElement = rvt_element.get_Geometry(options);

                BoundingBoxXYZ boundingBox = rvt_element.get_BoundingBox(null);
                FamilyInstance familyInstance = rvt_element as FamilyInstance;

                ElementType type = rvt_doc.GetElement(rvt_element.GetTypeId()) as ElementType;

                this.Location = utils.LocationToString(rvt_element.Location);


                if (!(rvt_element is FamilyInstance) && boundingBox != null)
                {
                    this.BoundingBox = utils.GetBoundingBoxMaxMin(boundingBox);
                    var verticies = utils.PointArrayToString(utils.GetCanonicVerticies(rvt_element));
                    this.Verticies = (verticies == "") ? null : verticies;
                }

                ElementType elementType = rvt_doc.GetElement(rvt_element.GetTypeId()) as ElementType;

                ParameterController parameterController = new ParameterController();
                this.InstanceParameter = parameterController.GetElementParameters(rvt_element, false);
                if (type != null)
                {
                    this.FamilyName = type.FamilyName;
                    this.TypeName = type.Name;
                    this.TypeParameter = parameterController.GetElementParameters(rvt_element, true);
                }

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
