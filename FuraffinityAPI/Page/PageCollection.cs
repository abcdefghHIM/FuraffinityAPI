using FuraffinityAPI.Struct;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FuraffinityAPI.Page
{
    public class PageCollection<T> : IDisposable
    {
        private object _lock;
        private bool disposed = false;
        private bool isReadEnd = false;
        private AutoResetEvent autoReset;
        private ConcurrentQueue<ResourceContainer[]> queue;
        private ConcurrentQueue<string[]> strings;
        private Task task;

        internal PageCollection(SimpleHttpClient httpClient, OrderedSemaphore semaphore, string userName)
        {
            _lock = new object();
            autoReset = new AutoResetEvent(false);
            queue = new ConcurrentQueue<ResourceContainer[]>();
            strings = new ConcurrentQueue<string[]>();
            task = new Task(() =>
            {
                if (typeof(T) == typeof(GalleryPage))
                    NewGalleryTask(httpClient, semaphore, userName);
                else if (typeof(T) == typeof(FavoritesPage))
                    NewFavoritesTask(httpClient, semaphore, userName);
                else if (typeof(T) == typeof(StarPage))
                    NewStarTask(httpClient, semaphore, userName);
                else if (typeof(T) == typeof(FansPage))
                    NewFansTask(httpClient, semaphore, userName);

            });
            task.Start();
        }

        public ResourceContainer[]? GetResourceContainers()
        {
            if (disposed)
                return null;
            ResourceContainer[]? obj;
            if (queue.Count != 0)
            {
                if (queue.TryDequeue(out obj))
                {
                    return obj;
                }
                return [];
            }
            if (!isReadEnd)
            {
                lock (_lock)
                {
                    if (queue.Count != 0)
                    {
                        if (queue.TryDequeue(out obj))
                        {
                            return obj;
                        }
                        return [];
                    }
                }
                autoReset.WaitOne();
                if (queue.TryDequeue(out obj))
                {
                    return obj;
                }
            }
            return null;
        }

        public string[]? GetStrings()
        {
            if (disposed)
                return null;
            string[]? obj;
            if (strings.Count != 0)
            {
                if (strings.TryDequeue(out obj))
                {
                    return obj;
                }
                return [];
            }
            if (!isReadEnd)
            {
                lock (_lock)
                {
                    if (strings.Count != 0)
                    {
                        if (strings.TryDequeue(out obj))
                        {
                            return obj;
                        }
                        return [];
                    }
                }
                autoReset.WaitOne();
                if (strings.TryDequeue(out obj))
                {
                    return obj;
                }
            }
            return null;
        }

        internal void NewGalleryTask(SimpleHttpClient httpClient, OrderedSemaphore semaphore, string userName)
        {
            string? url = $"/gallery/{userName}";
            while (true)
            {
                if (disposed)
                {
                    autoReset.Set();
                    break;
                }
                lock (_lock)
                {
                    if (url == null)
                    {
                        isReadEnd = true;
                        autoReset.Set();
                        break;
                    }
                    GalleryPage page = new GalleryPage(httpClient, semaphore, url);
                    url = page.GetNextUrlAsync().Result;
                    queue.Enqueue(page.GetArrayAsync().Result);
                    autoReset.Set();
                }
            }
        }

        internal void NewFavoritesTask(SimpleHttpClient httpClient, OrderedSemaphore semaphore, string userName)
        {
            string? url = $"/favorites/{userName}";
            while (true)
            {
                if (disposed)
                {
                    autoReset.Set();
                    break;
                }
                lock (_lock)
                {
                    if (url == null)
                    {
                        isReadEnd = true;
                        autoReset.Set();
                        break;
                    }
                    FavoritesPage page = new FavoritesPage(httpClient, semaphore, url);
                    url = page.GetNextUrlAsync().Result;
                    queue.Enqueue(page.GetArrayAsync().Result);
                    autoReset.Set();
                }
            }
        }

        internal void NewStarTask(SimpleHttpClient httpClient, OrderedSemaphore semaphore, string userName)
        {
            string? url = $"/watchlist/by/{userName}";
            while (true)
            {
                if (disposed)
                {
                    autoReset.Set();
                    break;
                }
                lock (_lock)
                {
                    if (url == null)
                    {
                        isReadEnd = true;
                        autoReset.Set();
                        break;
                    }
                    StarPage page = new StarPage(httpClient, semaphore, url);
                    url = page.GetNextUrlAsync().Result;
                    strings.Enqueue(page.GetArrayAsync().Result);
                    autoReset.Set();
                }
            }
        }

        internal void NewFansTask(SimpleHttpClient httpClient, OrderedSemaphore semaphore, string userName)
        {
            string? url = $"/watchlist/to/{userName}";
            while (true)
            {
                if (disposed)
                {
                    autoReset.Set();
                    break;
                }
                lock (_lock)
                {
                    if (url == null)
                    {
                        isReadEnd = true;
                        autoReset.Set();
                        break;
                    }
                    FansPage page = new FansPage(httpClient, semaphore, url);
                    url = page.GetNextUrlAsync().Result;
                    strings.Enqueue(page.GetArrayAsync().Result);
                    autoReset.Set();
                }
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            task?.Dispose();
            queue.Clear();
            strings.Clear();
            GC.SuppressFinalize(this);
        }

        ~PageCollection()
        {
            Dispose();
        }
    }
}
