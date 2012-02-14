using System;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;

namespace Carnac.KeyMonitor
{
    [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class InterceptKeys : IObservable<InterceptKeyEventArgs>, IDisposable
    {
        static volatile InterceptKeys current;
        static readonly object CurrentLock = new object();
        readonly NativeMethods.LowLevelKeyboardProc callback;
        readonly Subject<InterceptKeyEventArgs> subject;
        bool disposed;
        IntPtr hookId = IntPtr.Zero;
        decimal subscriberCount;

        InterceptKeys()
        {
            subject = new Subject<InterceptKeyEventArgs>();
            callback = HookCallback;
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
                                                      NativeMethods.UnhookWindowsHookEx(hookId);
                                                  dispose.Dispose();
                                              });
        }

        IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                bool alt = (Control.ModifierKeys & Keys.Alt) != 0;
                bool control = (Control.ModifierKeys & Keys.Control) != 0;
                bool keyDown = wParam == (IntPtr)NativeMethods.WM_KEYDOWN;
                bool keyUp = wParam == (IntPtr)NativeMethods.WM_KEYUP;
                int vkCode = Marshal.ReadInt32(lParam);
                var key = (Keys)vkCode;

                var interceptKeyEventArgs = new InterceptKeyEventArgs(key,
                                                                      keyDown
                                                                          ? KeyDirection.Down
                                                                          : keyUp
                                                                                ? KeyDirection.Up
                                                                                : KeyDirection.Unknown,
                                                                      alt, control);
                subject.OnNext(interceptKeyEventArgs);
                Debug.Write(key);
                if (interceptKeyEventArgs.Handled)
                {
                    Debug.WriteLine(" handled");
                    return (IntPtr)1; //handled                    
                }
            }

            return NativeMethods.CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        static IntPtr SetHook(NativeMethods.LowLevelKeyboardProc proc)
        {
            //TODO: This requires FullTrust to use the Process class - is there any options for doing this in MediumTrust?
            //
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, proc,
                                                      NativeMethods.GetModuleHandle(curModule.ModuleName), 0);
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