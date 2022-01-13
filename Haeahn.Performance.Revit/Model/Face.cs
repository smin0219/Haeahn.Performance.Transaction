using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    class Face
    {
        public int Id { get; set; }
        public double Area { get; set; }
        public bool HasRegions { get; set; }
        public bool IsTwoSided { get; set; }
        public string MaterialElementId { get; set; }
        public bool OrientationMatchesSurfaceOrientation { get; set; }
    }
}
