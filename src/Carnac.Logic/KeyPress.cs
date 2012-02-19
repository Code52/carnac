using System.Collections.Generic;
using System.Diagnostics;
using Carnac.Logic.KeyMonitor;

namespace Carnac.Logic
{
    public class KeyPress
    {
        public Process Process { get; private set; }
        public InterceptKeyEventArgs InterceptKeyEventArgs { get; private set; }
        public bool IsLetter { get; private set; }
        public IEnumerable<string> Input { get; private set; }

        public KeyPress(Process process, InterceptKeyEventArgs interceptKeyEventArgs, bool isLetter, IEnumerable<string> input)
        {
            Process = process;
            InterceptKeyEventArgs = interceptKeyEventArgs;
            IsLetter = isLetter;
            Input = input;
        }
    }
}