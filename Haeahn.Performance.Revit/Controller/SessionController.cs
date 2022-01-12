using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    class SessionController
    {
        internal Session GetSession(Employee employee, Project project = null)
        {
            var currentDateTime = DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"));

            Session session = new Session();
            
            session.UserId = employee.Id;
            session.UserName = employee.Name;
            session.StartTime = null;
            session.EndTime = null;

            if(project != null)
            {
                session.ProjectCode = project.Code;
                session.ProjectName = project.Name;
            }

            return session;
        }
    }
}
