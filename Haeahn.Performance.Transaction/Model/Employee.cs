using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Transaction.Model
{
    class Employee
    {
        internal Employee() { }
        internal Employee(string name, string employeeId, string department)
        {
            this.Name = name;
            this.Id = employeeId;
            this.Department = department;
        }
        //EmployeeId = 사원번호
        internal string Id { get; set; }
        //EmployeeName = 사원이름
        internal string Name { get; set; }
        //Department = 부서
        internal string Department { get; set; }
    }
}
