using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    public class EmployeeManager
    {
        internal Employee GetCurrentUser()
        {
            var employee = new Employee();
            employee.EmployeeName = ExternalApplication.rvt_uiapp.Application.Username;
            employee.EmployeeId = "20210916";
            employee.Department = "IT연구실";
            return employee;
        }
    }
}
