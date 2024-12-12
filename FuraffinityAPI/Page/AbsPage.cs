using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuraffinityAPI.Page
{
    public abstract class AbsPage
    {
        private object _lock;
        private Task<string> textHtml;
        private HtmlDocument? doc;
        protected HttpClient httpClient;
        protected string[] args;
        protected SemaphoreSlim semaphore;

        internal AbsPage(HttpClient httpClient, SemaphoreSlim semaphore, params string[] args)
        {
            _lock = new object();
            var url = GetUrl(args);
            if (url == null)
            {
                throw new ArgumentNullException();
            }
            this.args = args;
            this.semaphore = semaphore;
            this.httpClient = httpClient;
            textHtml = GetStringAsync(url);
        }

        protected internal abstract string? GetUrl(params string[] args);

        protected async Task<HtmlDocument> GetHtmlDocumentAsync()
        {
            if (doc != null)
            {
                return doc;
            }
            var text = await textHtml;
            lock (_lock)
            {
                if (doc == null)
                {
                    doc = new HtmlDocument();
                    doc.LoadHtml(text);
                }
            }
            return doc;
        }

        protected async Task<string> GetStringAsync(string url)
        {
            await semaphore.WaitAsync();
            string text;
            text = await httpClient.GetStringAsync(url);
            semaphore.Release();
            return text;
        }

        public override string ToString()
        {
            return textHtml.Result;
        }
    }
}
