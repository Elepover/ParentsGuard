using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ParentsGuard.Types
{
    public class ThreadPool
    {
        public EventWaitHandle EventWaitHandle { get; } = new EventWaitHandle(true, EventResetMode.ManualReset);
        public List<Thread> AlwaysAliveThreads { get; } = new List<Thread>();
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
            EventWaitHandle.Reset();
        }

        public void Resume()
        {
            EventWaitHandle.Set();
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
