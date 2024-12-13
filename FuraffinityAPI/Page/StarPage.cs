using FuraffinityAPI.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuraffinityAPI.Page
{
    public class StarPage : AbsPage
    {
        private object _lock;
        private string[]? list;

        internal StarPage(HttpClient httpClient, OrderedSemaphore semaphore, params string[] args) : base(httpClient, semaphore, args)
        {
            _lock = new object();
        }

        protected internal override string? GetUrl(params string[] args)
        {
            if (args.Length == 0)
                return null;
            return $"https://www.furaffinity.net{args[0]}";
        }

        public async Task<string[]> GetArrayAsync()
        {
            if (list != null)
            {
                return list;
            }
            var doc = await GetHtmlDocumentAsync();
            lock (_lock)
            {
                if (list == null)
                {
                    var temp = doc.DocumentNode.SelectNodes("//div[contains(@class,'watch-list')]//a");
                    if (temp == null)
                    {
                        list = new string[] { };
                    }
                    else
                    {
                        list = temp.Select(e => e.InnerText).ToArray();
                    }

                }
            }
            return list;
        }

        public async Task<string?> GetNextUrlAsync()
        {
            var doc = await GetHtmlDocumentAsync();
            string[]? list = this.list;
            if (list == null)
            {
                list = await GetArrayAsync();
            }
            if (list.Length < 1000)
                return null;
            var form = doc.DocumentNode.SelectSingleNode("//div[@class='floatright']/form[.//button[normalize-space(text())='Next 1000']]");
            return form.Attributes["action"].Value;
        }
    }
}
