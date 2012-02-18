using System;
using System.ComponentModel.Composition;

namespace Carnac.Utilities
{
    [InheritedExport]
    public interface ITimerFactory
    {
        IDisposable Start(int period, Action callback);
    }
}
