using MikuMikuWorld.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    [DataContract]
    public class ImportedObject
    {
        public Result Result;

        [DataMember(Name = "magicnumber", Order = 0)]
        public string MagicNumber = "MMW";

        [DataMember(Name = "exportversion", Order = 1)]
        public string ExportVersion = "1.0";

        public string Path;

        [DataMember(Name = "name", EmitDefaultValue = false, Order = 2)]
        public string Name;

        [DataMember(Name = "author", EmitDefaultValue = false, Order = 3)]
        public string Author;

        [DataMember(Name = "editor", EmitDefaultValue = false, Order = 4)]
        public string Editor;

        [DataMember(Name = "version", EmitDefaultValue = false, Order = 5)]
        public string Version;

        [DataMember(Name = "author_url", EmitDefaultValue = false, Order = 6)]
        public string AuthorURL;

        [DataMember(Name = "editor_url", EmitDefaultValue = false, Order = 7)]
        public string EditorURL;

        [DataMember(Name = "description", EmitDefaultValue = false, Order = 8)]
        public string Description;

        public ImportedObjectType Type;

        [DataMember(Name = "property", EmitDefaultValue = false, Order = 9)]
        public ImportedProperty Property;

        [DataMember(Name = "bones", EmitDefaultValue = false, Order = 10)]
        public Bone[] Bones;

        [DataMember(Name = "materials", EmitDefaultValue = false, Order = 11)]
        public Material[] Materials;

        [DataMember(Name = "textures", EmitDefaultValue = false, Order = 12)]
        public Texture2D[] Textures;

        [DataMember(Name = "meshes", EmitDefaultValue = false, Order = 13)]
        public Mesh[] Meshes;

        [DataMember(Name = "morphs", EmitDefaultValue = false, Order = 14)]
        public Morph[] Morphs;

        [DataMember(Name = "motions", EmitDefaultValue = false, Order = 15)]
        public Motion[] Motions;

        public override string ToString()
        {
            return Name;
        }
    }

    [DataContract]
    [KnownType(typeof(byte[]))]
    [KnownType(typeof(int[]))]
    [KnownType(typeof(float[]))]
    public class ImportedProperty
    {
        [DataMember(Name = "name", EmitDefaultValue = false, Order = 0)]
        public string Name;

        [DataMember(Name = "value", EmitDefaultValue = false, Order = 1)]
        public object Value;

        [DataMember(Name = "children", EmitDefaultValue = false, Order = 2)]
        public ImportedProperty[] Children;

        public override string ToString()
        {
            return Name;
        }
    }

    public enum ImportedObjectType
    {
        Unknown,
        Text,
        Property,
        Texture,
        Material,
        Shader,
        ImageEffectShader,
        Effect,
        Mesh,
        Model,
        SkinnedModel,
        Motion,
        Stage,
        Character,
        Compound,
    }

    [DataContract]
    public class ImportedOverviewObject
    {
        public Result Result;

        [DataMember(Name = "magicnumber", Order = 0)]
        public string MagicNumber = "MMW";

        [DataMember(Name = "exportversion", Order = 1)]
        public string ExportVersion = "1.0";

        public string Path;

        [DataMember(Name = "name", EmitDefaultValue = false, Order = 2)]
        public string Name;

        [DataMember(Name = "author", EmitDefaultValue = false, Order = 3)]
        public string Author;

        [DataMember(Name = "editor", EmitDefaultValue = false, Order = 4)]
        public string Editor;

        [DataMember(Name = "version", EmitDefaultValue = false, Order = 5)]
        public string Version;

        [DataMember(Name = "author_url", EmitDefaultValue = false, Order = 6)]
        public string AuthorURL;

        [DataMember(Name = "editor_url", EmitDefaultValue = false, Order = 7)]
        public string EditorURL;

        [DataMember(Name = "description", EmitDefaultValue = false, Order = 8)]
        public string Description;

        public ImportedObjectType Type;

        public override string ToString()
        {
            return Name;
        }
    }
}
