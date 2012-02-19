using System.Windows.Forms;
using Carnac.Logic.KeyMonitor;

namespace Carnac.Tests
{
    public static class KeyStreams
    {
        public static KeyPlayer WinkeyE()
        {
            return new KeyPlayer
                       {
                           new InterceptKeyEventArgs(Keys.LWin, KeyDirection.Down, false, false, false),
                           new InterceptKeyEventArgs(Keys.E, KeyDirection.Down, false, false, false),
                           new InterceptKeyEventArgs(Keys.E, KeyDirection.Up, false, false, false),
                           new InterceptKeyEventArgs(Keys.LWin, KeyDirection.Up, false, false, false),
                       };
        }

        public static KeyPlayer ExclaimationMark()
        {
            return new KeyPlayer
                       {
                           new InterceptKeyEventArgs(Keys.LShiftKey, KeyDirection.Down, false, false, false),
                           new InterceptKeyEventArgs(Keys.D1, KeyDirection.Down, false, false, true),
                           new InterceptKeyEventArgs(Keys.D1, KeyDirection.Up, false, false, true),
                           new InterceptKeyEventArgs(Keys.LShiftKey, KeyDirection.Up, false, false, true),
                       };
        }

        public static KeyPlayer Number1()
        {
            return new KeyPlayer
                       {
                           new InterceptKeyEventArgs(Keys.D1, KeyDirection.Down, false, false, false),
                           new InterceptKeyEventArgs(Keys.D1, KeyDirection.Up, false, false, false)
                       };
        }

        public static KeyPlayer LetterL()
        {
            return new KeyPlayer
                       {
                           new InterceptKeyEventArgs(Keys.L, KeyDirection.Down, false, false, false),
                           new InterceptKeyEventArgs(Keys.L, KeyDirection.Up, false, false, false),
                       };
        }

        public static KeyPlayer ShiftL()
        {
            return new KeyPlayer
                       {
                           new InterceptKeyEventArgs(Keys.LShiftKey, KeyDirection.Down, false, false, false),
                           new InterceptKeyEventArgs(Keys.L, KeyDirection.Down, false, false, true),
                           new InterceptKeyEventArgs(Keys.L, KeyDirection.Up, false, false, true),
                           new InterceptKeyEventArgs(Keys.LShiftKey, KeyDirection.Up, false, false, true),
                       };
        }

        public static KeyPlayer CtrlShiftL()
        {
            return new KeyPlayer
                       {
                           new InterceptKeyEventArgs(Keys.LControlKey, KeyDirection.Down, false, false, false),
                           new InterceptKeyEventArgs(Keys.LShiftKey, KeyDirection.Down, false, true, false),
                           new InterceptKeyEventArgs(Keys.L, KeyDirection.Down, false, true, true),
                           new InterceptKeyEventArgs(Keys.L, KeyDirection.Up, false, true, true),
                           new InterceptKeyEventArgs(Keys.LShiftKey, KeyDirection.Up, false, true, true),
                           new InterceptKeyEventArgs(Keys.LControlKey, KeyDirection.Up, false, true, false)
                       };
        }

        public static KeyPlayer CtrlU()
        {
            return new KeyPlayer
                       {
                           new InterceptKeyEventArgs(Keys.LControlKey, KeyDirection.Down, false, false, false),
                           new InterceptKeyEventArgs(Keys.U, KeyDirection.Down, false, true, false),
                           new InterceptKeyEventArgs(Keys.U, KeyDirection.Up, false, true, false),
                           new InterceptKeyEventArgs(Keys.LControlKey, KeyDirection.Up, false, true, false)
                       };
        }
    }
}