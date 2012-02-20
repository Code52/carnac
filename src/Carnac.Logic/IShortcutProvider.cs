using System.Collections.Generic;
using Carnac.Logic.Models;

namespace Carnac.Logic
{
    public interface IShortcutProvider
    {
        IEnumerable<KeyShortcut> GetShortcutsMatching(IEnumerable<KeyPress> keys);
    }
}