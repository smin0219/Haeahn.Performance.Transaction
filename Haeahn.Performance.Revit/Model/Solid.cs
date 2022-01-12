using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    class Solid
    {
        public string SurfaceArea { get; set; }
        public string Volume { get; set; }
        public List<Face> Face { get; set; }
        public List<Edge> Edge { get; set; }
    }
}
