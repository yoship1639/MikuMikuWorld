using MikuMikuWorld;
using MikuMikuWorld.Network;
using MikuMikuWorldScript;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld_Walker_Server.Commands
{
    class CmdWorldDataDesc : Cmd
    {
        public override int[] ExecDataTypes => new int[]
        {
            MikuMikuWorld.Walker.DataType.RequestDataDesc,
        };

        public override bool OnDataReceived(MainForm form, bool isTcp, Peer peer, int dataType, byte[] data)
        {
            if (peer.Pending)
            {
                peer.SendTcp(MikuMikuWorld.Walker.DataType.ResponseDataDesc, "{}");
                return true;
            }

            var desc = new NwWorldDataDesc();

            var worlds = new List<NwDataInfo>();
            foreach (var w in form.WorldHashmap)
            {
                var info = new NwDataInfo(w.Key, w.Value.Length);
                worlds.Add(info);
            }
            desc.Worlds = worlds.ToArray();

            var chars = new List<NwDataInfo>();
            foreach (var ch in form.CharHashmap)
            {
                var info = new NwDataInfo(ch.Key, ch.Value.Length);
                chars.Add(info);
            }
            desc.Characters = chars.ToArray();

            var objs = new List<NwDataInfo>();
            foreach (var o in form.ObjHashmap)
            {
                var info = new NwDataInfo(o.Key, o.Value.Length);
                objs.Add(info);
            }
            desc.Objects = objs.ToArray();

            var goscs = new List<NwDataInfo>();
            foreach (var o in form.GameObjectScriptHashmap)
            {
                var info = new NwDataInfo(o.Key, o.Value.Length);
                goscs.Add(info);
            }
            desc.GameObjectScripts = goscs.ToArray();

            var json = Util.SerializeJsonBinary(desc);
            peer.SendTcp(MikuMikuWorld.Walker.DataType.ResponseDataDesc, json);

            return true;
        }
    }
}
