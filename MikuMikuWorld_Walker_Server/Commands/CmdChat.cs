using MikuMikuWorld;
using MikuMikuWorld.Walker;
using MikuMikuWorld.Walker.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld_Walker_Server.Commands
{
    class CmdChat : Cmd
    {
        public override int[] ExecDataTypes => new int[]
        {
            DataType.Chat,
        };

        public static void Send(NetworkServer ns, Peer[] peers, NwChat chat)
        {
            if (peers == null) peers = ns.peers.ToArray();

            foreach (var p in peers)
            {
                p.SendTcp(DataType.Chat, Util.SerializeJson(chat));
            }
        }

        public static void Send(NetworkServer ns, Peer from, string mes)
        {
            using (var ms = new MemoryStream())
            {
                using (var bw = new StreamWriter(ms))
                {
                    if (from == null) bw.Write(0);
                    else bw.Write(from.SessionID);
                    bw.Write(mes);
                }
                ns.SendTcp(DataType.Chat, ms.ToArray());
            }
        }

        public override bool Exec(MainForm form, string cmd)
        {
            var cmds = cmd.Split(' ');
            string mes = null;
            if (cmds[0] == "chat")
            {
                mes = string.Join(" ", cmds.Skip(1).Take(cmds.Length - 1).ToArray());
                Send(form.Server, null, new NwChat() { Text = cmds[1], From = 0 });
            }
            else if (cmds[0] == "chatfor")
            {
                var peers = new List<Peer>();
                var count = int.Parse(cmds[1]);
                for (var i = 0; i < count; i++)
                {
                    var peer = Array.Find(form.Server.Peers, p => p.SessionID == int.Parse(cmds[i + 2]));
                    if (peer != null) peers.Add(peer);
                }
                mes = string.Join(" ", cmds.Skip(count + 1).Take(cmds.Length - (count + 1)).ToArray());
                Send(form.Server, peers.ToArray(), new NwChat() { From = 0, Text = mes });
            }
            else if (cmds[0] == "chatto")
            {
                var peer = Array.Find(form.Server.Peers, p => p.SessionID == int.Parse(cmds[1]));
                if (peer == null) return false;
                mes = string.Join(" ", cmds.Skip(2).Take(cmds.Length - 2).ToArray());
                Send(form.Server, new Peer[] { peer }, new NwChat() { From = 0, Text = mes });
            }

            if (mes != null)
            {
                form.LogInfo("Server: " + mes, true);
                return true;
            }

            return false;
        }

        public override bool Executable(string cmd)
        {
            var cmds = cmd.Split(' ');
            return cmds[0] == "chat" || cmds[0] == "chatfor" || cmds[0] == "chatto";
        }

        public override bool OnDataReceived(MainForm form, bool isTcp, Peer peer, int dataType, byte[] data)
        {
            if (peer.Pending)
            {
                return true;
            }

            var chat = Util.DeserializeJson<NwChat>(data.ToJson());
            if (chat.To != null)
            {
                var peers = new List<Peer>();
                foreach (var id in chat.To)
                {
                    var p = Array.Find(form.Server.Peers, pe => pe.SessionID == id);
                    if (p != null) peers.Add(p);
                }
                chat.To = null;
                Send(form.Server, peers.ToArray(), chat);
            }
            else
            {
                chat.To = null;
                Send(form.Server, null, chat);
            }

            form.LogInfo($"{peer}: {chat.Text}", true);
            
            return false;
        }
    }
}
