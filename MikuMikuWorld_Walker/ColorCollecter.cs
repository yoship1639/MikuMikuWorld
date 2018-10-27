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

            Contrast = MMW.Contrast;
            Saturation = MMW.Saturation;
            Brightness = MMW.Brightness;
        }
    }
}
