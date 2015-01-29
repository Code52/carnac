using Carnac.Logic.Models;

namespace Carnac.Logic
{
    public interface IShortcutProvider
    {
        KeyShortcut[] GetShortcutsStartingWith(KeyPress[] keys);
    }
}