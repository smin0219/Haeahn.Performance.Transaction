using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    class Edge
    {
        public string Id { get; set; }
        public double ApproximateLength { get; set; }
        public List<XYZ> EndPoint { get; set; }
    }
}
