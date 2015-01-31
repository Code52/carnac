using System.Reactive.Concurrency;

namespace Carnac
{
    interface IConcurrencyService
    {
        IScheduler UiScheduler { get; }
        IScheduler Default { get; }
    }
}