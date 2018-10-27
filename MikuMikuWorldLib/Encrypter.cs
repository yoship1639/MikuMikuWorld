using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    public static class Encrypter
    {
        public static string Encrypt(string text, string key)
        {
            var aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.KeySize = key.Length * 8;
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();

            var buf = Encoding.Unicode.GetBytes(text);

            using (var ms = new MemoryStream())
            {
                using (var cs = new DeflateStream(ms, CompressionMode.Compress, true))
                {
                    cs.Write(buf, 0, buf.Length);
                }
                buf = ms.ToArray();
            }
   
            string res;
            using (ICryptoTransform encrypt = aes.CreateEncryptor())
            {
                byte[] dest = encrypt.TransformFinalBlock(buf, 0, buf.Length);
                res =  Convert.ToBase64String(dest);
            }

            var iv = Convert.ToBase64String(aes.IV);
            var l = iv.Length;
            res += iv;

            return res;
        }

        public static string Decrypt(string text, string key)
        {
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.KeySize = key.Length * 8;
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            var s = text.Substring(text.Length - 24, 24);
            var b = Convert.FromBase64String(s);
            aes.IV = b;

            var buf = Convert.FromBase64String(text.Remove(text.Length - 24, 24));

            byte[] dst = null;
            using (ICryptoTransform decrypt = aes.CreateDecryptor())
            {
                dst = decrypt.TransformFinalBlock(buf, 0, buf.Length);
            }

            using (var ms = new MemoryStream(dst))
            {
                using (var ms2 = new MemoryStream())
                using (var cs = new DeflateStream(ms, CompressionMode.Decompress))
                {
                    while (true)
                    {
                        var br = cs.ReadByte();
                        if (br == -1) break;
                        ms2.WriteByte((byte)br);
                    }
                    dst = ms2.ToArray();
                }
            }

            return Encoding.Unicode.GetString(dst);
        }
    }
}
