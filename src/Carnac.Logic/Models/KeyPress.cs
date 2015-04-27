using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Carnac.Logic.KeyMonitor;

namespace Carnac.Logic.Models
{
    public class KeyPress : KeyPressDefinition
    {
        public KeyPress(ProcessInfo process, InterceptKeyEventArgs interceptKeyEventArgs, bool winkeyPressed, IEnumerable<string> input):
            base(interceptKeyEventArgs.Key, winkeyPressed, interceptKeyEventArgs.ShiftPressed, interceptKeyEventArgs.AltPressed, interceptKeyEventArgs.ControlPressed)
        {
            Process = process;
            InterceptKeyEventArgs = interceptKeyEventArgs;
            Input = input;
            Timestamp = DateTime.Now;
        }

        public DateTime Timestamp { get; private set; }

        public ProcessInfo Process { get; private set; }

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

        #region Equality overides
        protected bool Equals(KeyPress other)
        {
            return base.Equals(other) 
                && Timestamp.Equals(other.Timestamp) 
                && Equals(Process, other.Process) 
                && Equals(InterceptKeyEventArgs, other.InterceptKeyEventArgs) 
                && Input.SequenceEqual(other.Input);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((KeyPress) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ Timestamp.GetHashCode();
                hashCode = (hashCode*397) ^ (Process != null ? Process.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (InterceptKeyEventArgs != null ? InterceptKeyEventArgs.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Input != null ? Input.GetHashCode() : 0);
                return hashCode;
            }
        }
        #endregion
    }
}