using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Walker
{
    public class Achivement
    {
        public static readonly string PublicPub = "3KNMmwCQLu4gFtjHCf7tR3mL2BhbbchyYRRtwaXtDRraX1AcpVGqmfD4B3oopCU2V8S262dgCxNfKM6ySW4MDfmD";

        public string Name { get; set; }
        public string DisplayName;
        public DateTime Timestamp;

        public string Sign;
        public string PublicKey;

        public Achivement Clone()
        {
            return new Achivement()
            {
                Name = Name,
                DisplayName = DisplayName,
                Timestamp = Timestamp,
                PublicKey = PublicKey,
            };
        }

        public bool Verify()
        {
            try
            {
                if (PublicKey == null || Sign == null) return false;

                var clone = Clone();
                var json = Util.SerializeJsonBinary(clone, false);

                var ds = DigitalSignature.FromKey(Util.FromBase58(PublicKey));
                return ds.Verify(json, Util.FromBase58(Sign));
            }
            catch { }

            return false;
        }
    }
}
