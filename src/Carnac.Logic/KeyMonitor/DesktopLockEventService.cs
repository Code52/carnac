using System;
using Microsoft.Win32;

namespace Carnac.Logic.KeyMonitor
{
    public class DesktopLockEventService : IDesktopLockEventService
    {
        public event EventHandler<EventArgs> DesktopUnlockedEvent;
        public event EventHandler<EventArgs> DesktopLockedEvent;

        public DesktopLockEventService()
        {
            SystemEvents.SessionSwitch += OnSystemEventsOnSessionSwitch;
        }

        void OnSystemEventsOnSessionSwitch(object sender, SessionSwitchEventArgs args)
        {
            if (args.Reason == SessionSwitchReason.SessionUnlock)
            {
                OnDesktopUnlockedEvent();
            }
        }

        private void OnDesktopUnlockedEvent()
        {
            var handler = DesktopUnlockedEvent;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            SystemEvents.SessionSwitch += OnSystemEventsOnSessionSwitch;
        }
    }
}