using FuraffinityAPI.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuraffinityAPI.Page
{
    public class FavoritesPage : GalleryPage
    {
        internal FavoritesPage(SimpleHttpClient httpClient, OrderedSemaphore semaphore, params string[] args) : base(httpClient, semaphore, args)
        {
        }

        protected internal override string? GetUrl(params string[] args)
        {
            if (args.Length == 0)
                return null;
            return $"https://www.furaffinity.net{args[0]}";
        }

        public async new Task<ResourceContainer[]> GetArrayAsync()
        {
            var doc = await GetHtmlDocumentAsync();
            var figures = doc.DocumentNode.SelectNodes("//section[@id='gallery-favorites']//figure");
            if (figures == null)
                return new ResourceContainer[0];
            return Furaffinity.ParseResourceContainer(figures);
        }
    }
}
