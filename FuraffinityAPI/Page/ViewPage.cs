using FuraffinityAPI.Struct;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuraffinityAPI.Page
{
    public class ViewPage : AbsPage
    {
        internal ViewPage(HttpClient httpClient, OrderedSemaphore semaphore, params string[] args) : base(httpClient, semaphore, args)
        {
        }

        protected internal override string? GetUrl(params string[] args)
        {
            if (args.Length == 0)
                return null;
            return $"https://www.furaffinity.net{args[0]}";
        }

        public async Task<ViewContainer> GetViewContainerAsync()
        {
            var doc = await GetHtmlDocumentAsync();
            return Furaffinity.ParseViewContainer(doc);
        }
    }
}
