#region Namespaces
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DDWorks_Shop_Designer.Database;
using System.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dinno.UserManager.Models;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using myApp;
using myApp.UI;
using myApp.Drawing;
using myApp.Database;
#endregion

namespace TemplateRevitCs
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        UIDbInformation ui = null;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            var view = doc.ActiveView;
            Init(uidoc);
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

            //var vertext = OpenDB_Click();
            //DrawMeshWithVectorFacesTo3D(doc, vertext);
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
        private List<Vector3> OpenDB_Click()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            C5DModel cmodel = null;
            openFileDialog.Filter = "Database files (*.db)|*.db|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string dbPath = openFileDialog.FileName;

                DatabaseManager dbManager = new DatabaseManager("D:\\8. 자료\\C5D자료\\sample.db");
                //var table = dbManager.GetTableNames();
                //foreach (var t in table)
                //{
                //    //Debug.WriteLine("Table Name: " + t);
                //}


                DataTable dt = dbManager.ReadToDataTable("GSysSampleProje_Geometries");
                foreach (DataRow row in dt.Rows)
                {
                    var name = row[4];
                    if (name.ToString().Contains("DIAPHRAGM VALVE 1_2__geom"))
                    {
                        var data = row[14];
                        cmodel = new C5DModel((byte[])row[14],0);
                       // cmodel.C5DHeader((byte[])row[14]);
                    }
                }
                if (cmodel == null) return null;
            }
            return cmodel.vertexData;
        }

        private void Init(UIDocument uiDoc)
        {
            // 초기화 작업 (예: UI 설정, 데이터 로드 등)
            ui = new UIDbInformation(uiDoc);
            ui.textBoxShift.Text = "0";
            ui.textBoxDBPath.Text = "D:\\8. 자료\\C5D자료\\sample.db";
           // ui.textBoxName.Text = "DIAPHRAGM VALVE 1_2__geom";
            ui.ShowDialog();
        }
        Dictionary<string, Autodesk.Revit.DB.Material> matCache = new Dictionary<string, Autodesk.Revit.DB.Material>();


    }
}
