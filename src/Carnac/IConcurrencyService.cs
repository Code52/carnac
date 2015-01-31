using System.Reactive.Concurrency;

namespace Carnac
{
    public interface IConcurrencyService
    {
        IScheduler UiScheduler { get; }
        IScheduler Default { get; }
    }
}