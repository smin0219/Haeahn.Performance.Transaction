using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    internal class Transaction
    {
        internal string EmployeeId { get; set; }
        internal string EmployeeName { get; set; }
        internal string ProjectCode { get; set; }
        internal string ElementId { get; set; }
        internal string ViewType { get; set; }
        internal string CategoryName { get; set; }
        internal string CategoryType { get; set; }
        internal string FamilyName { get; set; }
        public string TypeName { get; set; }
        public string Location { get; set; }
        public string Geometry { get; set; }
        public string Verticies { get; set; }
        internal string Difference { get; set; }
        internal string Event { get; set; }
        internal string EventDateTime { get; set; }
    }
}