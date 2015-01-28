using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;
using Carnac.Logic.Win32Methods;

namespace Carnac.Logic.KeyMonitor
{
    [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public class InterceptKeys : IInterceptKeys
    {
        public static readonly InterceptKeys Current = new InterceptKeys();
        readonly IObservable<InterceptKeyEventArgs> keyStream;
        LowLevelKeyboardProc callback;

        private InterceptKeys()
        {
            keyStream = Observable.Create<InterceptKeyEventArgs>(observer =>
            {
                Debug.Write("Subscribed to keys");
                IntPtr hookId = IntPtr.Zero;
                // Need to hold onto this callback, otherwise it will get GC'd as it is an unmanged callback
                callback = (nCode, wParam, lParam) =>
                {
                    if (nCode >= 0)
                    {
                        var eventArgs = CreateEventArgs(wParam, lParam);
                        observer.OnNext(eventArgs);
                        if (eventArgs.Handled)
                            return (IntPtr)1;
                    }

                    return CallNextHookEx(hookId, nCode, wParam, lParam);
                };
                hookId = SetHook(callback);
                return Disposable.Create(() =>
                {
                    Debug.Write("Unsubscribed from keys");
                    UnhookWindowsHookEx(hookId);
                    callback = null;
                });
            })
            .Publish().RefCount();
        }

        public IObservable<InterceptKeyEventArgs> GetKeyStream()
        {
            return keyStream;
        }

        InterceptKeyEventArgs CreateEventArgs(IntPtr wParam, IntPtr lParam)
        {
            bool alt = (Control.ModifierKeys & Keys.Alt) != 0;
            bool control = (Control.ModifierKeys & Keys.Control) != 0;
            bool shift = (Control.ModifierKeys & Keys.Shift) != 0;
            bool keyDown = wParam == (IntPtr)WM_KEYDOWN;
            bool keyUp = wParam == (IntPtr)WM_KEYUP;
            int vkCode = Marshal.ReadInt32(lParam);
            var key = (Keys)vkCode;
            //http://msdn.microsoft.com/en-us/library/windows/desktop/ms646286(v=vs.85).aspx
            if (key != Keys.RMenu && key != Keys.LMenu && wParam == (IntPtr)WM_SYSKEYDOWN)
            {
                alt = true;
                keyDown = true;
            }
            if (key != Keys.RMenu && key != Keys.LMenu && wParam == (IntPtr)WM_SYSKEYUP)
            {
                alt = true;
                keyUp = true;
            }

            return new InterceptKeyEventArgs(
                key,
                keyDown ?
                KeyDirection.Down : keyUp
                ? KeyDirection.Up : KeyDirection.Unknown,
                alt, control, shift);
        }

        static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            //TODO: This requires FullTrust to use the Process class - is there any options for doing this in MediumTrust?
            //
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }
    }
}