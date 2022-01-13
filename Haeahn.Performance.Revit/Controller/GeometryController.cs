using Autodesk.Revit.DB;
using System;
using System.Diagnostics;

namespace Haeahn.Performance.Revit
{
    class GeometryController
    {
        internal void AddCurvesAndSolids(Autodesk.Revit.DB.GeometryElement geometryElement, ref Curve curve, ref Solid solid)
        {
            GeometryController geometryController = new GeometryController();

            try
            {
                if (geometryElement != null)
                {
                    foreach (GeometryObject geometryObject in geometryElement)
                    {
                        Autodesk.Revit.DB.Curve rvt_curve = geometryObject as Autodesk.Revit.DB.Curve;
                        Autodesk.Revit.DB.Solid rvt_solid = geometryObject as Autodesk.Revit.DB.Solid;

                        if (rvt_curve != null)
                        {
                            curve = geometryController.ConvertRevitCurveToCurve(rvt_curve);
                        }
                        else if (rvt_solid != null)
                        {
                            solid = geometryController.ConvertRevitSolidToSolid(rvt_solid);
                        }
                        else
                        {
                            //If this GeometryObject is Instance, call AddCurvesAndSolids
                            Autodesk.Revit.DB.GeometryInstance geomInst = geometryObject as Autodesk.Revit.DB.GeometryInstance;
                            if (null != geomInst)
                            {
                                Autodesk.Revit.DB.GeometryElement transformedGeomElem = geomInst.GetInstanceGeometry(geomInst.Transform);
                                AddCurvesAndSolids(transformedGeomElem, ref curve, ref solid);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.Assert(false, ex.ToString());
                Log.WriteToFile(ex.ToString());
            }
        }
        //Period 가 없을 경우 Exception을 발생시키는 문제때문에 기본 Curve를 사용할 수 없음.
        internal Curve ConvertRevitCurveToCurve(Autodesk.Revit.DB.Curve rvt_curve)
        {
            Curve curve = new Curve();
            curve.GeometryType = "Curve";
            curve.IsBound = rvt_curve.IsBound;
            curve.IsCyclic = rvt_curve.IsCyclic;
            curve.Length = rvt_curve.Length;
            curve.IsClosed = rvt_curve.IsClosed;
            curve.Reference = (rvt_curve.Reference != null) ? ConvertRevitReferenceToReference(rvt_curve.Reference) : null;
            curve.ApproximateLength = rvt_curve.ApproximateLength;
            //Period 가 없을경우 Exception을 발생해서 Period는 수집하지 않는다.
            
            return curve;
        }

        internal Solid ConvertRevitSolidToSolid(Autodesk.Revit.DB.Solid rvt_solid)
        {
            Solid solid = new Solid();
            solid.GeometryType = "Solid";
            solid.SurfaceArea = rvt_solid.SurfaceArea;
            solid.Volume = rvt_solid.Volume;

            foreach(Autodesk.Revit.DB.Face rvt_face in rvt_solid.Faces)
            {
                solid.Faces.Add(ConvertRevitFaceToFace(rvt_face));
            }

            foreach(Autodesk.Revit.DB.Edge rvt_edge in rvt_solid.Edges)
            {
                solid.Edges.Add(ConvertRevitEdgeToEdge(rvt_edge));
            }

            return solid;
        }

        internal Face ConvertRevitFaceToFace(Autodesk.Revit.DB.Face rvt_face)
        {
            Face face = new Face();
            face.Id = rvt_face.Id;
            face.Area = rvt_face.Area;
            face.HasRegions = rvt_face.HasRegions;
            face.IsTwoSided = rvt_face.IsTwoSided;
            face.MaterialElementId = rvt_face.MaterialElementId.ToString();
            face.OrientationMatchesSurfaceOrientation = rvt_face.OrientationMatchesSurfaceOrientation;
            return face;
        }

        internal Edge ConvertRevitEdgeToEdge(Autodesk.Revit.DB.Edge rvt_edge)
        {
            Edge edge = new Edge();
            edge.ApproximateLength = rvt_edge.ApproximateLength;
            edge.Reference = (rvt_edge.Reference != null) ? ConvertRevitReferenceToReference(rvt_edge.Reference) : null;
            return edge;
        }

        internal Reference ConvertRevitReferenceToReference(Autodesk.Revit.DB.Reference rvt_reference)
        {
            Utilities utils = new Utilities();
            Reference reference = new Reference();
            reference.UVPoint = rvt_reference.UVPoint;
            reference.GlobalPoint = utils.PointToString(rvt_reference.GlobalPoint);
            reference.ElementReferenceType = rvt_reference.ElementReferenceType.ToString();
            reference.LinkedElementId = rvt_reference.LinkedElementId.ToString();
            reference.ElementId = rvt_reference.ElementId.ToString();

            return reference;
        }
    }
}
