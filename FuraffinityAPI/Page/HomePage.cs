using FuraffinityAPI.Interface;
using FuraffinityAPI.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuraffinityAPI.Page
{
    public class HomePage : AbsPage
    {
        internal HomePage(HttpClient httpClient, OrderedSemaphore semaphore, params string[] args) : base(httpClient, semaphore, args)
        {
        }

        protected internal override string? GetUrl(params string[] args)
        {
            return "https://www.furaffinity.net";
        }

        private async Task<ResourceContainer[]> GetValuesAsync(int index)
        {
            var doc = await GetHtmlDocumentAsync();
            var figures = doc.DocumentNode.SelectNodes($"//section[@class='gallery-section'][{index}]//figure");
            return Furaffinity.ParseResourceContainer(figures);
        }

        public async Task<ResourceContainer[]> GetRecentSubmissionsAsync()
        {
            return await GetValuesAsync(1);
        }

        public async Task<ResourceContainer[]> GetRecentWritingAndPoetryAsync()
        {
            return await GetValuesAsync(2);
        }

        public async Task<ResourceContainer[]> GetRecentMusicAndAudioAsync()
        {
            return await GetValuesAsync(3);
        }

        public async Task<ResourceContainer[]> GetFursuitingAndCraftsAsync()
        {
            return await GetValuesAsync(4);
        }
    }
}
