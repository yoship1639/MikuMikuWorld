using MikuMikuWorld.GameComponents.ImageEffects;
using MikuMikuWorld.Scripts.Player;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Scripts.HUD
{
    class DoFResolver : GameComponent
    {
        BokehDoF dof;
        GameObject focusedObj;
        PlayerRayData ray;
        string cameraType;

        protected override void OnLoad()
        {
            dof = MMW.MainCamera.GameObject.GetComponent<BokehDoF>();
        }

        protected override void Update(double deltaTime)
        {
            if (dof == null || !dof.Enabled) return;

            if (focusedObj == null)
            {
                dof.Focus = MMWMath.Approach(dof.Focus, 1.6f, (float)deltaTime * 5.0f);
                //dof.NearBias = MMWMath.Approach(dof.NearBias, 24.0f, (float)deltaTime * 10.0f);
                dof.FarBias = MMWMath.Approach(dof.FarBias, 0.0f, (float)deltaTime * 1.5f);
                dof.FarRadiusMax = MMWMath.Approach(dof.FarRadiusMax, 12.0f, (float)deltaTime * 12.0f);
            }
            else if (cameraType == "first person")
            {
                dof.Focus = MMWMath.Approach(dof.Focus, ray.distance, (float)deltaTime * 5.0f);
                //dof.NearBias = MMWMath.Approach(dof.NearBias, 12.0f, (float)deltaTime * 10.0f);
                dof.FarBias = MMWMath.Approach(dof.FarBias, Math.Min(0.5f / ray.distance, 2.0f), (float)deltaTime * 0.25f);
                dof.FarRadiusMax = MMWMath.Approach(dof.FarRadiusMax, Math.Min(12.0f / ray.distance, 24.0f), (float)deltaTime * 12.0f);
            }
        }

        protected override void OnReceivedMessage(string message, params object[] args)
        {
            if (message == "focus enter")
            {
                focusedObj = (GameObject)args[0];
                ray = (PlayerRayData)args[1];
            }
            else if (message == "focus leave")
            {
                focusedObj = null;
                ray = (PlayerRayData)args[1];
            }
            else if (message == "camera change")
            {
                cameraType = (string)args[0];
            }
        }
    }
}
