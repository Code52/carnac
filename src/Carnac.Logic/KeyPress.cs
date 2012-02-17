using System.Diagnostics;
using Carnac.Logic.KeyMonitor;

namespace Carnac.Logic
{
    public class KeyPress
    {
        public Process Process { get; private set; }
        public InterceptKeyEventArgs InterceptKeyEventArgs { get; private set; }

        public KeyPress(Process process, InterceptKeyEventArgs interceptKeyEventArgs)
        {
            Process = process;
            InterceptKeyEventArgs = interceptKeyEventArgs;
        }
    }
}