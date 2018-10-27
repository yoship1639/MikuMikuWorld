using MikuMikuWorld;
using MikuMikuWorld.Walker;
using MikuMikuWorldScript;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MikuMikuWorld_Walker_Server.Commands
{
    /*
    class CmdServerDesc : Cmd
    {
        public override bool OnPeerConnected(MainForm form, Peer peer)
        {
            ServerDesc desc = new ServerDesc();
            desc.GameType = 1;
            desc.Version = MainForm.Version;
            desc.WorldName = form.textBox_serverName.Text;
            desc.WorldDesc = form.textBox_serverDesc.Text;
            desc.WorldPass = form.checkBox_worldPass.Checked;
            desc.WorldImage = form.worldImageString;
            desc.Culture = ((CultureInfo)form.comboBox_culture.SelectedItem).LCID;
            desc.MaxPlayer = (int)form.numericUpDown_maxplyer.Value;
            desc.NowPlayer = form.listBox_player.Items.Count;
            desc.AllowUserEmote = form.checkBox_userEmote.Checked;
            desc.AllowUserModel = form.checkBox_userModel.Checked;
            desc.AllowUserObject = form.checkBox_userObj.Checked;
            desc.AllowUserSound = form.checkBox_userSound.Checked;
            desc.AllowUserStamp = form.checkBox_userStamp.Checked;

            var json = Util.SerializeJson(desc);
            peer.SendTcp(DataType.ResponseServerDesc, json);

            return true;
        }
    }*/
}
