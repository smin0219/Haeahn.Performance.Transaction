using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    internal class Session
    {
        internal Session(Employee employee, Project project = null)
        {
            CreateSession(employee, project);
        }
        internal string ProjectCode { get; set; }
        internal string ProjectName { get; set; }
        internal string UserId { get; set; }
        internal string UserName { get; set; }
        internal string StartTime { get; set; }
        internal string EndTime { get; set; }

        internal Session CreateSession(Employee employee, Project project = null)
        {
            this.UserId = employee.Id;
            this.UserName = employee.Name;
            this.StartTime = null;
            this.EndTime = null;

            if (project != null)
            {
                this.ProjectCode = project.Code;
                this.ProjectName = project.Name;
            }
            return this;
        }
    }
}
