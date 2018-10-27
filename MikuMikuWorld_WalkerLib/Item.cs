using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Walker
{
    public class WalkerItemInfo
    {
        public string Hash;
        public string Name;
        public string Desc;

        public string Type;
        public int Price;
        public int MaxStack;
        public bool Consume;
        public bool Sync;

        private byte[] icon;
        public byte[] Icon
        {
            get { return icon; }
            set
            {
                icon = value;
                bitmap = Util.ToBitmap(value);
            }
        }

        [JsonIgnore]
        public Bitmap bitmap;

        public WalkerItemInfo Clone()
        {
            return new WalkerItemInfo()
            {
                Hash = Hash,
                Name = Name,
                Desc = Desc,
                Type = Type,
                Price = Price,
                MaxStack = MaxStack,
                Consume = Consume,
                Sync = Sync,
                bitmap = bitmap,
                icon = icon,
            };
        }
    }

    public class WalkerItem
    {
        public WalkerItemInfo Info;
        public int Number;
        public byte[] Status;
    }
}
