using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    class ProjectManager
    {
        internal Project GetProject(string projectName, string projectCode)
        {
            Autodesk.Revit.DB.Document rvt_doc = ExternalApplication.rvt_doc;
            if(rvt_doc != null)
            {
                Project project = new Project();
                project.Name = projectName;
                project.Code = projectCode;
                return project;
            }
            else
            {
                Debug.Assert(false, "Autodesk.Revit.DB.Document rvt_doc is null");
                Log.WriteToFile("Autodesk.Revit.DB.Document rvt_doc is null");
                return null;
            }
        }
    }
}
