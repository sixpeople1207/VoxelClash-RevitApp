using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace DDWorks_Shop_Designer.Database
{
    internal class C5DModel
    {
        public List<Vector3> vertexData { get; }
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
            int size = br.ReadInt32()*4; //4바이트
            int count = 0;

            vertexData = new List<Vector3>();
            int scale = 1000;
            // 4바이트 씩이니까. size는 한번 읽는개수의 Size라 * 4 해야 Position과 동일해짐(Positon도 바이트)
            while (br.BaseStream.Position < size &&
                br.BaseStream.Position < br.BaseStream.Length) { 
                float vx = br.ReadSingle();
                float vy = br.ReadSingle();
                float vz = br.ReadSingle();
                float vnx = br.ReadSingle();
                float vny = br.ReadSingle();
                float vnz = br.ReadSingle();
                float vtx = br.ReadSingle();
                float vty = br.ReadSingle();
               // vertexData.Add(new Vector3(vx* scale, vy* scale, vz * scale));
            count += 1;
           // Debug.WriteLine($"vx:{vx} vy:{vy} vz:{vz}/vnx:{0} vny:{0} vnz:{0}/vtx:{0} vty:{0}");

            }
            Debug.WriteLine($"{count}");
            //while (br.BaseStream.Position < br.BaseStream.Length)
            //{
            //    byte[] extraData = br.ReadBytes(16); // 예: 16바이트 추가 데이터
            //    Debug.WriteLine($"{extraData[0]}");
            //}
        }

        public void C5DHeader(byte[] data)
        {
            int count = 0;
            //var ms = new MemoryStream(data);
            //var br = new BinaryReader(ms);
            //var ushort_3 = br.ReadUInt16();
            //var ushort_4 = br.ReadUInt16();
            //var ushort_5 = br.ReadUInt16();
            //var byte_1 = br.ReadByte();
            //var byte_2 = br.ReadByte();
            //var byte_3 = br.ReadByte();
            //var byte_4 = br.ReadByte();
            //var uint_0 = br.ReadUInt32();
            //var uint_1 = br.ReadUInt32();
            //var ushort_6 = br.ReadUInt16();
            //var modelByte = br.ReadBytes(ushort_6*4);
            //int scale = 1000;

            var mmm = new MemoryStream(data);
            var binaryReader = new BinaryReader(mmm);

            var int_0 = binaryReader.ReadInt32();
            var int_1 = binaryReader.ReadInt32();
            var int_2 = binaryReader.ReadInt32();
            var int_3 = binaryReader.ReadInt32();
            int num = int_1;
            List<long> list_0 = new List<long>(num);
            for (int i = 0; i < num; i++)
            {
                list_0.Add(binaryReader.ReadInt64());
            }
            Debug.WriteLine($"{count}");

        }
    }

    namespace ns22
    {
        // Token: 0x020006F5 RID: 1781
        internal class Class1650
        {
            // Token: 0x060044E7 RID: 17639 RVA: 0x0002EE73 File Offset: 0x0002D073
            public Class1650()
            {
                this.list_0 = new List<Class1650>();
            }

            // Token: 0x060044E8 RID: 17640 RVA: 0x0002EE86 File Offset: 0x0002D086
            public Class1650(Class1650 record)
            {
                this.ushort_1 = record.ushort_1;
                this.ushort_2 = record.ushort_2;
                this.byte_0 = record.byte_0;
                this.list_0 = record.list_0;
            }

            // Token: 0x060044E9 RID: 17641 RVA: 0x00007BDE File Offset: 0x00005DDE
            public virtual void vmethod_0()
            {
            }



            // Token: 0x060044EB RID: 17643 RVA: 0x0013D858 File Offset: 0x0013BA58
            public static Class1650 smethod_0(Stream stream)
            {
                BinaryReader binaryReader = new BinaryReader(stream);
                Class1650 @class = new Class1650();
                @class.ushort_1 = binaryReader.ReadUInt16();
                @class.ushort_2 = binaryReader.ReadUInt16();
                @class.byte_0 = binaryReader.ReadBytes((int)@class.ushort_2);
                return @class;
            }

            // Token: 0x170013DB RID: 5083
            // (get) Token: 0x060044EC RID: 17644 RVA: 0x0013D8A0 File Offset: 0x0013BAA0
            public int FullSize
            {
                get
                {
                    int num = (int)(4 + this.ushort_2);
                    foreach (Class1650 @class in this.list_0)
                    {
                        num += (int)(4 + @class.ushort_2);
                    }
                    return num;
                }
            }

            // Token: 0x170013DC RID: 5084
            // (get) Token: 0x060044ED RID: 17645 RVA: 0x0013D904 File Offset: 0x0013BB04
            public int TotalSize
            {
                get
                {
                    int num = (int)this.ushort_2;
                    foreach (Class1650 @class in this.list_0)
                    {
                        num += (int)@class.ushort_2;
                    }
                    return num;
                }
            }

            // Token: 0x170013DD RID: 5085
            // (get) Token: 0x060044EE RID: 17646 RVA: 0x0013D964 File Offset: 0x0013BB64
            public byte[] AllData
            {
                get
                {
                    if (this.list_0.Count == 0)
                    {
                        return this.byte_0;
                    }
                    List<byte> list = new List<byte>(this.TotalSize);
                    list.AddRange(this.byte_0);
                    foreach (Class1650 @class in this.list_0)
                    {
                        list.AddRange(@class.AllData);
                    }
                    return list.ToArray();
                }
            }

            // Token: 0x060044EF RID: 17647 RVA: 0x0013D9F0 File Offset: 0x0013BBF0
            public static object smethod_1(uint value)
            {
                bool flag = (value & 1U) == 1U;
                if ((value & 2U) == 0U)
                {
                    ulong data = (ulong)(value & 4294967292U) << 32;
                    double num = Class1650.smethod_2(data);
                    if (flag)
                    {
                        num /= 100.0;
                    }
                    return num;
                }
                int num2 = (int)(value & 4294967292U) >> 2;
                if (flag)
                {
                    return num2 / 100m;
                }
                return num2;
            }

            // Token: 0x060044F0 RID: 17648 RVA: 0x0013DA5C File Offset: 0x0013BC5C
            public static double smethod_2(ulong data)
            {
                byte[] bytes = BitConverter.GetBytes(data);
                return BitConverter.ToDouble(bytes, 0);
            }

            // Token: 0x060044F1 RID: 17649 RVA: 0x0013DA78 File Offset: 0x0013BC78
            public void method_0(BinaryWriter writer)
            {
                writer.Write(this.ushort_1);
                writer.Write(this.ushort_2);
                if (this.ushort_2 > 0)
                {
                    writer.Write(this.byte_0);
                    if (this.list_0.Count > 0)
                    {
                        foreach (Class1650 @class in this.list_0)
                        {
                            writer.Write(@class.ushort_1);
                            writer.Write(@class.ushort_2);
                            writer.Write(@class.byte_0);
                        }
                    }
                }
            }

          

            // Token: 0x0400216B RID: 8555
            public const ushort ushort_0 = 8224;

            // Token: 0x0400216C RID: 8556
            public ushort ushort_1;

            // Token: 0x0400216D RID: 8557
            public ushort ushort_2;

            // Token: 0x0400216E RID: 8558
            public byte[] byte_0;

            // Token: 0x0400216F RID: 8559
            public List<Class1650> list_0;
        }
    }
    namespace ns22
    {
        // Token: 0x0200070E RID: 1806
        internal sealed class Class1666 : Class1650
        {
            // Token: 0x060045AF RID: 17839 RVA: 0x0002EEE8 File Offset: 0x0002D0E8
            public Class1666(Class1650 record) : base(record)
            {
            }

            // Token: 0x060045B0 RID: 17840 RVA: 0x0002F564 File Offset: 0x0002D764
            public Class1666()
            {
                this.ushort_1 = 60;
            }
        }
    }
}
