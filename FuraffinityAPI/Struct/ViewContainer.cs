using FuraffinityAPI.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuraffinityAPI.Struct
{
    public struct ViewContainer
    {
        public Rating Rating { get; internal set; }
        public ResourceType Type { get; internal set; }
        public string Title { get; internal set; }
        public string Description { get; internal set; }
        public string ResourceUrl { get; internal set; }
        public string User { get; internal set; }
        public string AvatarUrl { get; internal set; }
        public DateTime PostTime { get; internal set; }
        public int Views { get; internal set; }
        public int Comments { get; internal set; }
        public int Favorites { get; internal set; }
        public string Category { get; internal set; }
        public string[] Species { get; internal set; }
        public string Gender { get; internal set; }
        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public string[] Tags { get; internal set; }
    }
}
