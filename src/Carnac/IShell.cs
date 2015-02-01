using System.Collections.ObjectModel;
using Carnac.Logic.Models;

namespace Carnac {
    public interface IShell {
        ObservableCollection<Message> Keys { get; }
        PopupSettings Settings { get; set; }
    }
}
