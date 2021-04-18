// http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266988.aspx

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.AniDbMetaStructure.Infrastructure
{
    public class AsyncSemaphore
    {
        private static readonly Task Completed = Task.FromResult(true);
        private int currentCount;
        private readonly Queue<TaskCompletionSource<bool>> waiters = new Queue<TaskCompletionSource<bool>>();

        public AsyncSemaphore(int initialCount)
        {
            if (initialCount < 0)
            {
                throw new ArgumentOutOfRangeException("initialCount");
            }

            this.currentCount = initialCount;
        }

        public void Release()
        {
            TaskCompletionSource<bool> toRelease = null;
            lock (this.waiters)
            {
                if (this.waiters.Count > 0)
                {
                    toRelease = this.waiters.Dequeue();
                }
                else
                {
                    ++this.currentCount;
                }
            }

            toRelease?.SetResult(true);
        }

        public Task WaitAsync()
        {
            lock (this.waiters)
            {
                if (this.currentCount > 0)
                {
                    --this.currentCount;
                    return Completed;
                }

                var waiter = new TaskCompletionSource<bool>();
                this.waiters.Enqueue(waiter);
                return waiter.Task;
            }
        }
    }

    public class AsyncLock
    {
        private readonly Task<Releaser> releaser;
        private readonly AsyncSemaphore semaphore;

        public AsyncLock()
        {
            this.semaphore = new AsyncSemaphore(1);
            this.releaser = Task.FromResult(new Releaser(this));
        }

        public Task<Releaser> LockAsync()
        {
            var wait = this.semaphore.WaitAsync();
            return wait.IsCompleted ? this.releaser : wait.ContinueWith(
                (_, state) => new Releaser((AsyncLock)state),
                this,
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        public struct Releaser : IDisposable
        {
            private readonly AsyncLock toRelease;

            internal Releaser(AsyncLock toRelease)
            {
                this.toRelease = toRelease;
            }

            public void Dispose()
            {
                this.toRelease?.semaphore.Release();
            }
        }
    }
}