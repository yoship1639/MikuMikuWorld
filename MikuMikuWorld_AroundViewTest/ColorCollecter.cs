using MikuMikuWorld.GameComponents.ImageEffects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    class ColorCollecter : ColorCollect
    {
        protected override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            if (Input.IsKeyDown(OpenTK.Input.Key.E)) Contrast -= (float)deltaTime;
            if (Input.IsKeyDown(OpenTK.Input.Key.R)) Contrast += (float)deltaTime;
            if (Input.IsKeyDown(OpenTK.Input.Key.T)) Saturation -= (float)deltaTime;
            if (Input.IsKeyDown(OpenTK.Input.Key.Y)) Saturation += (float)deltaTime;
            if (Input.IsKeyDown(OpenTK.Input.Key.U)) Brightness -= (float)deltaTime;
            if (Input.IsKeyDown(OpenTK.Input.Key.I)) Brightness += (float)deltaTime;
        }
    }
}
