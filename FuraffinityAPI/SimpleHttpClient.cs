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
    public class SimpleHttpClient: IDisposable
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
                        Console.Write(ex.Message);
                        Console.WriteLine(ex.StackTrace);
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
                    catch (Exception ex) when ((ex is TaskCanceledException && !((TaskCanceledException)ex).CancellationToken.IsCancellationRequested) || ex is HttpRequestException)
                    {
                        Console.Write(ex.Message);
                        Console.WriteLine(ex.StackTrace);
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
            catch(Exception e)
            {
                Console.Write(e.Message);
                Console.WriteLine(e.StackTrace);
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
