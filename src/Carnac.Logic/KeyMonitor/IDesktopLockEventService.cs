using System;

namespace Carnac.Logic.KeyMonitor
{
    public interface IDesktopLockEventService : IDisposable
    {
        event EventHandler<EventArgs> DesktopUnlockedEvent;
        event EventHandler<EventArgs> DesktopLockedEvent;
    }
}