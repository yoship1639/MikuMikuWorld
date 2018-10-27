using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Assets
{
    [DataContract]
    public abstract class Texture : IAsset
    {
        public bool Loaded { get; protected set; }

        public int Index { get; set; } = -1;

        [DataMember(Name = "name", EmitDefaultValue = false, Order = 0)]
        public string Name { get; set; }

        /// <summary>
        /// ソースとなったビットマップ
        /// </summary>
        public Bitmap SrcBitmap { get; internal set; }

        /// <summary>
        /// テクスチャサイズ
        /// </summary>
        [DataMember(Name = "size", EmitDefaultValue = false, Order = 1)]
        public Size Size { get; protected set; }

        /// <summary>
        /// ピクセルフォーマット
        /// </summary>
        [DataMember(Name = "format", EmitDefaultValue = false, Order = 2)]
        public PixelInternalFormat Format { get; protected set; }

        /// <summary>
        /// テクスチャターゲット
        /// </summary>
        public abstract TextureTarget Target { get; }

        [DataMember(Name = "minfilter", EmitDefaultValue = false, Order = 3)]
        public TextureMinFilter MinFilter { get; set; } = TextureMinFilter.Linear;

        [DataMember(Name = "magfilter", EmitDefaultValue = false, Order = 4)]
        public TextureMagFilter MagFilter { get; set; } = TextureMagFilter.Linear;

        [DataMember(Name = "wrapmode", EmitDefaultValue = false, Order = 5)]
        public TextureWrapMode WrapMode { get; set; } = TextureWrapMode.ClampToEdge;

        [DataMember(Name = "mipmap", EmitDefaultValue = false, Order = 6)]
        public bool UseMipmap { get; set; } = false;

        internal int texture = -1;

        public abstract Result Load();

        public abstract Result Unload();
    }
}
