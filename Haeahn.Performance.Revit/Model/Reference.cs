using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    class Reference
    {
        internal Reference()
        {
            this.UVPoint = new UV();
        }
        // Summary:
        //     The UV parameters of the reference, if the reference contains a face.
        //
        // Remarks:
        //     This value is valid only for references of type REFERENCE_TYPE_SURFACE. It is
        //     null for all other types.
        public UV UVPoint { get; set; }
        //
        // Summary:
        //     The position on which the reference is hit.
        //
        // Returns:
        //     null if the reference doesn't have a global point.
        //
        // Remarks:
        //     When using a plan view, the Z-value of a GlobalPoint is not meaningful.
        public string GlobalPoint { get; set; }
        //
        // Summary:
        //     The type of reference.
        public string ElementReferenceType { get; set; }
        //
        // Summary:
        //     The id of the top-level element in the linked document that is referred to by
        //     this reference.
        //
        // Remarks:
        //     InvalidElementId will be returned for references that don't refer to an element
        //     in a linked RVT file.
        public string LinkedElementId { get; set; }
        //
        // Summary:
        //     The element id for this reference.
        //
        // Remarks:
        //     InvalidElementId will be returned for references that don't refer to a particular
        //     element.
        public string ElementId { get; set; }
    }
}
