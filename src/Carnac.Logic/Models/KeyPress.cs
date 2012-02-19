using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Carnac.Logic.KeyMonitor;

namespace Carnac.Logic.Models
{
    public class KeyPress : KeyPressDefinition
    {
        public KeyPress(Process process, InterceptKeyEventArgs interceptKeyEventArgs, bool winkeyPressed, IEnumerable<string> input):
            base(interceptKeyEventArgs.Key, winkeyPressed, interceptKeyEventArgs.ShiftPressed, interceptKeyEventArgs.AltPressed, interceptKeyEventArgs.ControlPressed)
        {
            Process = process;
            InterceptKeyEventArgs = interceptKeyEventArgs;
            Input = input;
        }

        public Process Process { get; private set; }
        public InterceptKeyEventArgs InterceptKeyEventArgs { get; private set; }
        public IEnumerable<string> Input { get; private set; }

        public bool IsShortcut
        {
            get
            {
                return InterceptKeyEventArgs.AltPressed || InterceptKeyEventArgs.ControlPressed || WinkeyPressed;
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