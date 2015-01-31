using System.Reactive.Concurrency;

namespace Carnac.Logic
{
    public interface IConcurrencyService
    {
        IScheduler MainThreadScheduler { get; }
        IScheduler Default { get; }
    }
}