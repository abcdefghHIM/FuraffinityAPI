using FuraffinityAPI.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuraffinityAPI.Page
{
    public class GalleryPage : AbsPage
    {
        internal GalleryPage(HttpClient httpClient, SemaphoreSlim semaphore, params string[] args) : base(httpClient, semaphore, args)
        {
        }

        protected internal override string? GetUrl(params string[] args)
        {
            if (args.Length == 0)
                return null;
            return $"https://www.furaffinity.net{args[0]}";
        }

        public async Task<ResourceContainer[]> GetArrayAsync()
        {
            var doc = await GetHtmlDocumentAsync();
            var figures = doc.DocumentNode.SelectNodes("//section[@id='gallery-gallery']//figure");
            return Furaffinity.ParseResourceContainer(figures);
        }

        public async Task<string?> GetNextUrlAsync()
        {
            var doc = await GetHtmlDocumentAsync();
            var form = doc.DocumentNode.SelectSingleNode("//section[@class='gallery-section']//form[.//button[normalize-space(text())='Next']]");
            if (form == null)
                return null;
            return form.Attributes["action"].Value;
        }
    }
}
