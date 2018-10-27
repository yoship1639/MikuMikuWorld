using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.Network;
using MikuMikuWorld.Networks;
using MikuMikuWorld.Walker;
using MikuMikuWorldScript;
using OpenTK;

namespace MikuMikuWorld.Scripts
{
    class WalkerGameObjectScript : DrawableGameComponent
    {
        public override int[] TcpAcceptDataTypes => new int[]
        {
            DataType.ResponseScriptUpdate,
        };

        public GameObjectScript Script { get; internal set; }
        private bool loaded = false;
        private byte[] status;
        public byte[] Status
        {
            get { return status; }
            internal set { status = value; if (Script != null && loaded) Script.OnReceivedUpdateStatus(status); }
        }
        private bool focused = false;

        public WalkerGameObjectScript(GameObject go, GameObjectScript script, byte[] status)
        {
            script.GameObject = go;
            script.Server = MMW.GetAsset<Server>();
            script.WorldData = MMW.GetAsset<WorldData>();
            script.MasterData = MMW.MasterData;
            Status = status;
            Script = script;
        }

        protected override void OnLoad()
        {
            Script.OnLoad(Status);
            Layer = Script.Layer;
            loaded = true;
        }
        protected override void Update(double deltaTime)
        {
            if (!Script.Enabled) return;

            /*
            if (focused)
            {
                foreach (var k in Input.DownKeys) Script.OnKeyDown(k);
                foreach (var k in Input.PressedKeys) Script.OnKeyPressed(k);
                foreach (var k in Input.ReleasedKeys) Script.OnKeyReleased(k);

                foreach (var b in Input.DownButtons) Script.OnMouseButtonDown(b);
                foreach (var b in Input.PressedButtons) Script.OnMouseButtonPressed(b);
                foreach (var b in Input.ReleasedButtons) Script.OnMouseButtonReleased(b);
            }
            */

            Script.OnUpdate(deltaTime);
        }
        protected override void OnUnload()
        {
            Script.OnUnload();
            loaded = false;
        }
        protected override void Draw(double deltaTime, Camera camera)
        {
            if (!Script.Visible) return;
            Script.OnDraw(deltaTime, Drawer.ScriptDrawer, camera);
        }
        protected override void MeshDraw(double deltaTime, Camera camera)
        {
            if (!Script.Visible) return;
            Script.OnMeshDraw(deltaTime, Drawer.ScriptMeshDrawer, camera);
        }
        protected override void OnReceivedMessage(string message, params object[] args)
        {
            if (!Script.Enabled) return;

            if (message == "switch on")
            {
                try
                {
                    var dist = (Transform.WorldPosition - (Vector3)args[1]).Length;
                    if (dist <= (float)args[2]) Script.OnSwitchOn((int)args[0]);
                }
                catch { }
            }
            else if (message == "switch off")
            {
                try
                {
                    var dist = (Transform.WorldPosition - (Vector3)args[1]).Length;
                    if (dist <= (float)args[2]) Script.OnSwitchOff((int)args[0]);
                }
                catch { }
            }
            else if (message == "get coin")
            {
                try
                {
                    Script.OnReceivedMessage(message, args[0]);
                }
                catch { }
            }
            else if (message == "focus enter")
            {
                if ((GameObject)args[0] != GameObject) return;
                focused = true;
                Script.OnFocused();
            }
            else if (message == "focus leave")
            {
                if ((GameObject)args[0] != GameObject) return;
                focused = false;
                Script.OnDefocused();
            } 
            else
            {
                Script.OnReceivedMessage(message, args);
            }
        }

        public override void OnTcpReceived(int dataType, byte[] data)
        {
            Buffer.Read(data, br =>
            {
                var str = br.ReadString();
                if (str != GameObject.Hash) return;
                var name = br.ReadString();
                if (name != Script.ScriptHash) return;

                var length = br.ReadInt32();
                Status = br.ReadBytes(length);
            });
        }
    }
}
