using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    class Element
    {
        internal string Id { get; set; }
        internal string Name { get; set; }
        internal string ProjectCode { get; set; }
        internal string ProjectName { get; set; }
        internal string CategoryName { get; set; }
        internal string FamilyName { get; set; }
        internal string TypeName { get; set; }
        internal string Location { get; set; }
        internal string Geometry { get; set; }
        internal string Verticies { get; set; }
        internal string BoundingBox { get; set; }
        internal string InstanceParameter { get; set; }
        internal string TypeParameter { get; set; }
    }
}
