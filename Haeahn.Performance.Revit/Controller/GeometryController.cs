using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit 
{
    class GeometryController
    {
        internal Haeahn.Performance.Revit.Solid GetSolid(Autodesk.Revit.DB.GeometryElement geometryElement)
        {
            Haeahn.Performance.Revit.Solid solid = new Haeahn.Performance.Revit.Solid();

            foreach (GeometryObject geometryObject in geometryElement)
            {
                Autodesk.Revit.DB.Solid rvt_solid = geometryObject as Autodesk.Revit.DB.Solid;
                if (rvt_solid == null || rvt_solid.Faces.Size == 0 || rvt_solid.Edges.Size == 0)
                {
                    continue;
                }

                solid.SurfaceArea = rvt_solid.SurfaceArea.ToString();
                solid.Volume = rvt_solid.Volume.ToString();
                solid.Face = new List<Face>();

                foreach (Autodesk.Revit.DB.Face rvt_face in rvt_solid.Faces)
                {
                    Haeahn.Performance.Revit.Face face = new Haeahn.Performance.Revit.Face();
                    face.Id = rvt_face.Id.ToString();
                    face.Area = rvt_face.Area.ToString();
                    face.HasRegions = rvt_face.HasRegions.ToString();
                    face.IsTwoSided = rvt_face.IsTwoSided.ToString();
                    face.MaterialElementId = rvt_face.MaterialElementId.ToString();
                    face.OrientationMatchesSurfaceOrientation = rvt_face.OrientationMatchesSurfaceOrientation.ToString();
                    solid.Face.Add(face);
                }

                solid.Edge = new List<Haeahn.Performance.Revit.Edge>();

                foreach (Autodesk.Revit.DB.Edge rvt_edge in rvt_solid.Edges)
                {
                    Edge edge = new Edge();
                    edge.Id = rvt_edge.Id.ToString();
                    edge.ApproximateLength = rvt_edge.ApproximateLength;
                    edge.EndPoint = new List<XYZ> { rvt_edge.AsCurve().GetEndPoint(0), rvt_edge.AsCurve().GetEndPoint(1) };
                    solid.Edge.Add(edge);
                }
            }
            return solid;
        }
    }
}
