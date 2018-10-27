using MikuMikuWorld.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Scripts
{
    class WalkerObjectInfo : GameComponent
    {
        public WorldObject Object;

        public WalkerObjectInfo(WorldObject wo)
        {
            Object = wo;
        }

        protected override RequestResult<T> OnReceivedRequest<T>(string request, params object[] args)
        {
            if (request == "property")
            {
                if (Object.Properties == null) return null;
                var val = "";
                var res = Object.Properties.TryGetValue(args[0] as string, out val);
                if (res) return new RequestResult<T>(this, (T)(object)val);
            }
            else if (request == "properties")
            {
                return new RequestResult<T>(this, (T)(object)Object.Properties);
            }

            return null;
        }
    }
}
