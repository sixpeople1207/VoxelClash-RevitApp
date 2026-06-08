using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace myApp.Drawing
{
    internal class Modeling
    {
        /// <summary>
        /// Vector3 리스트를 받아서 Revit의 3D 형상으로 변환하여 DirectShape로 생성하는 메서드입니다.
        /// </summary>
        /// <param name="uiDoc"></param>
        /// <param name="vectorFaces"></param>
        public void DrawMeshWithVectorFacesTo3D(Document uiDoc, List<Vector3> vectorFaces)
        {
            // 1. 3D 형상을 지원하는 일반 모델(Generic Models) 카테고리 사용
            ElementId categoryId = new ElementId(BuiltInCategory.OST_GenericModel);
            ElementId materialId = ElementId.InvalidElementId; // 기본 재질 사용 (필요시 지정)

            TessellatedShapeBuilder builder = new TessellatedShapeBuilder();

            // 2. 전체 형상을 위한 페이스셋을 루프 외부에서 오픈
            builder.OpenConnectedFaceSet(true);
            // 3. 3개의 정점(Vector3)씩 묶어서 하나의 삼각형 면(Face)으로 추가
            for (int i = 0; i < vectorFaces.Count - 2; i += 3)
            {
                // 데이터 개수가 부족하면 중단
                if (i + 2 >= vectorFaces.Count) break;

                double x = UnitUtils.ConvertToInternalUnits(vectorFaces[i].X, UnitTypeId.Meters);
                double y = UnitUtils.ConvertToInternalUnits(vectorFaces[i].Y, UnitTypeId.Meters);
                double z = UnitUtils.ConvertToInternalUnits(vectorFaces[i].Z, UnitTypeId.Meters);

                double x2 = UnitUtils.ConvertToInternalUnits(vectorFaces[i + 1].X, UnitTypeId.Meters);
                double y2 = UnitUtils.ConvertToInternalUnits(vectorFaces[i + 1].Y, UnitTypeId.Meters);
                double z2 = UnitUtils.ConvertToInternalUnits(vectorFaces[i + 1].Z, UnitTypeId.Meters);

                double x3 = UnitUtils.ConvertToInternalUnits(vectorFaces[i + 2].X, UnitTypeId.Meters);
                double y3 = UnitUtils.ConvertToInternalUnits(vectorFaces[i + 2].Y, UnitTypeId.Meters);
                double z3 = UnitUtils.ConvertToInternalUnits(vectorFaces[i + 2].Z, UnitTypeId.Meters);

                var vertex1 = new XYZ((float)x, (float)y, (float)z);
                var vertex2 = new XYZ((float)x2, (float)y2, (float)z2);
                var vertex3 = new XYZ((float)x3, (float)y3, (float)z3);

                //if (x < 1e-6 || y < 1e-6 || z < 1e-6)
                //    continue;


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

            if (vectorFaces.Count > 0)
            {
                // 4. 모든 면 추가가 끝나면 닫고 빌드
                builder.CloseConnectedFaceSet();
                builder.Build();
            }

            // 5. 최종 결과물을 DirectShape로 단 한 번만 생성
            TessellatedShapeBuilderResult result = builder.GetBuildResult();

            // 트랜잭션 안에서 실행되어야 합니다.
            using (Transaction t = new Transaction(uiDoc, "Create Mesh"))
            {
                t.Start();
                DirectShape ds = DirectShape.CreateElement(uiDoc, categoryId);
                if (result != null)
                    ds.SetShape(result.GetGeometricalObjects());
                else
                {
                    MessageBox.Show("객체가 Null입니다.");
                }
                t.Commit();
            }
           
        }
    }
}
