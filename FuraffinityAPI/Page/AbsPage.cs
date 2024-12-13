using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
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
        protected OrderedSemaphore semaphore;

        internal AbsPage(HttpClient httpClient, OrderedSemaphore semaphore, params string[] args)
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
                    var title = doc.DocumentNode.SelectSingleNode("//title").InnerText;
                    if (title.Contains("System Error") || title.Contains("Account disabled"))
                    {
                        throw new InvalidDataException();
                    }
                }
            }
            return doc;
        }

        public async Task Notify()
        {
            await GetHtmlDocumentAsync();
        }

        protected async Task<string> GetStringAsync(string url)
        {
            await semaphore.WaitAsync();
            try
            {
                int maxRetries = 3;
                int delayMilliseconds = 1000;
                for (int retry = 0; retry < maxRetries; retry++)
                {
                    try
                    {
                        string text = await httpClient.GetStringAsync(url);
                        return text;
                    }
                    catch (HttpRequestException ex) when ((int)(ex.StatusCode ?? 0) == 503)
                    {
                        Console.WriteLine("503 Faulted");
                        if (retry < maxRetries - 1)
                        {
                            await Task.Delay(delayMilliseconds * (retry + 1));
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            finally
            {
                semaphore.Release();
            }
            throw new Exception();
        }

        public override string ToString()
        {
            return textHtml.Result;
        }
    }
}
