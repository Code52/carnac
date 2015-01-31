using System;
using Microsoft.Win32;

namespace Carnac.Logic.KeyMonitor
{
    public interface IDesktopLockEventService
    {
        IObservable<SessionSwitchEventArgs> GetSessionSwitchStream();
    }
}