using System.Collections.Generic;
using System.Linq;
using Carnac.Logic.KeyMonitor;

namespace Carnac.Logic.Models
{
    public sealed class KeyPress : KeyPressDefinition
    {
        public KeyPress(ProcessInfo process, InterceptKeyEventArgs interceptKeyEventArgs, bool winkeyPressed, IEnumerable<string> input) :
            base(interceptKeyEventArgs.Key, winkeyPressed, interceptKeyEventArgs.ShiftPressed, interceptKeyEventArgs.AltPressed, interceptKeyEventArgs.ControlPressed)
        {
            Process = process;
            InterceptKeyEventArgs = interceptKeyEventArgs;
            Input = input;
        }

        public ProcessInfo Process { get; private set; }

        public InterceptKeyEventArgs InterceptKeyEventArgs { get; private set; }

        public IEnumerable<string> Input { get; private set; }

        public bool HasModifierPressed
        {
            get
            {
                return InterceptKeyEventArgs.AltPressed
                    || InterceptKeyEventArgs.ControlPressed
                    || WinkeyPressed;
            }
        }

        public IEnumerable<string> GetTextParts()
        {
            var isFirst = true;
            foreach (var text in Input)
            {
                if (!isFirst)
                {
                    yield return " + ";
                }
                else
                {
                    isFirst = false;
                }
                yield return Format(text, HasModifierPressed);
            }
        }
        
        static string Format(string text, bool isShortcut)
        {
            if (text == "Left")
                return GetString(8592);
            if (text == "Up")
                return GetString(8593);
            if (text == "Right")
                return GetString(8594);
            if (text == "Down")
                return GetString(8595);

            // If the space is part of a shortcut sequence
            // present it as a primitive key. E.g. Ctrl+Space.
            // Otherwise we want to preserve a space as part of
            // what is probably a sentence.
            if (text == " " && isShortcut)
                return "Space";

            return text;
        }

        static string GetString(int decimalValue)
        {
            return new string(new[] { (char)decimalValue });
        }

        #region Equality overides

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((KeyPress)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Process != null ? Process.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (InterceptKeyEventArgs != null ? InterceptKeyEventArgs.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Input != null ? Input.GetHashCode() : 0);
                return hashCode;
            }
        }

        bool Equals(KeyPress other)
        {
            return base.Equals(other)
                && Equals(Process, other.Process)
                && Equals(InterceptKeyEventArgs, other.InterceptKeyEventArgs)
                && Input.SequenceEqual(other.Input);
        }
        #endregion
    }
}