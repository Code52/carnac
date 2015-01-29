using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Carnac.Logic.KeyMonitor;

namespace Carnac.Logic.Models
{
    public class KeyPress : KeyPressDefinition
    {
        public KeyPress(Processinfo process, InterceptKeyEventArgs interceptKeyEventArgs, bool winkeyPressed, IEnumerable<string> input):
            base(interceptKeyEventArgs.Key, winkeyPressed, interceptKeyEventArgs.ShiftPressed, interceptKeyEventArgs.AltPressed, interceptKeyEventArgs.ControlPressed)
        {
            Process = process;
            InterceptKeyEventArgs = interceptKeyEventArgs;
            Input = input;
            Timestamp = DateTime.Now;
        }

        public DateTime Timestamp { get; private set; }

        public Processinfo Process { get; private set; }

        public InterceptKeyEventArgs InterceptKeyEventArgs { get; private set; }

        public IEnumerable<string> Input { get; private set; }

        public bool HasModifierPressed
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
                return !HasModifierPressed && InterceptKeyEventArgs.Key >= Keys.A && InterceptKeyEventArgs.Key <= Keys.Z;
            }
        }
    }
}