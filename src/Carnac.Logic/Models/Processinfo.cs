namespace Carnac.Logic.Models
{
    public class Processinfo
    {
        public Processinfo(string processName)
        {
            ProcessName = processName;
        }

        public string ProcessName { get; private set; }
    }
}