using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ParentsGuard.Types
{
    public class ThreadPool
    {
        public List<(EventWaitHandle, Thread)> AlwaysAliveThreads { get; } = new List<(EventWaitHandle, Thread)>();
        public List<(CancellationTokenSource, Thread)> WorkerThreads { get; } = new List<(CancellationTokenSource, Thread)>();

        public void Clean()
        {
            foreach (var threadToRemove in WorkerThreads.Where(x => x.Item2?.IsAlive == false))
            {
                WorkerThreads.Remove(threadToRemove);
            }
        }

        public void Suspend()
        {
            foreach (var thread in AlwaysAliveThreads)
            {
                thread.Item1.Reset();
            }
        }

        public void Resume()
        {
            foreach (var thread in AlwaysAliveThreads)
            {
                thread.Item1.Set();
            }
        }

        public void AbortWorker((CancellationTokenSource, Thread) worker)
            => worker.Item1.Cancel();

        public void AbortWorkers()
        {
            foreach (var worker in WorkerThreads)
            {
                AbortWorker(worker);
            }
        }
    }
}
