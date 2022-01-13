using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    class Curve
    {

        public string GeometryType { get; set; }
        public bool IsBound { get; set; }
        public bool IsCyclic { get; set; }
        public double Length { get; set; }
        public bool IsClosed { get; set; }
        public Reference Reference { get; set; }
        public double ApproximateLength { get; set; }
    }
}
