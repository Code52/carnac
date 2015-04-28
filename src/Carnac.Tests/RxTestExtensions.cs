using System;

namespace Carnac.Tests
{
    public static class RxTestExtensions
    {
        public static long Seconds(this int seconds)
        {
            return TimeSpan.FromSeconds(seconds).Ticks;
        }

        public static long Seconds(this double seconds)
        {
            return TimeSpan.FromSeconds(seconds).Ticks;
        }
    }
}