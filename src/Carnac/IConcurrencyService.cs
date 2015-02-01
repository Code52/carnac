using System.Reactive.Concurrency;

namespace Carnac
{
    public interface IConcurrencyService
    {
        IScheduler MainThreadScheduler { get; }
        IScheduler Default { get; }
    }
}