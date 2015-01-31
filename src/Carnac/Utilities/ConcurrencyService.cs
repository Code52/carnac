using System.Reactive.Concurrency;
using System.Threading;
using Carnac.Logic;

namespace Carnac.Utilities
{
    public class ConcurrencyService : IConcurrencyService
    {
        public ConcurrencyService()
        {
            MainThreadScheduler = new SynchronizationContextScheduler(SynchronizationContext.Current);
            Default = Scheduler.Default;
        }

        public IScheduler MainThreadScheduler { get; private set; }

        public IScheduler Default { get; private set; }
    }
}