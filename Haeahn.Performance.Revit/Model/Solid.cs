using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    class Solid
    {
        internal Solid()
        {
            this.Faces = new List<Face>();
            this.Edges = new List<Edge>();
        }
        public string GeometryType { get; set; }
        public double SurfaceArea { get; set; }
        public double Volume { get; set; }
        public List<Face> Faces { get; set; }
        public List<Edge> Edges { get; set; }
    }
}
