using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuraffinityAPI.Page
{
    public class FansPage : StarPage
    {
        internal FansPage(HttpClient httpClient, OrderedSemaphore semaphore, params string[] args) : base(httpClient, semaphore, args)
        {
        }
    }
}
