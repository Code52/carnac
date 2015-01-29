using System.Collections.Generic;
using System.ComponentModel.Composition;
using Carnac.Logic.Native;

namespace Carnac.Logic
{
    [InheritedExport]
    public interface IScreenManager
    {
        IEnumerable<DetailedScreen> GetScreens();
    }
}
