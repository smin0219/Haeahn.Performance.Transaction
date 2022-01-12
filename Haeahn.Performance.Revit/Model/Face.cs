using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    class Face
    {
        public string Id { get; set; }
        public string Area { get; set; }
        public string HasRegions { get; set; }
        public string IsTwoSided { get; set; }
        public string MaterialElementId { get; set; }
        public string OrientationMatchesSurfaceOrientation { get; set; }
    }
}
