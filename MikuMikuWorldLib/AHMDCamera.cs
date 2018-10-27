using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    public enum HMDCameraType
    {
        Default,
        Right,
        Left,
    };

    public abstract class AHMDCamera
    {
        public abstract bool Connected { get; }
        public abstract DisplayDevice DisplayDevice { get; }
        public abstract HMDCameraType CameraType { get; set; }
    }
}
