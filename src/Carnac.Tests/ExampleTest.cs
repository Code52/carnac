using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows.Forms;
using Carnac.Logic;
using Carnac.Logic.KeyMonitor;
using Xunit;

namespace Carnac.Tests
{
    public class ExampleTest
    {
        [Fact]
        public void this_is_how_to_write_a_carnac_test()
        {
            //Start off by running KeyStreamCapture, hit start capture, type what you want, hit end capture, 
            //copy the code into the test, then clean it up (code gen is ugly code)... Ctrl + E, C (R# Code Cleanup) should do
            //In this example I pressed Ctrl + Shift + L

            // arrange
            var ctrlShiftLSource = CtrlShiftL();

            // act
            var provider = new KeyProvider(ctrlShiftLSource);
            //I havent got any further.. Thoughts? :P
        }

        private IObservable<InterceptKeyEventArgs> CtrlShiftL()
        {
            var keys = new List<InterceptKeyEventArgs>
                           {
                               new InterceptKeyEventArgs(Keys.LControlKey, KeyDirection.Down, false, false, false),
                               new InterceptKeyEventArgs(Keys.LShiftKey, KeyDirection.Down, false, true, false),
                               new InterceptKeyEventArgs(Keys.L, KeyDirection.Down, false, true, true),
                               new InterceptKeyEventArgs(Keys.L, KeyDirection.Up, false, true, true),
                               new InterceptKeyEventArgs(Keys.LShiftKey, KeyDirection.Up, false, true, true),
                               new InterceptKeyEventArgs(Keys.LControlKey, KeyDirection.Up, false, true, false)
                           };
            return keys.ToObservable();
        }
    }
}