using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    class ParameterManager
    {
        internal List<KeyValuePair<string, string>> GetParameters(Autodesk.Revit.DB.Element element)
        {
            List<KeyValuePair<string, string>> parameterList = new List<KeyValuePair<string, string>>();

            foreach (Parameter parameter in element.GetOrderedParameters())
            {
                string key = parameter.Definition.Name;
                string value = parameter.AsValueString();

                parameterList.Add(new KeyValuePair<string, string>( key, value));
            }

            return parameterList;
        }
    }
}
