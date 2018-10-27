using MikuMikuWorld.Networks;
using MikuMikuWorld.Walker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Scripts.Player
{
    class UdpSender : GameComponent
    {
        private Server server;
        private GameObject player;
        public double IntervalMillSec { get; set; } = 400.0;
        private double total = 0.0;

        protected override void OnLoad()
        {
            player = MMW.FindGameObject(o => o.Tags.Contains("player"));
            server = MMW.GetAsset<Server>();
        }

        protected override void Update(double deltaTime)
        {
            total += deltaTime * 1000.0;
            if (total < IntervalMillSec) return;
            total -= IntervalMillSec;

            var pos = player.Transform.Position;
            var rot = player.Transform.Rotate;

            var buf = Buffer.Write(bw =>
            {
                // type
                bw.Write(DataType.ResponsePlayerTransform);
                // length
                bw.Write(24);

                bw.Write(pos.X);
                bw.Write(pos.Y);
                bw.Write(pos.Z);
                bw.Write(rot.X);
                bw.Write(rot.Y);
                bw.Write(rot.Z);

                // TODO: script udps
            });

            server.SendUdp(buf);
        }
    }
}
