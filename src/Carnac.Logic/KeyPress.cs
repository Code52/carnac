using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Carnac.Logic.KeyMonitor;

namespace Carnac.Logic
{
    public class KeyPress
    {
        public KeyPress(Process process, InterceptKeyEventArgs interceptKeyEventArgs, bool winkeyPressed, IEnumerable<string> input)
        {
            Process = process;
            InterceptKeyEventArgs = interceptKeyEventArgs;
            WinkeyPressed = winkeyPressed;
            Input = input;
        }

        public Process Process { get; private set; }
        public InterceptKeyEventArgs InterceptKeyEventArgs { get; private set; }
        protected bool WinkeyPressed { get; private set; }
        public IEnumerable<string> Input { get; private set; }

        public bool IsShortcut
        {
            get
            {
                return InterceptKeyEventArgs.AltPressed || InterceptKeyEventArgs.ControlPressed ||
                       WinkeyPressed;
            }
        }

        public bool IsLetter
        {
            get
            {
                return InterceptKeyEventArgs.Key >= Keys.A && InterceptKeyEventArgs.Key <= Keys.Z;
            }
        }
    }
}