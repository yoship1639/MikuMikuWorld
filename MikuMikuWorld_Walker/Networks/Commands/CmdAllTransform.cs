using MikuMikuWorld.Network;
using MikuMikuWorld.Scripts;
using MikuMikuWorld.Walker;
using MikuMikuWorldScript;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Networks.Commands
{
    class CmdAllTransform : ICmd
    {
        private WalkerScript walker;
        private WorldData worldData;

        public CmdAllTransform(WalkerScript walker)
        {
            this.walker = walker;
            worldData = MMW.GetAsset<WorldData>();
        }

        public int[] ExecutableDataTypes => new int[]
        {
            DataType.ResponseAllTransform
        };

        public bool Execute(Server server, int dataType, byte[] data, bool isTcp)
        {
            if (isTcp) return false;

            var ids = new List<int>();
            var poses = new List<Vector3f>();
            var rots = new List<Vector3f>();

            Buffer.Read(data, br =>
            {
                while (br.BaseStream.Position < data.Length)
                {
                    ids.Add(br.ReadInt32());
                    poses.Add(new Vector3f(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()));
                    rots.Add(new Vector3f(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()));
                }
            });

            for (var i = 0; i < ids.Count; i++)
            {
                var player = worldData.Players.Find(p => p.SessionID == ids[i]);
                if (player == null) continue;

                player.Position = poses[i];
                player.Rotation = rots[i];
            }

            return true;
        }
    }
}
