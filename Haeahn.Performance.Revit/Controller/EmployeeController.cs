using Autodesk.Revit.DB;
using Haeahn.Performance.Revit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    public class EmployeeController
    {
        internal Employee GetEmployee(string employeeId)
        {
            Employee employee = new Employee();
            employee.Name = "sj.min";
            employee.Id = employeeId;
            employee.Department = "IT연구실";
            return employee;
        }
    }
}
