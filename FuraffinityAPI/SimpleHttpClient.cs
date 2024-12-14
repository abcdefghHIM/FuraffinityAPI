using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FuraffinityAPI
{
    internal class SimpleHttpClient: IDisposable
    {
        private bool disposed = false;
        private HttpClient? httpClient;
        private CookieContainer? cookieContainer;
        private int MaxRetries { get; set; } = 3;
        private int MaxTimeoutRetries { get; set; } = 5;
        private int DelayMilliseconds { get; set; } = 1000;

        public async Task<string> SimpleGetStringAsync(string? requestUri)
        {
            if (disposed)
                throw new TaskCanceledException();
            try
            {
                string text = "";
                int retry = 0;
                int timeout = 0;
                
                while (true)
                {
                    try
                    {
                        if (httpClient == null)
                        {
                            throw new NullReferenceException();
                        }
                        text = await httpClient.GetStringAsync(requestUri);
                        break;
                    }
                    catch (HttpRequestException ex) when ((int)(ex.StatusCode ?? 0) == 503)
                    {
                        Console.WriteLine("503 Faulted");
                        if (retry < MaxRetries - 1)
                        {
                            await Task.Delay(DelayMilliseconds * (retry + 1));
                            retry++;
                        }
                        else
                        {
                            throw;
                        }
                    }
                    catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
                    {
                        if (httpClient == null)
                        {
                            throw new NullReferenceException();
                        }
                        httpClient.Dispose();
                        if (timeout < MaxTimeoutRetries - 1)
                        {
                            await Task.Delay(DelayMilliseconds * (timeout + 1));
                            CreateHttpClient();
                            timeout++;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                return text;
            }
            catch
            {
                throw;
            }
        }

        public void CreateHttpClient(CookieContainer? cookieContainer=null)
        {
            if (disposed)
                return;
            if (cookieContainer == null)
                cookieContainer = this.cookieContainer;
            if (cookieContainer == null)
                return;
            var handler = new HttpClientHandler()
            {
                CookieContainer = cookieContainer
            };
            httpClient = new HttpClient(handler);
            
        }

        public void Dispose()
        {
            if (disposed)
                return;
            if (httpClient != null)
                httpClient.Dispose();
            disposed = true;
            GC.SuppressFinalize(this);
        }

        ~SimpleHttpClient()
        {
            Dispose();
        }
    }
}
