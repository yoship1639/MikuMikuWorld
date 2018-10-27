using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Walker
{
    public class DataType
    {
        public static readonly byte[] Magic = Encoding.UTF8.GetBytes("MMWN");

        public static readonly int Login = 6;
        public static readonly int LoginResult = 7;

        public static readonly int ResponseHostRemotePort = 10;
        public static readonly int ResponseClientLocalPort = 11;

        public static readonly int RequestServerDesc = 100;
        public static readonly int ResponseServerDesc = 101;
        public static readonly int RequestWorldInfo = 102;
        public static readonly int ResponseWorldInfo = 103;

        public static readonly int RequestDataDesc = 110;
        public static readonly int ResponseDataDesc = 111;
        public static readonly int RequestWorld = 112;
        public static readonly int ResponseWorld = 113;
        public static readonly int RequestCharacter = 114;
        public static readonly int ResponseCharacter = 115;
        public static readonly int RequestObject = 116;
        public static readonly int ResponseObject = 117;
        public static readonly int RequestScript = 118;
        public static readonly int ResponseScript = 119;
        public static readonly int RequestGameObjectScript = 120;
        public static readonly int ResponseGameObjectScript = 121;

        public static readonly int RequestGameObjects = 150;
        public static readonly int ResponseGameObjects = 151;

        public static readonly int RequestScriptUpdate = 160;
        public static readonly int ResponseScriptUpdate = 161;

        public static readonly int RequestObjectDestroy = 170;
        public static readonly int ResponseObjectDestroy = 171;

        public static readonly int ResponseReady = 200;

        public static readonly int ResponsePlayerJoin = 201;

        public static readonly int RequestLeaveWorld = 210;
        public static readonly int ResponseLeaveWorld = 211;

        public static readonly int RequestWorldStatus = 220;
        public static readonly int ResponseWorldStatus = 221;

        public static readonly int ResponsePlayerTransform = 300;
        public static readonly int ResponseAllTransform = 301;

        public static readonly int RequestObjectPicked = 310;
        public static readonly int ResponseObjectPicked = 311;
        public static readonly int RequestObjectPut = 320;
        public static readonly int ResponseObjectPut = 321;
        public static readonly int RequestItemUsed = 330;
        public static readonly int ResponseItemUsed = 331;

        public static readonly int Chat = 1000;
        public static readonly int PictureChat = 1010;
    }
}
