using System.Collections.Generic;
using Carnac.Logic.Models;

namespace Carnac.Logic
{
    public interface IShortcutProvider
    {
        List<KeyShortcut> GetShortcutsStartingWith(KeyPress keyPress);
    }
}