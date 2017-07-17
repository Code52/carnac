using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Carnac.Logic.KeyMonitor;
using Carnac.Logic.Models;
using Microsoft.Win32;
using System.Windows.Media;

namespace Carnac.Logic
{
    public class KeyProvider : IKeyProvider
    {
        readonly IInterceptKeys interceptKeysSource;
        readonly Dictionary<int, Process> processes;
        readonly IPasswordModeService passwordModeService;
        readonly IDesktopLockEventService desktopLockEventService;

        private readonly IList<Keys> modifierKeys =
            new List<Keys>
                {
                    Keys.LControlKey,
                    Keys.RControlKey,
                    Keys.LShiftKey,
                    Keys.RShiftKey,
                    Keys.LMenu,
                    Keys.RMenu,
                    Keys.ShiftKey,
                    Keys.Shift,
                    Keys.Alt,
                    Keys.LWin,
                    Keys.RWin
                };

        private bool winKeyPressed;

        [DllImport("User32.dll")]
        private static extern int GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public KeyProvider(IInterceptKeys interceptKeysSource, IPasswordModeService passwordModeService, IDesktopLockEventService desktopLockEventService)
        {
            processes = new Dictionary<int, Process>();
            this.interceptKeysSource = interceptKeysSource;
            this.passwordModeService = passwordModeService;
            this.desktopLockEventService = desktopLockEventService;
        }

        public IObservable<KeyPress> GetKeyStream()
        {
            // We are using an observable create to tie the lifetimes of the session switch stream and the keystream
            return Observable.Create<KeyPress>(observer =>
            {
                // When desktop is locked we will not get the keyup, because we track the windows key
                // specially we need to set it to not being pressed anymore
                var sessionSwitchStreamSubscription = desktopLockEventService.GetSessionSwitchStream()
                .Subscribe(ss =>
                {
                    if (ss.Reason == SessionSwitchReason.SessionLock)
                        winKeyPressed = false;
                }, observer.OnError);

                var keyStreamSubsription = interceptKeysSource.GetKeyStream()
                    .Select(DetectWindowsKey)
                    .Where(k => !IsModifierKeyPress(k) && k.KeyDirection == KeyDirection.Down)
                    .Select(ToCarnacKeyPress)
                    .Where(keypress => keypress != null)
                    .Where(k => !passwordModeService.CheckPasswordMode(k.InterceptKeyEventArgs))
                    .Subscribe(observer);

                return new CompositeDisposable(sessionSwitchStreamSubscription, keyStreamSubsription);
            });
        }

        InterceptKeyEventArgs DetectWindowsKey(InterceptKeyEventArgs interceptKeyEventArgs)
        {
            if (interceptKeyEventArgs.Key == Keys.LWin || interceptKeyEventArgs.Key == Keys.RWin)
            {
                if (interceptKeyEventArgs.KeyDirection == KeyDirection.Up)
                    winKeyPressed = false;
                else if (interceptKeyEventArgs.KeyDirection == KeyDirection.Down)
                    winKeyPressed = true;
            }

            return interceptKeyEventArgs;
        }

        bool IsModifierKeyPress(InterceptKeyEventArgs interceptKeyEventArgs)
        {
            return modifierKeys.Contains(interceptKeyEventArgs.Key);
        }

        KeyPress ToCarnacKeyPress(InterceptKeyEventArgs interceptKeyEventArgs)
        {
            var process = GetAssociatedProcess();
            if (process == null)
            {
                return null;
            }

            var isLetter = interceptKeyEventArgs.IsLetter();
            var inputs = ToInputs(isLetter, winKeyPressed, interceptKeyEventArgs).ToArray();
            try
            {
                string processFileName = process.MainModule.FileName;
                ImageSource image = IconUtilities.GetProcessIconAsImageSource(processFileName);
                return new KeyPress(new ProcessInfo(process.ProcessName, image), interceptKeyEventArgs, winKeyPressed, inputs);
            }
            catch (Exception)
            {
                return new KeyPress(new ProcessInfo(process.ProcessName), interceptKeyEventArgs, winKeyPressed, inputs); ;
            }
        }

        static IEnumerable<string> ToInputs(bool isLetter, bool isWinKeyPressed, InterceptKeyEventArgs interceptKeyEventArgs)
        {
            var controlPressed = interceptKeyEventArgs.ControlPressed;
            var altPressed = interceptKeyEventArgs.AltPressed;
            var shiftPressed = interceptKeyEventArgs.ShiftPressed;
            if (controlPressed)
                yield return "Ctrl";
            if (altPressed)
                yield return "Alt";
            if (isWinKeyPressed)
                yield return "Win";

            if (controlPressed || altPressed)
            {
                //Treat as a shortcut, don't be too smart
                if (shiftPressed)
                    yield return "Shift";

                yield return interceptKeyEventArgs.Key.SanitiseLower();
            }
            else
            {
                yield return interceptKeyEventArgs.Key.Sanitise();
            }
        }

        Process GetAssociatedProcess()
        {
            var handle = GetForegroundWindow();

            if (processes.ContainsKey(handle))
            {
                return processes[handle];
            }

            uint processId;
            GetWindowThreadProcessId(new IntPtr(handle), out processId);
            try
            {
                var p = Process.GetProcessById(Convert.ToInt32(processId));
                processes.Add(handle, p);
                return p;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}