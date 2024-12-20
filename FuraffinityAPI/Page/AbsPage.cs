﻿using HtmlAgilityPack;
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
        protected SimpleHttpClient httpClient;
        protected string[] args;
        protected OrderedSemaphore semaphore;

        internal AbsPage(SimpleHttpClient httpClient, OrderedSemaphore semaphore, params string[] args)
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
                    var title = doc.DocumentNode.SelectSingleNode("//title").InnerText.Trim();
                    var notice = doc.DocumentNode.SelectSingleNode("//section[contains(@class,'notice-message')]");
                    if (notice != null || title == "System Error")
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
                string text = await httpClient.SimpleGetStringAsync(url);
                return text;
            }
            catch
            {
                throw;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public override string ToString()
        {
            return textHtml.Result;
        }
    }
}
