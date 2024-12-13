using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuraffinityAPI
{
    public class OrderedSemaphore
    {
        private readonly int _maxCount;
        private int _currentCount;
        private readonly ConcurrentQueue<TaskCompletionSource<bool>> _queue;

        public OrderedSemaphore(int initialCount, int maxCount)
        {
            if (initialCount < 0 || initialCount > maxCount)
                throw new ArgumentOutOfRangeException(nameof(initialCount));

            _currentCount = initialCount;
            _maxCount = maxCount;
            _queue = new ConcurrentQueue<TaskCompletionSource<bool>>();
        }

        public OrderedSemaphore(int initialCount) : this(initialCount, initialCount) { }

        public Task WaitAsync()
        {
            lock (_queue)
            {
                if (_currentCount > 0)
                {
                    _currentCount--;
                    return Task.CompletedTask;
                }
                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                _queue.Enqueue(tcs);
                return tcs.Task;
            }
        }

        public void Release()
        {
            lock (_queue)
            {
                if (_queue.TryDequeue(out var tcs))
                {
                    tcs.SetResult(true);
                }
                else if (_currentCount < _maxCount)
                {
                    _currentCount++;
                }
                else
                {
                    throw new SemaphoreFullException("Semaphore is already at maximum count.");
                }
            }
        }
    }
}
