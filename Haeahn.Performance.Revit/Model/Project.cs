using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    internal class Project
    {
        internal string Name { get; set; }
        internal string Code {get;set;}

        internal Project(ProjectInfo projectInfo)
        {
            CreateProject(projectInfo);
        }

        internal Project CreateProject(ProjectInfo projectInfo)
        {
            if(ExternalApplication.rvt_doc != null)
            {
                this.Name = projectInfo.Name;
                this.Code = projectInfo.Number.ToString();
            }
            return this;
        }
    }
}
