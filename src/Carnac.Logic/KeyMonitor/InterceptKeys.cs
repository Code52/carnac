using System;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;
using System.Windows.Input;

namespace Carnac.Logic.KeyMonitor
{
    [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class InterceptKeys : IObservable<InterceptKeyEventArgs>, IDisposable
    {
        static volatile InterceptKeys current;
        static readonly object CurrentLock = new object();
        readonly Win32Methods.LowLevelKeyboardProc callback;
        readonly Subject<InterceptKeyEventArgs> subject;
        bool disposed;
        IntPtr hookId = IntPtr.Zero;
        decimal subscriberCount;
        readonly bool disposeDesktopUnlockedEventService;
        readonly IDesktopLockEventService desktopLockEventService;
        Keys? winKeyAtDesktopLocked;

        InterceptKeys()
            :this(new DesktopLockEventService())
        {
            // if needed, the other c'tor can be used to pass in an object owned by something
            // else, in which case that owner would be responsible for disposing the object.
            disposeDesktopUnlockedEventService = true;
        }

        InterceptKeys(IDesktopLockEventService desktopLockEventService)
        {
            if (desktopLockEventService == null) throw new ArgumentNullException("desktopLockEventService");
            subject = new Subject<InterceptKeyEventArgs>();
            callback = HookCallback;
            this.desktopLockEventService = desktopLockEventService;
            this.desktopLockEventService.DesktopUnlockedEvent += DesktopLockEventServiceDesktopUnlockEvent;
            this.desktopLockEventService.DesktopUnlockedEvent += DesktopLockEventServiceDesktopLockEvent;
        }

        /// <summary>
        /// Handle desktop lock event.
        /// Keep track of the state of the Window key(s) when lock occurs so we can check their state
        /// at unlock and simulate a key event to mirror current state of the windows keys, if needed.
        /// </summary>
        /// <param name="sender">who sent the event; not used, should be EventArgs.Empty</param>
        /// <param name="e">the event args; not used, should be EventArgs.Empty</param>
        void DesktopLockEventServiceDesktopLockEvent(object sender, EventArgs e)
        {
            // save which Win key is pressed, if any.
            if(Keyboard.IsKeyDown(Key.LWin))
            {
                winKeyAtDesktopLocked = Keys.LWin;
            }
            else if (Keyboard.IsKeyDown(Key.RWin))
            {
                winKeyAtDesktopLocked = Keys.RWin;
            }
            else winKeyAtDesktopLocked = null;
        }

        /// <summary>
        /// Handle desktop unlock event.
        /// Simulate a key event to mirror current state of the windows keys, if needed.
        /// </summary>
        /// <param name="sender">who sent the event; not used, should be EventArgs.Empty</param>
        /// <param name="e">the event args; not used, should be EventArgs.Empty</param>
        void DesktopLockEventServiceDesktopUnlockEvent(object sender, EventArgs e)
        {
            if (!winKeyAtDesktopLocked.HasValue) return; // key wasn't pressed at lock
            if (Keyboard.IsKeyDown(winKeyAtDesktopLocked.Value == Keys.LWin ? Key.LWin : Key.RWin))
                return; // key still down

            var interceptKeyEventArgs = new InterceptKeyEventArgs(
                winKeyAtDesktopLocked.Value,
                KeyDirection.Up,
                Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt),
                Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl),
                Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift));

            subject.OnNext(interceptKeyEventArgs);
            Debug.Write(winKeyAtDesktopLocked.Value);
            if (interceptKeyEventArgs.Handled)
            {
                Debug.WriteLine(" handled");
            }
        }

        public static InterceptKeys Current
        {
            get
            {
                if (current == null)
                {
                    lock (CurrentLock)
                    {
                        if (current == null)
                        {
                            current = new InterceptKeys();
                        }
                    }
                }
                return current;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IDisposable Subscribe(IObserver<InterceptKeyEventArgs> observer)
        {
            Debug.WriteLine("Subscribed");
            IDisposable dispose = subject.Subscribe(observer);
            subscriberCount++;
            if (subscriberCount == 1)
                hookId = SetHook(callback);
            return new DelegateDisposable(() =>
                                              {
                                                  subscriberCount--;
                                                  if (subscriberCount == 0)
                                                      Win32Methods.UnhookWindowsHookEx(hookId);
                                                  dispose.Dispose();
                                              });
        }

        IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                bool alt = (Control.ModifierKeys & Keys.Alt) != 0;
                bool control = (Control.ModifierKeys & Keys.Control) != 0;
                bool shift = (Control.ModifierKeys & Keys.Shift) != 0;
                bool keyDown = wParam == (IntPtr)Win32Methods.WM_KEYDOWN;
                bool keyUp = wParam == (IntPtr)Win32Methods.WM_KEYUP;
                int vkCode = Marshal.ReadInt32(lParam);
                var key = (Keys)vkCode;
                //http://msdn.microsoft.com/en-us/library/windows/desktop/ms646286(v=vs.85).aspx
                if (key != Keys.RMenu && key != Keys.LMenu && wParam == (IntPtr)Win32Methods.WM_SYSKEYDOWN)
                {
                    alt = true;
                    keyDown = true;
                }
                if (key != Keys.RMenu && key != Keys.LMenu && wParam == (IntPtr)Win32Methods.WM_SYSKEYUP)
                {
                    alt = true;
                    keyUp = true;
                }

                var interceptKeyEventArgs = new InterceptKeyEventArgs(
                    key,
                    keyDown ?
                    KeyDirection.Down: keyUp 
                    ? KeyDirection.Up: KeyDirection.Unknown,
                    alt, control, shift);

                subject.OnNext(interceptKeyEventArgs);
                Debug.Write(key);
                if (interceptKeyEventArgs.Handled)
                {
                    Debug.WriteLine(" handled");
                    return (IntPtr)1; //handled                    
                }
            }

            return Win32Methods.CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        static IntPtr SetHook(Win32Methods.LowLevelKeyboardProc proc)
        {
            //TODO: This requires FullTrust to use the Process class - is there any options for doing this in MediumTrust?
            //
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return Win32Methods.SetWindowsHookEx(Win32Methods.WH_KEYBOARD_LL, proc,
                                                      Win32Methods.GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (subject != null)
                    {
                        subject.Dispose();
                    }

                    desktopLockEventService.DesktopUnlockedEvent -= DesktopLockEventServiceDesktopLockEvent;

                    if (disposeDesktopUnlockedEventService)
                    {
                        desktopLockEventService.Dispose();
                    }
                }

                disposed = true;
            }
        }

        class DelegateDisposable : IDisposable
        {
            readonly Action dispose;

            public DelegateDisposable(Action dispose)
            {
                this.dispose = dispose;
            }

            public void Dispose()
            {
                dispose();
            }
        }
    }
}