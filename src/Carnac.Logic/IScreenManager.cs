using System.Collections.Generic;
using Carnac.Logic.Native;

namespace Carnac.Logic
{
    public interface IScreenManager
    {
        IEnumerable<DetailedScreen> GetScreens();
    }
}
