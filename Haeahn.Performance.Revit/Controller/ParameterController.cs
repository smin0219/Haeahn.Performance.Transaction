using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    class ParameterController
    {
        internal Dictionary<string, string> GetParameters(Autodesk.Revit.DB.Element element)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            foreach (Parameter parameter in element.GetOrderedParameters())
            {
                var key = parameter.Definition.Name;
                var value = parameter.AsValueString();

                if (!parameters.ContainsKey(key))
                {
                    parameters.Add(key, value);
                }
                // 딕셔너리가 키를 포함하고 있으면 값이 있는 것을 우선으로 삽입
                else
                {
                    if (string.IsNullOrEmpty(parameters[key]))
                    {
                        parameters[key] = value;
                    }
                }
            }

            return parameters;
        }
    }
}
