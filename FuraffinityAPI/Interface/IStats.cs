using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuraffinityAPI.Interface
{
    public interface IStats
    {
        public int GetViews();
        public int GetSubmissions();
        public int GetFavs();
        public int GetCommentsEarned();
        public int GetCommentsMade();
        public int GetJournals();
    }
}
