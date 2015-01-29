using System.Collections.Generic;
using Carnac.Logic.KeyMonitor;

namespace Carnac.Logic
{
    public interface IPasswordModeService
    {
        bool CheckPasswordMode(InterceptKeyEventArgs key);
        IEnumerable<InterceptKeyEventArgs> PasswordKeyCombination { get; }
    }
}