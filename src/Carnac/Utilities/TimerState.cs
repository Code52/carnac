using System;
using System.Timers;
using System.Windows;
using System.Windows.Threading;

namespace Carnac.Utilities
{
    public class TimerState : IDisposable
    {
        readonly Timer timer;
        bool isDisposing;

        public TimerState(int period, Action callback)
        {
            timer = new Timer(period);
            timer.Elapsed +=
                (s, e) =>
                    {
                        if (Application.Current == null || Application.Current.Dispatcher == null) return;
                        Application.Current.Dispatcher.BeginInvoke(callback, DispatcherPriority.Background, null);
                    };
        }

        public void Dispose()
        {
            if (isDisposing)
                return;

            isDisposing = true;
            timer.Stop();
        }

        public void Start()
        {
            timer.Start();
        }
    }
}