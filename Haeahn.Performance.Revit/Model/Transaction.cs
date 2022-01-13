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
        internal string ElementId { get; set; }
        internal string ElementName { get; set; }
        internal string ProjectCode { get; set; }
        internal string CategoryType { get; set; }
        internal string Difference { get; set; }
        public string EventType { get; set; }
        internal string EventDateTime { get; set; }
    }
}