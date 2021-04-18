using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.AniDbMetaStructure.Infrastructure
{
    internal class RateLimiters : IRateLimiters
    {
        public static readonly IRateLimiters Instance = new RateLimiters();

        private RateLimiters()
        {
            this.AniDb = new RateLimiter(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5),
                TimeSpan.FromMinutes(5), new SemaphoreSlim(1, 1));
        }

        public IRateLimiter AniDb { get; }

        /// <summary>
        ///     The RateLimiter class attempts to regulate the rate at which an event occurs, by delaying
        ///     new occurances of the event.
        /// </summary>
        /// <remarks>
        ///     The <see cref="RateLimiter" /> will allow bursts of activity (down to a minimum occurance interval),
        ///     but attempts to maintain a minimum interval between occurances over a given time window.
        /// </remarks>
        private class RateLimiter : IRateLimiter
        {
            private readonly AsyncLock _lock;
            private readonly int maxAllowedInWindow;
            private readonly TimeSpan minimumInterval;
            private readonly TimeSpan targetInterval;
            private readonly TimeSpan timeWindowDuration;

            private readonly List<DateTime> window;

            private DateTime lastTake;

            /// <summary>
            ///     Creates a new instance of the <see cref="RateLimiter" /> class.
            /// </summary>
            /// <param name="minimumInterval">The minimum time between events.</param>
            /// <param name="targetInterval">The target average time between events.</param>
            /// <param name="timeWindow">The time span over which the average rate is calculated.</param>
            public RateLimiter(TimeSpan minimumInterval, TimeSpan targetInterval, TimeSpan timeWindow,
                SemaphoreSlim semaphore)
            {
                this.window = new List<DateTime>();
                this._lock = new AsyncLock();
                this.minimumInterval = minimumInterval;
                this.targetInterval = targetInterval;
                this.timeWindowDuration = timeWindow;
                this.Semaphore = semaphore;

                this.maxAllowedInWindow = (int)(timeWindow.Ticks / targetInterval.Ticks);

                this.lastTake = DateTime.Now - minimumInterval;
            }

            /// <summary>
            ///     Attempts to trigger an event, waiting if needed.
            /// </summary>
            /// <returns>A task which completes when it is safe to proceed.</returns>
            public async Task TickAsync()
            {
                using (await this._lock.LockAsync())
                {
                    var wait = this.CalculateWaitDuration();
                    if (wait.Ticks > 0)
                    {
                        await Task.Delay(wait);
                    }

                    var now = DateTime.Now;
                    this.window.Add(now);
                    this.lastTake = now;
                }
            }

            public SemaphoreSlim Semaphore { get; }

            private TimeSpan CalculateWaitDuration()
            {
                this.FlushExpiredRecords();
                if (this.window.Count == 0)
                {
                    return TimeSpan.Zero;
                }

                var now = DateTime.Now;
                var minWait = this.lastTake + this.minimumInterval - now;

                var load = (float)this.window.Count / this.maxAllowedInWindow;

                var waitTicks = minWait.Ticks + (this.targetInterval.Ticks - minWait.Ticks) * load;
                return new TimeSpan((long)waitTicks);
            }

            private void FlushExpiredRecords()
            {
                var now = DateTime.Now;
                while (this.window.Count > 0 && now - this.window[0] > this.timeWindowDuration)
                    this.window.RemoveAt(0);
            }
        }
    }
}