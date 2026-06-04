using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace DDWorks_Shop_Designer.Database
{
    internal class C5DModel
    {
        public List<float> vertexData { get; }
        public C5DModel(byte[] data)
        {
            var ms = new MemoryStream(data);
            var br = new BinaryReader(ms);

            // ---------------------------
            // 1. HEADER 읽기 (예: 4개 int)
            // ---------------------------
            byte[] d = br.ReadBytes(1);
            int type = br.ReadInt32();
            int version = br.ReadInt32();
            int flags = br.ReadInt32();
            int size = br.ReadInt32();
            int count = 0;


            // 4바이트 씩이니까. size는 한번 읽는개수의 Size라 * 4 해야 Position과 동일해짐(Positon도 바이트)
            while (br.BaseStream.Position < size * 4 &&
                br.BaseStream.Position < br.BaseStream.Length) { 
                float vx = br.ReadSingle();
                float vy = br.ReadSingle();
                float vz = br.ReadSingle();
                float vnx = br.ReadSingle();
                float vny = br.ReadSingle();
                float vnz = br.ReadSingle();
                float vtx = br.ReadSingle();
                float vty = br.ReadSingle();
            count += 1;
            Debug.WriteLine($"vx:{vx} vy:{vy} vz:{vz}/vnx:{vnx} vny:{vny} vnz:{vnz}/vtx:{vtx} vty:{vty}");

            }

            Debug.WriteLine($"{count}");    
        }
    }
}
