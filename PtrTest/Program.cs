using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PtrTest
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern unsafe void CopyMemory(void* dst, void* src, int size);

        static void Main(string[] args)
        {
            float[] data = new float[] { 1.0f, 2.0f, 3.0f };
            Stopwatch sw = new Stopwatch();

            Vector3[] vecs = new Vector3[1000000];
            for (int i = 0; i < 1000000; i++) vecs[i] = new Vector3();

            sw.Start();
            for (int i = 0; i < 1000000; i++)
            {
                vecs[i].X = 1.0f;
                vecs[i].Y = 2.0f;
                vecs[i].Z = 3.0f;
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            sw.Reset();

            sw.Start();
            for (int i = 0; i < 1000000; i++)
            {
                vecs[i] = new Vector3()
                {
                    X = 1.0f,
                    Y = 2.0f,
                    Z = 3.0f,
                };
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            sw.Reset();

            /*
            sw.Start();
            IntPtr ptr = IntPtr.Zero;
            float[] data = new float[] {1.0f, 2.0f, 3.0f };
            for (int i = 0; i < 1000000; i++)
            {
                Marshal.StructureToPtr(vecs[i], ptr, false);
                Marshal.Copy(data, 0, ptr, 3);
            }
            sw.Stop();

            Console.WriteLine(sw.Elapsed);
            */

            sw.Start();
            for (int i = 0; i < 1000000; i++)
            {
                unsafe
                {
                    fixed (float* src = data)
                    fixed (Vector3* dst = &vecs[i])
                    {
                        CopyMemory(dst, src, 12);
                    }
                }
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            sw.Reset();

            sw.Start();
            for (int i = 0; i < 1000000; i++)
            {
                vecs[i].Set(1.0f, 2.0f, 3.0f);
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            sw.Reset();

            sw.Start();
            unsafe
            {
                var v = new Vector3(2.0f, 4.0f, 6.0f);
                fixed (Vector3* dst = &vecs[0])
                {
                    Vector3* p = dst;
                    for (var i = 0; i < 1000000; i++)
                    {
                        *p = v;
                        p++;
                    }
                }
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            sw.Reset();

            Console.ReadLine();
        }
    }

    struct Vector3
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public void Set(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
