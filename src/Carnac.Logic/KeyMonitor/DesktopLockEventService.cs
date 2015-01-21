using System;
using System.Reactive.Linq;
using Microsoft.Win32;

namespace Carnac.Logic.KeyMonitor
{
    public class DesktopLockEventService : IDesktopLockEventService
    {
        public IObservable<SessionSwitchEventArgs> GetSessionSwitchStream()
        {
            return Observable.FromEvent<SessionSwitchEventHandler, SessionSwitchEventArgs>(
                    add => SystemEvents.SessionSwitch += add, remove => SystemEvents.SessionSwitch -= remove);
        }
    }
}