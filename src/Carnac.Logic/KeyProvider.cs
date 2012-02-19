using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Carnac.Logic.KeyMonitor;

namespace Carnac.Logic
{
    public class KeyProvider : IObservable<KeyPress>
    {
        private readonly IObservable<InterceptKeyEventArgs> interceptKeysSource;
        private readonly Dictionary<int, Process> processes;

        private readonly List<Keys> modifierKeys =
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
                };

        [DllImport("User32.dll")]
        private static extern int GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public KeyProvider(IObservable<InterceptKeyEventArgs> interceptKeysSource)
        {
            processes = new Dictionary<int, Process>();
            this.interceptKeysSource = interceptKeysSource;
        }

        public IDisposable Subscribe(IObserver<KeyPress> observer)
        {
            return interceptKeysSource
                .Where(k => !IsModifierKeyPress(k) && k.KeyDirection == KeyDirection.Down)
                .Select(ToCarnacKeyPress)
                .Subscribe(observer);
        }

        private bool IsModifierKeyPress(InterceptKeyEventArgs interceptKeyEventArgs)
        {
            return modifierKeys.Contains(interceptKeyEventArgs.Key);
        }

        private KeyPress ToCarnacKeyPress(InterceptKeyEventArgs interceptKeyEventArgs)
        {
            var process = GetAssociatedProcess();

            var isLetter = interceptKeyEventArgs.Key >= Keys.A &&
                           interceptKeyEventArgs.Key <= Keys.Z;

            var inputs = ToInputs(isLetter, interceptKeyEventArgs);

            return new KeyPress(process, interceptKeyEventArgs, isLetter, inputs);
        }

        private IEnumerable<string> ToInputs(bool isLetter, InterceptKeyEventArgs interceptKeyEventArgs)
        {
            var controlPressed = interceptKeyEventArgs.ControlPressed;
            var altPressed = interceptKeyEventArgs.AltPressed;
            var shiftPressed = interceptKeyEventArgs.ShiftPressed;
            if (controlPressed)
                yield return "Ctrl";
            if (altPressed)
                yield return "Alt";

            if (controlPressed || altPressed)
            {
                //Treat as a shortcut, don't be too smart
                if (shiftPressed)
                    yield return "Shift";

                yield return interceptKeyEventArgs.Key.Sanitise();
            }
            else
            {
                string input;
                var shiftModifiesInput = interceptKeyEventArgs.Key.SanitiseShift(out input);

                if (!isLetter && !shiftModifiesInput && shiftPressed)
                    yield return "Shift";

                if (interceptKeyEventArgs.ShiftPressed && shiftModifiesInput)
                    yield return input;
                else if (isLetter && !interceptKeyEventArgs.ShiftPressed)
                    yield return interceptKeyEventArgs.Key.ToString().ToLower();
                else
                    yield return interceptKeyEventArgs.Key.Sanitise();
            }
        }

        private Process GetAssociatedProcess()
        {
            Process process;

            int handle = GetForegroundWindow();

            if (!processes.ContainsKey(handle))
            {
                uint processID;
                GetWindowThreadProcessId(new IntPtr(handle), out processID);
                var p = Process.GetProcessById(Convert.ToInt32(processID));
                processes.Add(handle, p);
                process = p;
            }
            else
                process = processes[handle];
            return process;
        }
    }
}