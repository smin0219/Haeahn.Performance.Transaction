using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    class SessionManager
    {
        internal Session GetSession(Project project, Employee employee)
        {
            var currentDateTime = DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"));

            Session session = new Session();
            session.ProjectCode = project.Code;
            session.ProjectName = project.Name;
            session.UserId = employee.Id;
            session.UserName = employee.Name;
            session.StartTime = null;
            session.EndTime = null;

            return session;
        }
    }
}
