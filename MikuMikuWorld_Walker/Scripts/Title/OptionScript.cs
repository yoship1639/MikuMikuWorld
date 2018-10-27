using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikuMikuWorld.Assets;
using MikuMikuWorld.Controls;
using MikuMikuWorld.GameComponents;
using OpenTK;

namespace MikuMikuWorld.Scripts.Title
{
    class OptionScript : DrawableGameComponent
    {
        private List<Control> controls = new List<Control>();
        private TransitControl transit;
        private float transition = 0.0f;
        private bool trans = false;
        public bool AcceptInput { get; set; } = true;
        private MenuInputResolver input;
        private UserData userData;

        protected override void OnLoad()
        {
            base.OnLoad();

            Layer = LayerUI;

            MMW.FindGameComponent<BackgroundScript>().Trans(new OpenTK.Graphics.Color4(148, 212, 222, 255), 0.25);

            transit = new TransitControl();
            transit.LocalLocation = new Vector2(MMW.ClientSize.Width * 2.0f, 0);
            transit.Size = new Vector2(MMW.ClientSize.Width, MMW.ClientSize.Height);
            transit.Target = Vector2.Zero;

            input = new MenuInputResolver();

            var label = new Label()
            {
                Parent = transit,
                Alignment = ContentAlignment.TopCenter,
                Text = "OPTION",
                Font = new Font("Yu Gothic UI Light", 40.0f),
                LocalLocation = new Vector2(0.0f, 32.0f),
            };
            controls.Add(label);

            userData = MMW.GetAsset<UserData>();

            var labelName = new Label(transit, "User Name", new Vector2(200.0f, 160.0f));
            labelName.Font = Control.DefaultFontB;
            controls.Add(labelName);
            var textBoxName = new TextBox2(labelName, userData.UserName, new Vector2(200.0f, 0.0f), new Vector2(300.0f, 32.0f));
            textBoxName.MaxLength = 16;
            textBoxName.TextChanged += (s, e) => { userData.UserName = textBoxName.Text; };
            controls.Add(textBoxName);

            var labelArchive = new Label(transit, "Display Achivement", new Vector2(200.0f, 200.0f));
            labelArchive.Font = Control.DefaultFontB;
            controls.Add(labelArchive);
            var comboArchive = new ComboBox(labelArchive, new Vector2(200.0f, 0.0f), new Vector2(300.0f, 32.0f));
            if (userData.Achivements.Count > 0)
            {
                comboArchive.Items = userData.Achivements.ToArray();
                comboArchive.SelectedIndex = userData.ArchiveIndex;
                comboArchive.DisplayMember = "Name";
                comboArchive.SelectedIndexChanged += (s, e) => { userData.ArchiveIndex = e; };
            }
            else comboArchive.Enabled = false;
            controls.Add(comboArchive);
        }

        protected override void Update(double deltaTime)
        {
            transition += (trans ? -1.0f : 1.0f) * (float)deltaTime * 5.0f;
            transition = MMWMath.Saturate(transition);

            transit.Update(deltaTime);

            if (AcceptInput && !trans)
            {
                input.Update(deltaTime);
                controls.ForEach(c => c.Update(null, deltaTime));

                if (input.IsBack)
                {
                    MMW.GetAsset<Sound>("back").Play();
                    trans = true;
                    transit.Target = new Vector2(-MMW.ClientSize.Width * 2.0f, 0.0f);
                    GameObject.AddComponent<TitleScript>();
                }
            }

            if (trans && transition < 0.01f) Destroy();
        }

        protected override void Draw(double deltaTime, Camera camera)
        {
            var g = Drawer.BindGraphicsDraw();

            controls.ForEach(c => c.Draw(g, deltaTime));

            Drawer.IsGraphicsUsed = true;
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            controls.ForEach(c => c.Unload());
        }
    }
}
