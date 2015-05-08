using System.Windows.Media;

namespace Carnac.Logic.Models
{
    public class ProcessInfo
    {
        public ProcessInfo(string processName)
        {
            ProcessName = processName;
        }

        public ProcessInfo(string processName, ImageSource processIcon)
        {
            ProcessName = processName;
            ProcessIcon = processIcon;
        }

        public string ProcessName { get; private set; }

        public ImageSource ProcessIcon { get; private set; }
    }
}