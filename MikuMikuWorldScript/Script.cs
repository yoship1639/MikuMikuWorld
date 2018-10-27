using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorldScript
{
    public abstract class Script
    {
        /// <summary>
        /// スクリプト名
        /// </summary>
        public abstract string ScriptName { get; }

        /// <summary>
        /// スクリプトの説明
        /// </summary>
        public abstract string ScriptDesc { get; }

        /// <summary>
        /// <para>スクリプトのレイヤ、0 ~ 15 メッシュ描画前、16 ~ 31 メッシュ描画後、32 ~ 47 HUD表示</para>
        /// </summary>
        public virtual int Layer { get; }

        /// <summary>
        /// 更新前の処理
        /// </summary>
        public virtual void OnBeforeUpdate(IMasterData data) { }

        /// <summary>
        /// 更新処理
        /// </summary>
        public virtual void OnUpdate(IMasterData data) { }

        /// <summary>
        /// 更新後の処理
        /// </summary>
        public virtual void OnAfterUpdate(IMasterData data) { }

        /// <summary>
        /// スクリプトが読み込まれた時に呼ばれる
        /// </summary>
        public virtual void OnLoad(IMasterData data) { }

        /// <summary>
        /// スクリプトが破棄される時に呼ばれる
        /// </summary>
        public virtual void OnUnload(IMasterData data) { }

        /// <summary>
        /// 描画処理時に呼ばれる
        /// </summary>
        public virtual void OnDraw(IMasterData data, IDrawer drawer) { }

        /// <summary>
        /// メッシュ描画処理時に呼ばれる
        /// </summary>
        public virtual void OnMeshDraw(IMasterData data, IDrawer drawer) { }
    }

    public abstract class NetworkScript : Script
    {
        /// <summary>
        /// 送信するUDPデータディクショナリ。
        /// 送信予定のデータはここに格納される
        /// </summary>
        public Dictionary<int, byte[]> SendUdpDataDic { get; private set; } = new Dictionary<int, byte[]>();

        /// <summary>
        /// 処理できるデータの種類
        /// </summary>
        public abstract int[] ExecutableDataTypes { get; }

        /// <summary>
        /// UDP通信で利用するデータの合計の長さ。
        /// 許容される最大長は60000であるが、
        /// スクリプトは複数利用される可能性があるため短いほど良い
        /// </summary>
        public abstract int UdpDataLength { get; }

        /// <summary>
        /// サーバに送信するUDPデータをセットする。
        /// 高速で同期したいデータはここにセットする。
        /// データ長の合計はUdpDataLength以下でなければならない
        /// </summary>
        public void SetUdpData(int dataType, byte[] data)
        {
            if (SendUdpDataDic.ContainsKey(dataType)) SendUdpDataDic[dataType] = data;
            else SendUdpDataDic.Add(dataType, data);
        }

        /// <summary>
        /// TCPでデータ送信する。
        /// </summary>
        public void SendTcpData(int dataType, byte[] data)
        {
            //Server.SendTcp(dataType, data);
        }

        /// <summary>
        /// サーバデータ。
        /// 必要な情報はここから取得する
        /// </summary>
        //public IServer Server { get; set; }
    }

    public abstract class ClientScript : NetworkScript
    {
        /// <summary>
        /// UDPでサーバからデータ受信した時に呼ばれる
        /// </summary>
        public abstract void OnUdpReceived(int dataType, byte[] data);

        /// <summary>
        /// TCPでサーバからデータ受信した時に呼ばれる
        /// </summary>
        public abstract void OnTcpReceived(int dataType, byte[] data);
    }

    public abstract class ServerScript : NetworkScript
    {
        /// <summary>
        /// UDPでデータ受信した時に呼ばれる
        /// </summary>
        //public abstract void OnUdpReceived(int dataType, byte[] data, Player player);

        /// <summary>
        /// TCPでデータ受信した時に呼ばれる
        /// </summary>
        //public abstract void OnTcpReceived(int dataType, byte[] data, Player player);

        /// <summary>
        /// コマンドが入力されたとき呼ばれる。
        /// コマンドを処理した場合true、処理できない場合falseを返す。
        /// </summary>
        public virtual bool OnCommend(string cmd)
        {
            return false;
        }
    }
}
