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

        public ShortcutCollection(IList<KeyShortcut> values) 
            : base(values)
        {
            
        }

        public KeyShortcut[] GetShortcutsMatching(IEnumerable<KeyPress> keys)
        {
            return this.Where(s => s.StartsWith(keys)).ToArray();
        }
    }
}