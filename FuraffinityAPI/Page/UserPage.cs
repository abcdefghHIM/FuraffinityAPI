using FuraffinityAPI.Interface;
using FuraffinityAPI.Struct;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace FuraffinityAPI.Page
{
    public class UserPage : AbsPage
    {
        internal UserPage(HttpClient httpClient, OrderedSemaphore semaphore, params string[] args) : base(httpClient, semaphore, args)
        {
        }

        public async Task<string> GetAvatarUrlAsync()
        {
            var doc = await GetHtmlDocumentAsync();
            var img = doc.DocumentNode.SelectSingleNode("//userpage-nav-avatar//img");
            return "https:" + img.Attributes["src"].Value;
        }

        public async Task<string> GetNameAsync()
        {
            var doc = await GetHtmlDocumentAsync();
            var img = doc.DocumentNode.SelectSingleNode("//userpage-nav-avatar//img");
            return img.Attributes["alt"].Value;
        }

        public async Task<DateTime> GetRegisteredTimeAsync()
        {
            var doc = await GetHtmlDocumentAsync();
            var obj = doc.DocumentNode.SelectSingleNode("//userpage-nav-user-details/div/username/span/following-sibling::text()");
            string format = "MMM dd, yyyy HH:mm";
            DateTime dateTime = DateTime.ParseExact(obj.InnerText.Trim(), format, CultureInfo.InvariantCulture);
            return dateTime;
        }

        public async Task<string> GetDescriptionAsync()
        {
            var doc = await GetHtmlDocumentAsync(); 
            var text = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'userpage-profile')]").InnerText.Trim();
            return HttpUtility.HtmlDecode(text);
        }

        public async Task<IStats> GetStatsAsync()
        {
            var doc = await GetHtmlDocumentAsync();
            return new UserStats(doc);
        }

        public PageCollection<StarPage> GetStarPage()
        {
            return new PageCollection<StarPage>(httpClient, semaphore, args[0]);
        }

        public PageCollection<FansPage> GetFansPage()
        {
            return new PageCollection<FansPage>(httpClient, semaphore, args[0]);
        }

        public PageCollection<GalleryPage> GetGallery()
        {
            return new PageCollection<GalleryPage>(httpClient, semaphore, args[0]);
        }
        public PageCollection<FavoritesPage> GetFavorites()
        {
            return new PageCollection<FavoritesPage>(httpClient, semaphore, args[0]);
        }

        protected internal override string? GetUrl(params string[] args)
        {
            if (args.Length == 0)
                return null;
            return $"https://www.furaffinity.net/user/{args[0]}";
        }
    }
}
