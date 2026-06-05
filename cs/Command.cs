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
            var view = doc.ActiveView;


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

            var vertext = OpenDB_Click();
            DrawMeshWithVectorFacesTo3D(doc, vertext);
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
                        cmodel = new C5DModel((byte[])row[14]);
                        cmodel.C5DHeader((byte[])row[14]);
                    }
                }
                if (cmodel == null) return null;
            }
            return cmodel.vertexData;
        }

        Dictionary<string, Autodesk.Revit.DB.Material> matCache = new Dictionary<string, Autodesk.Revit.DB.Material>();
        public void DrawMeshWithVectorFacesTo3D(Document uiDoc, List<Vector3> vectorFaces)
        {
            // 1. 3D 형상을 지원하는 일반 모델(Generic Models) 카테고리 사용
            ElementId categoryId = new ElementId(BuiltInCategory.OST_GenericModel);
            ElementId materialId = ElementId.InvalidElementId; // 기본 재질 사용 (필요시 지정)

            TessellatedShapeBuilder builder = new TessellatedShapeBuilder();

            // 2. 전체 형상을 위한 페이스셋을 루프 외부에서 오픈
            builder.OpenConnectedFaceSet(true);
            // 3. 3개의 정점(Vector3)씩 묶어서 하나의 삼각형 면(Face)으로 추가
            for (int i = 0; i < vectorFaces.Count-2; i += 3)
            {
                double x = UnitUtils.ConvertToInternalUnits(vectorFaces[i].X, UnitTypeId.Meters);
                double y = UnitUtils.ConvertToInternalUnits(vectorFaces[i].Y, UnitTypeId.Meters);
                double z = UnitUtils.ConvertToInternalUnits(vectorFaces[i].Z, UnitTypeId.Meters);
                
                double x2 = UnitUtils.ConvertToInternalUnits(vectorFaces[i+1].X, UnitTypeId.Meters);
                double y2 = UnitUtils.ConvertToInternalUnits(vectorFaces[i+1].Y, UnitTypeId.Meters);
                double z2 = UnitUtils.ConvertToInternalUnits(vectorFaces[i+1].Z, UnitTypeId.Meters);
                
                double x3 = UnitUtils.ConvertToInternalUnits(vectorFaces[i+2].X, UnitTypeId.Meters);
                double y3 = UnitUtils.ConvertToInternalUnits(vectorFaces[i+2].Y, UnitTypeId.Meters);
                double z3 = UnitUtils.ConvertToInternalUnits(vectorFaces[i+2].Z, UnitTypeId.Meters);

                var vertex1 = new XYZ((float)x, (float)y, (float)z);
                var vertex2 = new XYZ((float)x2, (float)y2, (float)z2);
                var vertex3 = new XYZ((float)x3, (float)y3, (float)z3);

                //if (x < 1e-6 || y < 1e-6 || z < 1e-6)
                //    continue;
                // 데이터 개수가 부족하면 중단
                if (i + 2 >= vectorFaces.Count) break;

                // Vector3를 Revit의 XYZ 객체로 변환하여 리스트 생성
                List<XYZ> vertices = new List<XYZ>
                    {
                        vertex1,
                        vertex2,
                        vertex3
                    };

                // 루프 안에서는 면만 계속 추가
                builder.AddFace(new TessellatedFace(vertices, materialId));
            }

            // 4. 모든 면 추가가 끝나면 닫고 빌드
            builder.CloseConnectedFaceSet();
            builder.Build();

            // 5. 최종 결과물을 DirectShape로 단 한 번만 생성
            TessellatedShapeBuilderResult result = builder.GetBuildResult();

            // 트랜잭션 안에서 실행되어야 합니다.
            using (Transaction t = new Transaction(uiDoc, "Create Mesh"))
            {
                t.Start();
                DirectShape ds = DirectShape.CreateElement(uiDoc, categoryId);
                ds.SetShape(result.GetGeometricalObjects());
                t.Commit();
            }

            Autodesk.Revit.DB.Material GetOrCreateMaterial(Document doc, string name)
            {
                if (matCache.ContainsKey(name))
                    return matCache[name];

                var mat = new FilteredElementCollector(doc)
                    .OfClass(typeof(Autodesk.Revit.DB.Material))
                    .Cast<Autodesk.Revit.DB.Material>()
                    .FirstOrDefault(m => m.Name == name);

                if (mat == null)
                {
                    ElementId id = Autodesk.Revit.DB.Material.Create(doc, name);
                    mat = doc.GetElement(id) as Autodesk.Revit.DB.Material;
                }

                matCache[name] = mat;
                return mat;
            }
        }
    }
    }
