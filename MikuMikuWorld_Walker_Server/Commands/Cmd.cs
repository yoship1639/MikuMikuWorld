using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld_Walker_Server.Commands
{
    public abstract class Cmd
    {
        public virtual int[] ExecDataTypes { get; } = new int[0];
        public virtual bool Executable(string cmd) => false;
        public virtual bool Exec(MainForm form, string cmd) => true;

        public virtual bool OnPeerConnected(MainForm form, Peer peer) => false;
        public virtual bool OnPeerAccepted(MainForm form, Peer peer) => false;
        public virtual bool OnDataReceived(MainForm form, bool isTcp, Peer peer, int dataType, byte[] data) => false;
        public virtual bool OnPeerDisconnected(MainForm form, Peer peer) => false;
    }
}
