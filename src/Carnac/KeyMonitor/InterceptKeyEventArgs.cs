using System;
using System.Windows.Forms;

namespace Carnac.KeyMonitor
{
    public class InterceptKeyEventArgs : EventArgs
    {
        public InterceptKeyEventArgs(Keys key, KeyDirection keyDirection, bool altPressed, bool controlPressed)
        {
            AltPressed = altPressed;
            ControlPressed = controlPressed;
            Key = key;
            KeyDirection = keyDirection;
        }

        public bool Handled { get; set; }
        public bool AltPressed { get; private set; }
        public bool ControlPressed { get; private set; }
        public Keys Key { get; private set; }
        public KeyDirection KeyDirection { get; private set; }
    }
}