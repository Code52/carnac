using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using Carnac.Logic.KeyMonitor;

namespace Carnac.Logic
{
    public class KeyProvider : IObservable<KeyPress>
    {
        private readonly IObservable<InterceptKeyEventArgs> interceptKeysSource;
        private readonly Dictionary<int, Process> processes;

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
                .Select(ToCarnacKeyPress)
                .Subscribe(observer);
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