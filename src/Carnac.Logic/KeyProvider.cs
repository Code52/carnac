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

            return new KeyPress(process, interceptKeyEventArgs);
        }
    }
}