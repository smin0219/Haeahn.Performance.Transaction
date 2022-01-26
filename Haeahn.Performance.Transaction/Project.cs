using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace Haeahn.Performance.Transaction
{
    internal class Project
    {
        internal string Name { get; set; }
        internal string Code { get; set; }
        internal string Type { get; set; }

        internal Project(Autodesk.Revit.DB.ProjectInfo projectInfo)
        {
            CreateProject(projectInfo);
        }

        internal Project CreateProject(Autodesk.Revit.DB.ProjectInfo projectInfo)
        {
            if (ExternalApplication.rvt_doc != null)
            {
                this.Name = projectInfo.Name;
                this.Code = projectInfo.Number.ToString();
                this.Type = "기획설계";
            }
            return this;
        }
    }
}
