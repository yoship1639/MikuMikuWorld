using MikuMikuWorld.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MikuMikuWorld
{
    public static class Util
    {
        public static string ToBitmapString(Bitmap bitmap)
        {
            if (bitmap == null) return null;
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);

                return Convert.ToBase64String(ms.ToArray());
            }
        }
        public static Bitmap FromBitmapString(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return null;
            var data = Convert.FromBase64String(str);

            using (var ms = new MemoryStream(data))
            {
                return (Bitmap)Image.FromStream(ms);
            }
        }

        public static byte[] FromBitmap(Bitmap bitmap)
        {
            if (bitmap == null) return null;
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);

                return ms.ToArray();
            }
        }
        public static Bitmap ToBitmap(byte[] data)
        {
            if (data == null) return null;
            using (var ms = new MemoryStream(data))
            {
                return (Bitmap)Image.FromStream(ms);
            }
        }

        public static string ToCompString(byte[] data)
        {
            if (data == null) return null;
            var comp = Compress(data);
            return Convert.ToBase64String(comp);
        }
        public static byte[] FromCompString(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return null;
            var buf = Convert.FromBase64String(str);
            return Decompress(buf);
        }

        public static byte[] Compress(byte[] data)
        {
            if (data == null) return null;
            using (var ms = new System.IO.MemoryStream())
            {
                using (var cs = new DeflateStream(ms, CompressionMode.Compress, true))
                {
                    cs.Write(data, 0, data.Length);
                }
                return ms.ToArray();
            }
        }
        public static byte[] Decompress(byte[] data)
        {
            if (data == null) return null;
            using (var dst = new MemoryStream())
            {
                using (var ms = new MemoryStream(data))
                {
                    using (var cs = new DeflateStream(ms, CompressionMode.Decompress))
                    {
                        cs.CopyTo(dst);
                    }
                }
                return dst.ToArray();
            }
        }

        public static string ComputeHash(string path, int length = 8)
        {
            byte[] buf = null;
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                buf = md5.ComputeHash(fs);
                md5.Clear();
            }
            var res = new StringBuilder();
            foreach (byte b in buf)
            {
                res.Append(b.ToString("x2"));
            }

            return res.ToString().Substring(0, length);
        }
        public static string ComputeHash(byte[] data, int length = 8)
        {
            byte[] buf = null;
            var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            buf = md5.ComputeHash(data);
            md5.Clear();

            var res = new StringBuilder();
            foreach (byte b in buf)
            {
                res.Append(b.ToString("x2"));
            }

            return res.ToString().Substring(0, length);
        }
        public static string CreateHash(int length = 8)
        {
            byte[] random = new byte[256];
            var rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(random);
            rng.Dispose();

            return ComputeHash(random, length);
        }

        class IPAddressConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(IPAddress));
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString());
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return IPAddress.Parse((string)reader.Value);
            }
        }
        class IPEndPointConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(IPEndPoint));
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                IPEndPoint ep = (IPEndPoint)value;
                JObject jo = new JObject();
                jo.Add("Address", JToken.FromObject(ep.Address, serializer));
                jo.Add("Port", ep.Port);
                jo.WriteTo(writer);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                JObject jo = JObject.Load(reader);
                IPAddress address = jo["Address"].ToObject<IPAddress>(serializer);
                int port = (int)jo["Port"];
                return new IPEndPoint(address, port);
            }
        }

        public static string SerializeJson(object data, bool typenamehandling = true)
        {
            var setting = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>() { new IPAddressConverter(), new IPEndPointConverter() },
            };
            if (typenamehandling) setting.TypeNameHandling = TypeNameHandling.All;
            return JsonConvert.SerializeObject(data, Formatting.Indented, setting);
        }
        public static T DeserializeJson<T>(string json, bool typenamehandling = true)
        {
            var setting = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>() { new IPAddressConverter(), new IPEndPointConverter() },
            };
            if (typenamehandling) setting.TypeNameHandling = TypeNameHandling.All;
            return JsonConvert.DeserializeObject<T>(json, setting);
        }
        private static string ToReadable(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return json;
            int i = 0;
            int indent = 0;
            int quoteCount = 0;
            int position = -1;
            var sb = new StringBuilder();
            int lastindex = 0;
            while (true)
            {
                if (i > 0 && json[i] == '"' && json[i - 1] != '\\') quoteCount++;

                if (quoteCount % 2 == 0) //is not value(quoted)
                {
                    if (json[i] == '{' || json[i] == '[')
                    {
                        indent++;
                        position = 1;
                    }
                    else if (json[i] == '}' || json[i] == ']')
                    {
                        indent--;
                        position = 0;
                    }
                    else if (json.Length > i && json[i] == ',' && json[i + 1] == '"')
                    {
                        position = 1;
                    }
                    if (position >= 0)
                    {
                        sb.AppendLine(json.Substring(lastindex, i + position - lastindex));
                        sb.Append(new string(' ', indent * 4));
                        lastindex = i + position;
                        position = -1;
                    }
                }

                i++;
                if (json.Length <= i)
                {
                    sb.Append(json.Substring(lastindex));
                    break;
                }

            }
            return sb.ToString();
        }

        public static byte[] SerializeBson(object data)
        {
            using (var ms = new MemoryStream())
            {
                using (var bw = new BsonWriter(ms))
                {
                    var js = new JsonSerializer();
                    js.Serialize(bw, data);
                }

                return ms.ToArray();
            }
        }
        public static T DeserializeBson<T>(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                using (var br = new BsonReader(ms))
                {
                    var js = new JsonSerializer();
                    return js.Deserialize<T>(br);
                }
            }
        }

        public static byte[] SerializeJsonBinary(object data, bool typenamehandling = true)
        {
            byte[] buf = null;
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    var json = SerializeJson(data, typenamehandling);
                    var l = json.Length;
                    bw.Write(json);
                }
                buf = ms.ToArray();
            }

            return buf;
        }
        public static byte[] SerializeJsonBinaryCompress(object data, bool typenamehandling = true)
        {
            byte[] buf = null;
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    var json = SerializeJson(data, typenamehandling);
                    var l = json.Length;
                    bw.Write(json);
                }
                buf = Compress(ms.ToArray());
            }

            return buf;
        }
        public static T DeserializeJsonBinary<T>(byte[] data, bool typenamehandling = true)
        {
            using (var ms = new MemoryStream(data))
            {
                using (var br = new BinaryReader(ms))
                {
                    var str = br.ReadString();
                    return DeserializeJson<T>(str, typenamehandling);
                }
            }
        }
        public static T DeserializeJsonBinaryCompress<T>(byte[] data, bool typenamehandling = true)
        {
            using (var ms = new MemoryStream(Decompress(data)))
            {
                using (var br = new BinaryReader(ms))
                {
                    var str = br.ReadString();
                    return DeserializeJson<T>(str, typenamehandling);
                }
            }
        }

        public static string ToJson(this byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                using (var br = new BinaryReader(ms))
                {
                    return  br.ReadString();
                }
            }
        }

        public static Bitmap FromImageFile(string filepath)
        {
            try
            {
                var bitmap = (Bitmap)Image.FromFile(filepath);
                return bitmap;
            }
            catch { }

            try
            {
                using (FileStream fs = new FileStream(filepath, FileMode.Open))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        var tga = new TgaLib.TgaImage(br);
                        var bitmap = tga.GetBitmap().ToBitmap(PixelFormat.Format32bppPArgb);
                        return bitmap;
                    }
                }
            }
            catch { }

            return null;
        }

        public static Vector2 ToScreenPos(Vector3 pos, Matrix4 view, Matrix4 proj, int width, int height)
        {
            /*
            var p = new Vector4(pos, 1.0f);
            p = Vector4.Transform(p, view);
            p = Vector4.Transform(p, proj);
            p.X /= p.Z;
            p.Y /= p.Z;
            p.X = (p.X + 1) * width / 2;
            p.Y = (p.Y + 1) * height / 2;

            return new Vector2(p.X, p.Y);
            */

            var p = new Vector4(pos, 1.0f);
            p = Vector4.Transform(p, view);
            p = Vector4.Transform(p, proj);
            p = Vector4.Transform(p, CreateScreen(width, height));

            if (p.Z < 0.0f) return new Vector2(float.NaN);
            p.X /= p.W;
            p.Y /= p.W;
            return p.Xy;
            /*
            var vps = view * proj * CreateScreen(width, height);
            var v = new Vector4(0.0f, height, 0.0f, 1.0f) * Matrix4.CreateTranslation(pos) * vps;
            var p = v.Xy / v.W;

            return p;
            */
        }

        public static Matrix4 CreateScreen(float width, float height)
        {
            Matrix4 m = Matrix4.Identity;
            var w = width * 0.5f;
            var h = height * 0.5f;
            m.M11 = w;
            m.M22 = -h;
            m.M41 = w;
            m.M42 = h;

            return m;
        }

        private static Bitmap ToBitmap(this BitmapSource bitmapSource, System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            int width = bitmapSource.PixelWidth;
            int height = bitmapSource.PixelHeight;
            int stride = width * ((bitmapSource.Format.BitsPerPixel + 7) / 8);  // 行の長さは色深度によらず8の倍数のため
            IntPtr intPtr = IntPtr.Zero;
            try
            {
                intPtr = Marshal.AllocCoTaskMem(height * stride);
                bitmapSource.CopyPixels(new Int32Rect(0, 0, width, height), intPtr, height * stride, stride);
                using (var bitmap = new Bitmap(width, height, stride, pixelFormat, intPtr))
                {
                    // IntPtrからBitmapを生成した場合、Bitmapが存在する間、AllocCoTaskMemで確保したメモリがロックされたままとなる
                    // （FreeCoTaskMemするとエラーとなる）
                    // そしてBitmapを単純に開放しても解放されない
                    // このため、明示的にFreeCoTaskMemを呼んでおくために一度作成したBitmapから新しくBitmapを
                    // 再作成し直しておくとメモリリークを抑えやすい
                    return new Bitmap(bitmap);
                }
            }
            finally
            {
                if (intPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(intPtr);
            }
        }

        public static byte[] RNG(int length)
        {
            byte[] random = new byte[length];
            var rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(random);
            rng.Dispose();

            return random;
        }

        private static Random rand = new Random();
        public static int RandomInt(int min = 0, int max = int.MaxValue)
        {
            return rand.Next(min, max);
        }
        public static float RandomFloat()
        {
            return (float)rand.NextDouble();
        }

        private static readonly string Base58Digits = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
        public static string CreateBase58(int length)
        {
            return ToBase58(RNG(length)).Substring(0, length);
        }

        public static string ToBase58(byte[] array)
        {
            BigInteger data = 0;
            for (var i = 0; i < array.Length; i++)
            {
                data = data * 256 + array[i];
            }

            var sb = new StringBuilder();
            while (data > 0)
            {
                var index = (int)(data % 58);
                data /= 58;
                sb.Insert(0, Base58Digits[index]);
            }

            for (var i = 0; i < array.Length && array[i] == 0; i++)
            {
                sb.Insert(0, Base58Digits[0]);
            }

            return sb.ToString();
        }

        public static byte[] FromBase58(string str)
        {
            BigInteger data = 0;
            for (var i = 0; i < str.Length; i++)
            {
                var index = Base58Digits.IndexOf(str[i]);
                if (index < 0)
                    throw new FormatException(string.Format("Invalid Base58 character `{0}` at position {1}", str[i], i));
                data = data * 58 + index;
            }

            var zeroCount = str.TakeWhile(c => c == Base58Digits[0]).Count();
            var zeros = Enumerable.Repeat((byte)0, zeroCount);
            var bytes = data.ToByteArray().Reverse().SkipWhile(b => b == 0);

            return zeros.Concat(bytes).ToArray();
        }
    }
}
