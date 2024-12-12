using FuraffinityAPI.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuraffinityAPI.Struct
{
    public struct ResourceContainer
    {
        public Rating Rating { get; internal set; }
        public ResourceType Type { get; internal set; }
        public string User { get; internal set; }
        public string Title { get; internal set; }
        public string Url { get; internal set; }
        public string ImageUrl { get; internal set; }
        public long Sid { get; internal set; }
        public float Width { get; internal set; }
        public float Height { get; internal set; }
    }
}
