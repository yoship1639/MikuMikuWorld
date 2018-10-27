using MikuMikuWorldScript;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Walker
{
    public class WalkerPlayer
    {
        public string Name;
        public string UserID;
        public int SessionID;
        public int Color = int.MaxValue;
        public Bitmap Icon { get; set; }

        public bool IsAdmin;

        public long Rank;
        public List<Achivement> Achivements;
        public long LikesCount;
        public long LikedCount;
        public int ArchiveIndex;
        public string Comment;

        public Vector3f Position { get; set; }
        public Vector3f Rotation { get; set; }
        public string CharacterHash { get; set; }

        public void Update(WalkerPlayer player)
        {
            Rank = player.Rank;
            Achivements = player.Achivements;
            LikesCount = player.LikesCount;
            LikedCount = player.LikedCount;
            Comment = player.Comment;
        }
    }
}
