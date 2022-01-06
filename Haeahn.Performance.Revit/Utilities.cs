﻿using Autodesk.Revit.DB;
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
            return string.Format("{0},{1},{2}", point.X.ToString(), point.Y.ToString(), point.Z.ToString());
        }

        public string CurveTessellateToString(Curve curve)
        {
            return PointArrayToString(curve.Tessellate());
        }

        public string LocationToString(Location location)
        {
            LocationPoint locationPoint = location as LocationPoint;
            LocationCurve locationCurve = location as LocationCurve;

            return (locationPoint == null ? ((locationCurve == null) ? null : CurveTessellateToString(locationCurve.Curve)) : PointToString(locationPoint.Point));
        }

        public string BuildJson(BoundingBoxXYZ boundingBox)
        {
            try
            {
                return "{" + string.Format("\"Max\": \"{0}\", \"Min\": \"{1}\"", PointToString(boundingBox.Max), PointToString(boundingBox.Min)) + "}";
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Dictionary<string, string> GetBoundingBoxPoint(BoundingBoxXYZ boundingBox)
        {
            return new Dictionary<string, string> { { "Max", this.PointToString(boundingBox.Max) }, { "Min", this.PointToString(boundingBox.Min) } };
        }
        public List<XYZ> GetCanonicVerticies(Element element)
        {
            GeometryElement geoElement = element.get_Geometry(new Options());
            Transform transform = Transform.Identity;
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
        public string BuildJson(IList<Parameter> parameters)
        {
            List<string> properties = new List<string>();
            foreach (Parameter param in parameters)
            {
                properties.Add(string.Format("\"{0}\":\"{1}\"", param.Definition.Name, param.AsValueString()));
            }
            properties.Sort();
            return "{" + string.Join(",", properties) + "}";
        }
        public void AddVertices(Dictionary<XYZ, int> vertexLookup, Transform t, Solid s)
        {
            try
            {
                //  Debug.Assert(0 < s.Edges.Size,
                //"expected a non-empty solid");

                if (!(s.Edges.Size > 0))
                {
                    foreach (Face f in s.Faces)
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


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        public void AddVertices(Dictionary<XYZ, int> vertexLookup, Transform transform, GeometryElement geoElement)
        {
            try
            {
                if (geoElement == null)
                {
                    return;
                }

                foreach (GeometryObject obj in geoElement)
                {
                    Solid solid = obj as Solid;

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
