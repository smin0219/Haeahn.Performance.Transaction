using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    internal class Project
    {
        internal string ProjectName { get; set; }
        internal string ProjectNumber {get;set;}
        internal Project()
        {
            this.ProjectName = ExternalApplication.rvt_doc.ProjectInformation.Name;
            this.ProjectNumber = ExternalApplication.rvt_doc.ProjectInformation.Number;
        }
    }
}
