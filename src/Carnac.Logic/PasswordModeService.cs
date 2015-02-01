using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Carnac.Logic.Internal;
using Carnac.Logic.KeyMonitor;

namespace Carnac.Logic
{
    public class PasswordModeService : IPasswordModeService
    {
        readonly InterceptKeyEventArgsEqualityComparer comparer = new InterceptKeyEventArgsEqualityComparer();
        readonly FixedQueue<InterceptKeyEventArgs> log;
        InterceptKeyEventArgs[] passwordKeyCombination;
        bool currentMode;

        public PasswordModeService()
        {
            log = new FixedQueue<InterceptKeyEventArgs>(this.PasswordKeyCombination.Count());
        }

        public bool CheckPasswordMode(InterceptKeyEventArgs key)
        {
            log.Enqueue(key);
            var sortedLog = log.ToList();
            sortedLog.Sort();
            var isMatch = sortedLog.SequenceEqual(PasswordKeyCombination, comparer);
            if (isMatch)
            {
                currentMode = !currentMode;
                this.log.Clear();
                return true; //this way when the sequence is entered again to EXIT password mode, the key password keycombo doesn't show on screen
            }

            return currentMode;
        }

        public IEnumerable<InterceptKeyEventArgs> PasswordKeyCombination
        {
            get
            {
                if (passwordKeyCombination == null)
                {
                    passwordKeyCombination = new[]
                                                 {
                                                     new InterceptKeyEventArgs(Keys.P, KeyDirection.Down,true,true,false), 
                                                 };
                }

                return passwordKeyCombination;
            }
        }

        class InterceptKeyEventArgsEqualityComparer : IEqualityComparer<InterceptKeyEventArgs>
        {
            public bool Equals(InterceptKeyEventArgs x, InterceptKeyEventArgs y)
            {
                if (x == null && y == null)
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }

                return x.Key == y.Key
                       && x.ShiftPressed == y.ShiftPressed
                       && x.AltPressed == y.AltPressed
                       && x.ControlPressed == y.ControlPressed;
            }

            public int GetHashCode(InterceptKeyEventArgs obj)
            {
                return obj.Key.GetHashCode() << obj.AltPressed.GetHashCode()
                       << obj.ShiftPressed.GetHashCode() << obj.ControlPressed.GetHashCode();
            }
        }
    }
}