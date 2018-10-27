using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.Walker;
using OpenTK;

namespace MikuMikuWorld.Scripts.Character
{
    class CharacterNameResolver : DrawableGameComponent
    {
        public static Font font = Controls.ControlDrawer.fontSmallB;

        private bool shown = true;
        private WalkerPlayer player;
        private Assets.Character chara;
        private string type = "archive";
        private Brush brush;

        protected override void OnLoad()
        {
            Layer = LayerUI;

            player = GameObject.GetComponent<CharacterInfo>().Player;
            chara = GameObject.GetComponent<CharacterInfo>().Character;
            brush = Brushes.White;
        }

        protected override void Draw(double deltaTime, Camera camera)
        {
            if (!shown) return;
            if ((Transform.WorldPosition - camera.Transform.WorldPosition).Length > 25.0f) return;

            var wp = GameObject.Transform.WorldPosition;
            wp += new Vector3(0.0f, chara.Height + 0.15f, 0.0f);
            var pos = Util.ToScreenPos(wp, camera.View, camera.Projection, (int)MMW.Width, (int)MMW.Height);

            if (type == "name")
            {
                var w = Drawer.MeasureString(player.Name, font).X;
                Drawer.GetGraphics().DrawString(player.Name, font, brush, pos.X - w * 0.5f, pos.Y);
            }
            else if (type == "archive")
            {
                var name = player.Name + $" [WR.{player.Rank}]";
                var w1 = Drawer.MeasureString(name, font).X;

                if (player.ArchiveIndex < 0 || player.ArchiveIndex >= player.Achivements.Count)
                {
                    Drawer.GetGraphics().DrawString(name, font, brush, pos.X - w1 * 0.5f, pos.Y);
                }
                else if (player.Achivements[player.ArchiveIndex].Verify())
                {
                    var arc = player.Achivements[player.ArchiveIndex];
                    var w2 = Drawer.MeasureString(arc.Name, font).X;

                    var b = Brushes.Yellow;
                    if ((arc.Name == "Little Donor" || arc.Name == "Common Donor") && arc.PublicKey == Achivement.PublicPub) b = Brushes.LightGreen;
                    else if ((arc.Name == "Big Donor" || arc.Name == "Super Donor") && arc.PublicKey == Achivement.PublicPub) b = Brushes.RoyalBlue;
                    else if (arc.Name == "Messiah" && arc.PublicKey == Achivement.PublicPub) b = Brushes.Purple;
                    else if (arc.Name == "MMW Creater" && arc.PublicKey == Achivement.PublicPub) b = Brushes.OrangeRed;

                    Drawer.GetGraphics().DrawString(name, font, brush, pos.X - w1 * 0.5f, pos.Y - 18.0f);
                    Drawer.GetGraphics().DrawString(arc.Name, font, b, pos.X - w2 * 0.5f, pos.Y);
                }
            }
            else if (type == "comment")
            {
                var name = player.Name + $" [WR.{player.Rank}]";
                var w1 = Drawer.MeasureString(name, font).X;

                if (string.IsNullOrWhiteSpace(player.Comment))
                {
                    Drawer.GetGraphics().DrawString(name, font, brush, pos.X - w1 * 0.5f, pos.Y);
                }
                else
                {
                    var comment = $"「{player.Comment}」";
                    var w2 = Drawer.MeasureString(comment, font).X;

                    Drawer.GetGraphics().DrawString(name, font, brush, pos.X - w1 * 0.5f, pos.Y - 18.0f);
                    Drawer.GetGraphics().DrawString(comment, font, brush, pos.X - w2 * 0.5f, pos.Y);
                }
            }
        }

        protected override void OnReceivedMessage(string message, params object[] args)
        {
            if (message == "change chara display")
            {
                if (args == null || args.Length == 0)
                {
                    if (type == "none") type = "name";
                    else if (type == "name") type = "archive";
                    else if (type == "archive") type = "comment";
                    else if (type == "comment") type = "none";
                }
                else type = (string)args[0];
            }
            else if (message == "show dialog")
            {
                shown = false;
            }
            else if (message == "close dialog")
            {
                shown = true;
            }
        }
    }
}
