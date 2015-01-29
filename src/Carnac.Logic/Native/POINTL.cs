using System.Runtime.InteropServices;

namespace Carnac.Logic.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINTL
    {
        public int x;
        public int y;
    }
}