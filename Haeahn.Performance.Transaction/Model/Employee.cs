using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Transaction
{
    class Employee
    {
        internal Employee() { }
        internal Employee(string employeeId)
        {
            this.Name = "sj.min";
            this.Id = employeeId;
            this.Department = "IT연구실";
        }
        //EmployeeId = 사원번호
        internal string Id { get; set; }
        //EmployeeName = 사원이름
        internal string Name { get; set; }
        //Department = 부서
        internal string Department { get; set; }
    }
}
