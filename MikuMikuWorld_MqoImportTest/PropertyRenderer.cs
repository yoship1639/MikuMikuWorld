using MikuMikuWorld.GameComponents;
using MikuMikuWorld.GameComponents.Coliders;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    class PropertyRenderer : TextRenderer
    {
        Brush backBrush;
        GameObject mqo;

        public string UserText { get; set; }

        protected override void OnLoad()
        {
            base.OnLoad();

            for (var i = 0; i < 32; i++) SetText(i, "", 0, i * 16.0f);
            backBrush = new SolidBrush(Color.FromArgb(64, 0, 0, 0));

            mqo = MMW.FindGameObject((o => o.Tags.Contains("mqo")));
        }

        protected override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            var idx = 0;
            SetText(idx++, "FPS: " + MMW.FPS);
            SetText(idx++, "Name: " + mqo.Name);
            Console.WriteLine(MMW.FPS);
        }

        //protected override void Draw(double deltaTime, Camera camera)
        //{
        //    
        //}
    }
}
