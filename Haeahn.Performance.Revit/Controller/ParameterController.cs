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
        internal Dictionary<string, string> GetElementParameters(Autodesk.Revit.DB.Element element, bool isType)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            ElementType type = ExternalApplication.rvt_doc.GetElement(element.GetTypeId()) as ElementType;

            var orderedParameters = (isType && type != null) ? type.GetOrderedParameters() : element.GetOrderedParameters();

            foreach (Parameter parameter in orderedParameters)
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
