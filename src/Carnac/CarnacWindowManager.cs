using System.Windows;
using Caliburn.Micro;

namespace Carnac
{
    public class CarnacWindowManager : WindowManager
    {
        public Window CreateWindow(object rootModel)
        {
            return CreateWindow(rootModel, false, null, null);
        }
    }
}