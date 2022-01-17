using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    class Utilities
    {
        const double EPSILON = 1.0e-9;
        public string PointArrayToString(IList<XYZ> points)
        {
            return string.Join(", ", points.Select<XYZ, string>(p => PointToString(p)));
        }
        public string PointToString(XYZ point)
        {
            return string.Format("[{0},{1},{2}]", point.X.ToString(), point.Y.ToString(), point.Z.ToString());
        }
        public string CurveTessellateToString(Autodesk.Revit.DB.Curve curve)
        {
            return PointArrayToString(curve.Tessellate());
        }
        public string LocationToString(Location location)
        {
            LocationPoint locationPoint = location as LocationPoint;
            LocationCurve locationCurve = location as LocationCurve;

            return (locationPoint == null ? ((locationCurve == null) ? null : CurveTessellateToString(locationCurve.Curve)) : PointToString(locationPoint.Point));
        }

        public Dictionary<string, string> GetBoundingBoxMaxMin(BoundingBoxXYZ boundingBox)
        {
            Dictionary< string, string > boundingBoxMaxMinPoint = new Dictionary<string, string>();
            boundingBoxMaxMinPoint.Add("Max", this.PointToString(boundingBox.Max));
            boundingBoxMaxMinPoint.Add("Min", this.PointToString(boundingBox.Min));

            return boundingBoxMaxMinPoint;
        }
        public List<XYZ> GetCanonicVerticies(Autodesk.Revit.DB.Element rvt_element)
        {
            Autodesk.Revit.DB.GeometryElement geoElement = rvt_element.get_Geometry(new Options());
            Autodesk.Revit.DB.Transform transform = Transform.Identity;
            Dictionary<XYZ, int> vertexLookup = new Dictionary<XYZ, int>();

            AddVertices(vertexLookup, transform, geoElement);

            List<XYZ> keys = new List<XYZ>(vertexLookup.Keys);

            keys.Sort(Compare);
            return keys;
        }
        public static bool IsZero(double a, double tolerance)
        {
            return tolerance > Math.Abs(a);
        }
        public static bool IsZero(double a)
        {
            return IsZero(a, EPSILON);
        }
        public static bool IsEqual(double a, double b)
        {
            return IsZero(b - a);
        }
        public static int Compare(double a, double b)
        {
            return IsEqual(a, b) ? 0 : (a < b ? -1 : 1);
        }
        public int Compare(XYZ p, XYZ q)
        {
            int d = Compare(p.X, q.X);

            if (0 == d)
            {
                d = Compare(p.Y, q.Y);

                if (0 == d)
                {
                    d = Compare(p.Z, q.Z);
                }
            }
            return d;
        }
        public void AddVertices(Dictionary<Autodesk.Revit.DB.XYZ, int> vertexLookup, Autodesk.Revit.DB.Transform t, Autodesk.Revit.DB.Solid s)
        {
            try
            {
                foreach (Autodesk.Revit.DB.Face f in s.Faces)
                {
                    Mesh m = f.Triangulate();

                    if (m != null)
                    {
                        foreach (XYZ p in m.Vertices)
                        {
                            XYZ q = t.OfPoint(p);
                            if (!vertexLookup.ContainsKey(q))
                            {
                                vertexLookup.Add(q, 1);
                            }
                            else
                            {
                                ++vertexLookup[q];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        public void AddVertices(Dictionary<Autodesk.Revit.DB.XYZ, int> vertexLookup, Autodesk.Revit.DB.Transform transform, Autodesk.Revit.DB.GeometryElement geoElement)
        {
            try
            {
                if (geoElement == null)
                {
                    return;
                }

                foreach (GeometryObject obj in geoElement)
                {
                    Autodesk.Revit.DB.Solid solid = obj as Autodesk.Revit.DB.Solid;

                    if (solid != null)
                    {
                        if (solid.Faces.Size > 0)
                        {
                            AddVertices(vertexLookup, transform, solid);
                        }
                    }
                    else
                    {
                        GeometryInstance instance = obj as GeometryInstance;

                        if (instance != null)
                        {
                            GeometryElement element = instance.GetSymbolGeometry();

                            Debug.Assert(instance.Transform != null, "GeometryInstance Transform is null");

                            if (element != null)
                            {
                                AddVertices(vertexLookup, instance.Transform.Multiply(transform), element);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
