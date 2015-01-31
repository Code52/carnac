using System.Reactive.Concurrency;
using System.Threading;

namespace Carnac
{
    public class ConcurrencyService : IConcurrencyService
    {
        readonly IScheduler uiScheduler;
        readonly IScheduler defaultScheduler;

        public ConcurrencyService()
        {
            uiScheduler = new SynchronizationContextScheduler(SynchronizationContext.Current);
            defaultScheduler = Scheduler.Default;
        }

        public IScheduler UiScheduler
        {
            get { return uiScheduler; }
        }

        public IScheduler Default
        {
            get { return defaultScheduler; }
        }
    }
}