using System.Runtime.InteropServices;

namespace Carnac.ViewModels
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINTL
    {
        public int x;
        public int y;
    }
}