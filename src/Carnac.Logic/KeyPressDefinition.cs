using System.Windows.Forms;

namespace Carnac.Logic
{
    public class KeyPressDefinition
    {
        public KeyPressDefinition(
            Keys key,
            bool winkeyPressed = false,
            bool shiftPressed = false,
            bool altPressed = false,
            bool controlPressed = false)
        {
            Key = key;
            ControlPressed = controlPressed;
            AltPressed = altPressed;
            ShiftPressed = shiftPressed;
            WinkeyPressed = winkeyPressed;
        }

        public Keys Key { get; private set; }
        public bool ControlPressed { get; private set; }
        public bool AltPressed { get; private set; }
        public bool ShiftPressed { get; private set; }
        public bool WinkeyPressed { get; private set; }

        public bool Equals(KeyPressDefinition other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Key, Key) &&
                   other.ControlPressed.Equals(ControlPressed) &&
                   other.AltPressed.Equals(AltPressed) &&
                   other.ShiftPressed.Equals(ShiftPressed) &&
                   other.WinkeyPressed.Equals(WinkeyPressed);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(KeyPressDefinition)) return false;
            return Equals((KeyPressDefinition)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = Key.GetHashCode();
                result = (result * 397) ^ ControlPressed.GetHashCode();
                result = (result * 397) ^ AltPressed.GetHashCode();
                result = (result * 397) ^ ShiftPressed.GetHashCode();
                result = (result * 397) ^ WinkeyPressed.GetHashCode();
                return result;
            }
        }
    }
}