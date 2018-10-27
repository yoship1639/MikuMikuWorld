using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Walker
{
    public class DigitalSignature
    {
        private ECDsa dsa;
        public byte[] PrivateKey { get; private set; }
        public byte[] PublicKey { get; private set; }

        private DigitalSignature(byte[] privateKey, byte[] publicKey)
        {
            PrivateKey = privateKey;
            PublicKey = publicKey;
        }

        public static DigitalSignature Generate()
        {
            var dsa = ECDsa.Create("ECDsaCng");
            dsa.GenerateKey(ECCurve.CreateFromFriendlyName("secp256k1"));
            var param = dsa.ExportParameters(true);

            var ds = new DigitalSignature(param.D, param.Q.X.Concat(param.Q.Y).ToArray());
            ds.dsa = dsa;

            return ds;
        }

        public static DigitalSignature FromKey(byte[] publicKey)
        {
            if (publicKey == null || publicKey.Length != 64) return null;

            var param = new ECParameters()
            {
                Curve = ECCurve.CreateFromFriendlyName("secp256k1"),
                Q = new ECPoint()
                {
                    X = publicKey.Take(32).ToArray(),
                    Y = publicKey.Skip(32).Take(32).ToArray(),
                },
            };

            var ds = new DigitalSignature(null, publicKey);

            try
            {
                ds.dsa = ECDsa.Create(param);
            }
            catch
            {
                return null;
            }

            return ds;
        }

        public static DigitalSignature FromKey(byte[] privateKey, byte[] publicKey)
        {
            if (privateKey == null || privateKey.Length != 32) return null;
            if (publicKey == null || publicKey.Length != 64) return null;

            var param = new ECParameters()
            {
                Curve = ECCurve.CreateFromFriendlyName("secp256k1"),
                D = privateKey,
                Q = new ECPoint()
                {
                    X = publicKey.Take(32).ToArray(),
                    Y = publicKey.Skip(32).Take(32).ToArray(),
                },
            };

            var ds = new DigitalSignature(privateKey, publicKey);

            try
            {
                ds.dsa = ECDsa.Create(param);
            }
            catch
            {
                return null;
            }

            return ds;
        }


        public byte[] Sign(byte[] data)
        {
            return dsa.SignData(data, HashAlgorithmName.SHA256);
        }

        public bool Verify(byte[] data, byte[] sign)
        {
            return dsa.VerifyData(data, sign, HashAlgorithmName.SHA256);
        }
    }
}
