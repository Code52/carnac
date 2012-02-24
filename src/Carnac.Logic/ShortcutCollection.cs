using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Carnac.Logic.Models;

namespace Carnac.Logic
{
    public class ShortcutCollection : Collection<KeyShortcut>
    {
        public string Group { get; set; }
        public string Process { get; set; }

        public IEnumerable<KeyShortcut> GetShortcutsMatching(IEnumerable<KeyPress> keys)
        {
            var matches = this.Where(s => s.EndsWith(keys));
            return matches;
        }
    }
}