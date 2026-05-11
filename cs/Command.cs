#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
#endregion

namespace TemplateRevitCs
{
  [Transaction(TransactionMode.Manual)]
  public class Command : IExternalCommand
  {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = doc.ActiveView;

            double size = UnitUtils.ConvertToInternalUnits(1000.0, UnitTypeId.Millimeters);

            // =========================
            // 1. PIPE 미리 캐싱
            // =========================
            var pipes = new FilteredElementCollector(doc, view.Id)
                .OfCategory(BuiltInCategory.OST_PipeCurves)
                .WhereElementIsNotElementType()
                .ToList();

            Dictionary<ElementId, BoundingBoxXYZ> pipeBounds = new();

            foreach (var p in pipes)
            {
                var solid = GetSolid(p);
                if (solid == null) continue;

                pipeBounds[p.Id] = solid.GetBoundingBox();
            }

            // =========================
            // 2. VOXEL (메모리만 생성)
            // =========================
            List<(XYZ min, XYZ max)> voxels = new();

            int gridX = 160, gridY = 160, gridZ = 13;

            for (int x = 0; x < gridX; x++)
                for (int y = 0; y < gridY; y++)
                    for (int z = 0; z < gridZ; z++)
                    {
                        XYZ min = new XYZ(x * size, y * size, z * size);
                        XYZ max = new XYZ(min.X + size, min.Y + size, min.Z + size);

                        voxels.Add((min, max));
                    }

            // =========================
            // 3. 충돌 계산 (Revit 없이)
            // =========================
            List<(XYZ min, XYZ max)> hits = new();

            foreach (var v in voxels)
            {
                foreach (var pb in pipeBounds.Values)
                {
                    if (Intersect(v.min, v.max, pb))
                    {
                        hits.Add(v);
                        break;
                    }
                }
            }

            // =========================
            // 4. 결과만 Revit에 생성
            // =========================
            using (Transaction tx = new Transaction(doc, "Voxel Result"))
            {
                tx.Start();

                foreach (var v in hits)
                {
                    XYZ min = v.min;
                    XYZ max = v.max;

                    double dx = max.X - min.X;
                    double dy = max.Y - min.Y;
                    double dz = max.Z - min.Z;

                    if (dx < 1e-6 || dy < 1e-6 || dz < 1e-6)
                        continue;

                    XYZ p1 = new XYZ(v.min.X, v.min.Y, v.min.Z);
                    XYZ p2 = new XYZ(v.max.X, v.min.Y, v.min.Z);
                    XYZ p3 = new XYZ(v.max.X, v.max.Y, v.min.Z);
                    XYZ p4 = new XYZ(v.min.X, v.max.Y, v.min.Z);

                    CurveLoop loop = new CurveLoop();
                    loop.Append(Line.CreateBound(p1, p2));
                    loop.Append(Line.CreateBound(p2, p3));
                    loop.Append(Line.CreateBound(p3, p4));
                    loop.Append(Line.CreateBound(p4, p1));

                    Solid box = GeometryCreationUtilities.CreateExtrusionGeometry(
                        new List<CurveLoop> { loop },
                        XYZ.BasisZ,
                        v.max.Z - v.min.Z);

                    DirectShape ds = DirectShape.CreateElement(doc,
                        new ElementId(BuiltInCategory.OST_GenericModel));

                    ds.SetShape(new GeometryObject[] { box });
                }

                tx.Commit();
            }

            return Result.Succeeded;
        }

        Solid GetSolid(Element ds)
        {
            Options opt = new Options();
            opt.DetailLevel = ViewDetailLevel.Fine;
            opt.IncludeNonVisibleObjects = true;
            opt.ComputeReferences = false;

            GeometryElement geo = ds.get_Geometry(opt);
            if (geo == null) return null;

            foreach (GeometryObject obj in geo)
            {
                Solid s = obj as Solid;
                if (s != null && s.Volume > 0)
                    return s;
            }
            return null;
        }

        BoundingBoxXYZ ToWorldBBox(BoundingBoxXYZ box)
        {
            Transform t = box.Transform;

            XYZ min = t.OfPoint(box.Min);
            XYZ max = t.OfPoint(box.Max);

            return new BoundingBoxXYZ
            {
                Min = min,
                Max = max
            };
        }
        bool Intersect(XYZ minA, XYZ maxA, BoundingBoxXYZ b)
        {
            var wb = ToWorldBBox(b);

            return (minA.X <= wb.Max.X && maxA.X >= wb.Min.X) &&
                   (minA.Y <= wb.Max.Y && maxA.Y >= wb.Min.Y) &&
                   (minA.Z <= wb.Max.Z && maxA.Z >= wb.Min.Z);
        }

        Dictionary<string, Material> matCache = new Dictionary<string, Material>();

        Material GetOrCreateMaterial(Document doc, string name)
        {
            if (matCache.ContainsKey(name))
                return matCache[name];

            var mat = new FilteredElementCollector(doc)
                .OfClass(typeof(Material))
                .Cast<Material>()
                .FirstOrDefault(m => m.Name == name);

            if (mat == null)
            {
                ElementId id = Material.Create(doc, name);
                mat = doc.GetElement(id) as Material;
            }

            matCache[name] = mat;
            return mat;
        }
    }
}
