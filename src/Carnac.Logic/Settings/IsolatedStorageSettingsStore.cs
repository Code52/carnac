using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Carnac.Logic.Settings
{
    public class IsolatedStorageSettingsStore : ISettingsStorage
    {
        const IsolatedStorageScope Scope = IsolatedStorageScope.Assembly | IsolatedStorageScope.User | IsolatedStorageScope.Roaming;

        public string SerializeList(List<string> listOfItems)
        {
            var ms = new MemoryStream();
            var writer = JsonReaderWriterFactory.CreateJsonWriter(ms);
            new DataContractJsonSerializer(typeof(List<string>)).WriteObject(ms, listOfItems);
            writer.Flush();
            var jsonString = Encoding.Default.GetString(ms.ToArray());

            return jsonString;
        }

        public List<string> DeserializeList(string serializedList)
        {
            return (List<string>)new DataContractJsonSerializer(typeof(List<string>))
                .ReadObject(new MemoryStream(Encoding.Default.GetBytes(serializedList)));
        }

        public void Save(string key, Dictionary<string, string> settings)
        {
            var filename = key + ".settings";

            var serializer = new DataContractJsonSerializer(typeof(Dictionary<string, string>));

            using (var isoStore = IsolatedStorageFile.GetStore(Scope, null, null))
            {
                using (var stream = new IsolatedStorageFileStream(filename, FileMode.Create, isoStore))
                    serializer.WriteObject(stream, settings);
            }
        }

        public Dictionary<string, string> Load(string key)
        {
            var filename = key + ".settings";

            var serializer = new DataContractJsonSerializer(typeof(Dictionary<string, string>));
            using (var isoStore = IsolatedStorageFile.GetStore(Scope, null, null))
            {
                if (isoStore.FileExists(filename))
                {
                    using (var stream = new IsolatedStorageFileStream(filename, FileMode.Open, isoStore))
                        return (Dictionary<string, string>)serializer.ReadObject(stream);
                }
            }

            return new Dictionary<string, string>();
        }
    }
}
