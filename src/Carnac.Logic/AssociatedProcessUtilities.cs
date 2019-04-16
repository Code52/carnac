using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Carnac.Logic
{
    public static class AssociatedProcessUtilities
    {
        private static readonly Dictionary<int, Process> processes = new Dictionary<int, Process>();

        [DllImport("User32.dll")]
        private static extern int GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public static Process GetAssociatedProcess()
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
