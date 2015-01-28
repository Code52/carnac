using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Win32;

namespace Carnac.Logic.KeyMonitor
{
    public class DesktopLockEventService : IDesktopLockEventService
    {
        public IObservable<SessionSwitchEventArgs> GetSessionSwitchStream()
        {
            // Cannot use Observable.FromEventPattern as it causes an exception about security or something
            return Observable.Create<SessionSwitchEventArgs>(observer =>
            {
                SessionSwitchEventHandler handler = (sender, args) =>
                {
                    observer.OnNext(args);
                };

                SystemEvents.SessionSwitch += handler;
                return Disposable.Create(() => SystemEvents.SessionSwitch -= handler);
            });
        }
    }
}