using MikuMikuWorld;
using MikuMikuWorld.Assets;
using MikuMikuWorld.Assets.Shaders;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.GameComponents.Coliders;
using MikuMikuWorld.GameComponents.ImageEffects;
using MikuMikuWorld.GameComponents.Lights;
using MikuMikuWorld.Importers;
using MikuMikuWorld.Properties;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qiita_Shader
{
    class Game : MMWGameWindow
    {
        public Game() :
            base(1280, 720, "Walker")
        {
            //MMW.Configuration.Antialias = MMWConfiguration.AntialiasType.FXAA;
            //MMW.Configuration.ShadowQuality = MMWConfiguration.ShadowQualityType.High;
            //MMW.Configuration.AO = MMWConfiguration.AOType.SSAO;
            //MMW.Configuration.Shader = MMWConfiguration.ShaderType.Physical;
            //MMW.Configuration.IBLQuality = MMWConfiguration.IBLQualityType.Default;
            //MMW.Configuration.Bloom = MMWConfiguration.BloomType.Bloom;
            //MMW.Configuration.MotionBlur = MMWConfiguration.MotionBlurType.MotionBlur;
            //MMW.Configuration.DoF = MMWConfiguration.DoFType.DoF;
            //MMW.Configuration.ToneMapping = true;
            //MMW.Configuration.DrawEdge = false;

            /*
            // Oculusが接続済み
            if (MMW.HMDCamera.Connected)
            {
                WindowState = WindowState.Fullscreen;
                //ClientSize = new Size(MMW.Rift.VResolution, MMW.Rift.HResolution);
                ClientSize = new Size(MMW.HMDCamera.DisplayDevice.Height, 1280);
            }
            else
            {
                WindowState = WindowState.Fullscreen;
                //ClientSize = new Size(1366, 768);
                ClientSize = new Size(1920, 1080);
                WindowBorder = WindowBorder.Fixed;
                CursorVisible = false;
            }
            */
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            var effs = MMW.MainCamera.GameObject.GetComponents<ImageEffect>();
            foreach (var eff in effs) eff.Enabled = false;

            var hw = new GameObject("Hello World");
            hw.AddComponent<HelloWorld>();
            MMW.RegistGameObject(hw);
        }
    }
}

