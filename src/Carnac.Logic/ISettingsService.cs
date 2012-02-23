using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace Carnac.Logic
{
    [InheritedExport]
    public interface ISettingsService
    {
        bool ContainsKey(string key);
        T Get<T>(string key);
        void Set<T>(string key, T value);
        void Save();
    }
}
