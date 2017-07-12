using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Carnac.Logic.Models;
using YamlDotNet.RepresentationModel;

namespace Carnac.Logic
{
    public class ShortcutProvider : IShortcutProvider
    {
        readonly List<ShortcutCollection> shortcuts;

        public ShortcutProvider()
        {
            string folder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\Keymaps\";
            string filter = "*.yml";
            if (!Directory.Exists(folder))
            {
                shortcuts = new List<ShortcutCollection>();
                return;
            }
            string[] files = Directory.GetFiles(folder, filter);

            shortcuts = GetYamlMappings(files).Select(GetShortcuts).ToList();
        }

        public List<KeyShortcut> GetShortcutsStartingWith(KeyPress keys)
        {
            var processName = keys.Process.ProcessName;
            return shortcuts
                .Where(s => s.Process == processName || string.IsNullOrWhiteSpace(s.Process))
                .SelectMany(shortcut => shortcut.GetShortcutsMatching(new[] { keys }))
                .ToList();
        }

        static string GetValueByKey(YamlMappingNode node, string name)
        {
            return node.Children.First(n => n.Key.ToString() == name).Value.ToString();
        }

        static KeyPressDefinition GetKeyPressDefintion(string combo)
        {
            combo = combo.ToLower();
            var key = combo.Split('+').Last();
            var keys = ReplaceKey.ToKey(key);
            if (keys != null)
                return
                    new KeyPressDefinition
                        (keys.Value,
                         shiftPressed: combo.Contains("shift"),
                         controlPressed: combo.Contains("ctrl"),
                         altPressed: combo.Contains("alt"),
                         winkeyPressed: combo.Contains("win"));
            return null;
        }

        static IEnumerable<YamlMappingNode> GetYamlMappings(IEnumerable<string> filePaths)
        {
            var yaml = new YamlStream();

            foreach (var file in filePaths)
            {
                yaml.Load(File.OpenText(file));
                var root = yaml.Documents[0].RootNode;

                var collection = root as YamlMappingNode;
                if (collection != null)
                    yield return collection;
            }
        }

        static ShortcutCollection GetShortcuts(YamlMappingNode collection)
        {
            string group = GetValueByKey(collection, "group");
            string process = GetValueByKey(collection, "process");

            var shortCuts = from groupShortcuts in collection.Children.Where(n => n.Key.ToString() == "shortcuts").Take(1).Select(x => x.Value).OfType<YamlSequenceNode>()
                            from shortcut in GetKeyShortcuts(groupShortcuts)
                            select shortcut;

            return new ShortcutCollection(shortCuts.ToList())
            {
                Process = process, 
                Group = @group
            };
        }

        static IEnumerable<KeyShortcut> GetKeyShortcuts(YamlSequenceNode groupShortcuts)
        {
            return from entry in groupShortcuts.Children.OfType<YamlMappingNode>()
                   from keys in entry.Children.Where(n => n.Key.ToString() == "keys").Take(1).Select(x=>x.Value).OfType<YamlSequenceNode>()
                   let name = GetValueByKey(entry, "name")
                   from definitions in keys.Children.Select(KeyPressDefinitions).Where(definitions => definitions.Count > 0)
                   select new KeyShortcut(name, definitions.ToArray());
        }

        static List<KeyPressDefinition> KeyPressDefinitions(YamlNode keyCombo)
        {
            return keyCombo.ToString()
                .Split(',')
                .Select(GetKeyPressDefintion)
                .Where(definition => definition != null)
                .ToList();
        }
    }
}