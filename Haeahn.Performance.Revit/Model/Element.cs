using Newtonsoft.Json;
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
        public string Id { get; set; }
        public string Name { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string CategoryName { get; set; }
        public string CategoryType { get; set; }
        public string FamilyName { get; set; }
        public string TypeName { get; set; }
        public string Location { get; set; }
        public string Geometry { get; set; }
        public string Verticies { get; set; }
        public string BoundingBox { get; set; }
        public string InstanceParameter { get; set; }
        public string TypeParameter { get; set; }

        internal Dictionary<string, List<string>> CompareTo(Element element)
        {
            try
            {
                Dictionary<string, List<string>> differences = new Dictionary<string, List<string>>();
                var elementType = this.GetType();

                foreach (var property in elementType.GetProperties())
                {
                    var thisElementValue = property.GetValue(this, null);
                    var anotherElementValue = property.GetValue(element, null);

                    if (!object.Equals(thisElementValue, anotherElementValue))
                    {
                        List<string> differentParameterValues = new List<string>();
                        if (property.Name == "Geometry" || property.Name == "BoundingBox" || property.Name == "InstanceParameter" || property.Name == "TypeParameter")
                        {
                            var thisElementParameter = JsonConvert.DeserializeObject<Dictionary<string, string>>((string)thisElementValue);
                            var anotherElementParameter = JsonConvert.DeserializeObject<Dictionary<string, string>>((string)anotherElementValue);

                            foreach (var key in thisElementParameter.Keys)
                            {
                                if (thisElementParameter[key] != anotherElementParameter[key])
                                {
                                    differentParameterValues.Add(string.Format("{0}: {1} -> {2}", key, thisElementParameter[key], anotherElementParameter[key]));
                                }
                            }
                            differences.Add(property.Name, differentParameterValues);
                        }
                        else
                        {
                            differentParameterValues.Add(string.Format("{0} -> {1}", thisElementValue, anotherElementValue));
                            differences.Add(property.Name, differentParameterValues);
                        }
                    }
                }
                return differences;
            }
            catch(Exception ex)
            {
                Debug.Assert(false, ex.ToString());
                Log.WriteToFile(ex.ToString());
                return null;
            }
            
        }
    }
}
