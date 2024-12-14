using FuraffinityAPI.Interface;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuraffinityAPI.Struct
{
    public class UserStats : IStats
    {
        private HtmlNode root;

        internal UserStats(HtmlDocument document)
        {
            root = document.DocumentNode.SelectSingleNode("//div[@class='userpage-section-right'][1]//div[@class='table']");
        }

        private int GetValue(int index)
        {
            return int.Parse(root.SelectSingleNode($"(.//span[@class='highlight'])[{index}]/following-sibling::text()").InnerText.Trim());
        }

        public int GetViews() { return GetValue(1); }
        public int GetSubmissions() { return GetValue(2); }
        public int GetFavs() { return GetValue(3); }
        public int GetCommentsEarned() { return GetValue(4); }
        public int GetCommentsMade() { return GetValue(5); }
        public int GetJournals() { return GetValue(6); }
    }
}
