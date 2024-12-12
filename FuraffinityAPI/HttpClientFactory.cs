using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FuraffinityAPI
{
    internal class HttpClientFactory : IDisposable
    {
        private readonly ConcurrentDictionary<string, HttpClient> httpClients;
        private readonly CookieContainer cookieContainer;
        private bool disposed = false;

        public HttpClientFactory()
        {
            httpClients = new ConcurrentDictionary<string, HttpClient>();
            cookieContainer = new CookieContainer();
        }

        public HttpClient CreateClient(string name)
        {
            if (httpClients.ContainsKey(name))
            {
                return httpClients[name];
            }
            var handler = new HttpClientHandler()
            {
                CookieContainer = cookieContainer
            };
            var client = new HttpClient(handler);
            httpClients[name] = client;
            return client;
        }

        public void SetCookie(string a, string b)
        {
            cookieContainer.Add(new Cookie("a", a, "/", ".furaffinity.net"));
            cookieContainer.Add(new Cookie("b", b, "/", ".furaffinity.net"));
        }

        public CookieCollection GetCookies()
        {
            return cookieContainer.GetAllCookies();
        }

        public void Dispose()
        {
            if (disposed)
                return;
            foreach (var client in httpClients.Values)
            {
                client.Dispose();
            }
            disposed = true;
            GC.SuppressFinalize(this);
        }

        ~HttpClientFactory()
        {
            Dispose();
        }
    }
}
