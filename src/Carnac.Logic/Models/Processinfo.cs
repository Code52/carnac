namespace Carnac.Logic.Models
{
    public class ProcessInfo
    {
        public ProcessInfo(string processName)
        {
            ProcessName = processName;
        }

        public string ProcessName { get; private set; }
    }
}