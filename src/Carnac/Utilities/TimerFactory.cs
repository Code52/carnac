using System;

namespace Carnac.Utilities
{
    public class TimerFactory : ITimerFactory
    {
        public IDisposable Start(int period, Action callback)
        {
            var timerState = new TimerState(period, callback);
            timerState.Start();
            return timerState;
        }
    }
}