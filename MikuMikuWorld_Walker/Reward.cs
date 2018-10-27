using MikuMikuWorld.Walker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    class Reward
    {
        public long Coin;
        public long Exp;
        public long SkillPoint;
        public Achivement Achivement;
        public string PublicKey;
        public string Sign;

        internal static Reward CreatePublicReward(long coin, long exp, string name, string display)
        {
            return CreateReward(coin, exp, 0, name, display, Achivement.PublicPub);
        }

        internal static Reward CreatePublicReward(long coin, long exp, long sp, string name, string display)
        {
            return CreateReward(coin, exp, sp, name, display, Achivement.PublicPub);
        }

        internal static Reward CreateReward(long coin, long exp, long sp, string name, string display, string pubKey)
        {
            var archive = new Achivement()
            {
                Name = name,
                DisplayName = display,
                Timestamp = DateTime.Now,
                PublicKey = pubKey,
            };

            var ph = "74e75754495ce815d3e402b7";
            var pd = "4/w+13O2Lot79PqnkRFG1D/tdyvaaig8iGp9By4RarTNzDRwb1XLZhum8NVCffYWCr2BIK3z+A5zzeO8RgtmFSTV4eQ+SpZqUWat2bq4z34=N270XW/OkiqMKI4g1yi1Tg==";
            var p = Encrypter.Decrypt(pd, ph);

            var ds = DigitalSignature.FromKey(Util.FromBase58(p), Util.FromBase58(pubKey));
            var json = Util.SerializeJsonBinary(archive, false);
            var sign = ds.Sign(json);

            archive.Sign = Util.ToBase58(sign);

            var rew = new Reward()
            {
                Achivement = archive,
                Coin = coin,
                Exp = exp,
                SkillPoint = sp,
                PublicKey = pubKey,
            };

            json = Util.SerializeJsonBinary(rew, false);
            sign = ds.Sign(json);

            rew.Sign = Util.ToBase58(sign);

            return rew;
        }

        public bool Verify()
        {
            try
            {
                if (PublicKey == null || Sign == null) return false;

                var rew = new Reward()
                {
                    Achivement = Achivement,
                    Coin = Coin,
                    Exp = Exp,
                    SkillPoint = SkillPoint,
                    PublicKey = PublicKey,
                };

                var json = Util.SerializeJsonBinary(rew, false);

                var ds = DigitalSignature.FromKey(Util.FromBase58(Achivement.PublicPub));
                return ds.Verify(json, Util.FromBase58(Sign));
            }
            catch { }

            return false;
        }
    }
}
