using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;


namespace Carnac.Logic
{
    /// <summary>
    /// Utility methods for processes
    /// </summary>
    public static class ProcessUtil
    {
        //Code is inspired by http://www.pinvoke.net/default.aspx/kernel32.createtoolhelp32snapshot

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr CreateToolhelp32Snapshot([In] UInt32 dwFlags, [In] UInt32 th32ProcessID);
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool Process32First([In] IntPtr hSnapshot, ref PROCESSENTRY32 lppe);
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool Process32Next([In] IntPtr hSnapshot, ref PROCESSENTRY32 lppe);
        [DllImport("kernel32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle([In] IntPtr hObject);

        //inner enum used only internally
        [Flags]
        private enum SnapshotFlags : uint
        {
            HeapList = 0x00000001,
            Process = 0x00000002,
            Thread = 0x00000004,
            Module = 0x00000008,
            Module32 = 0x00000010,
            Inherit = 0x80000000,
            All = 0x0000001F,
            NoHeaps = 0x40000000
        }
        //inner struct used only internally
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct PROCESSENTRY32
        {
            const int MAX_PATH = 260;
            internal UInt32 dwSize;
            internal UInt32 cntUsage;
            internal UInt32 th32ProcessID;
            internal IntPtr th32DefaultHeapID;
            internal UInt32 th32ModuleID;
            internal UInt32 cntThreads;
            internal UInt32 th32ParentProcessID;
            internal Int32 pcPriClassBase;
            internal UInt32 dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            internal string szExeFile;
        }

        /// <summary>
        /// Gets the first child process matching the regext
        /// </summary>
        /// <param name="exec">full path to the executable</param>
        /// <returns>a list containing all processes currently run by the given executable</returns>
        public static Process GetFirstChildMatching(int parentProcessId, Regex filterRegex)
        {
            var handleToSnapShot = IntPtr.Zero;

            try
            {
                var procEntry = new PROCESSENTRY32();
                procEntry.dwSize = (UInt32)Marshal.SizeOf(typeof(PROCESSENTRY32));
                handleToSnapShot = CreateToolhelp32Snapshot((uint)SnapshotFlags.Process, 0);

                if (!Process32First(handleToSnapShot, ref procEntry))
                {
                    return null;
                }

                do
                {
                    if (parentProcessId != procEntry.th32ParentProcessID)
                    {
                        continue;
                    }

                    if (filterRegex.IsMatch(procEntry.szExeFile))
                    {
                        return Process.GetProcessById((int)procEntry.th32ProcessID);
                    }

                    var subChild = GetFirstChildMatching((int)procEntry.th32ProcessID, filterRegex);
                    if (subChild != null)
                    {
                        return subChild;
                    }

                } while (Process32Next(handleToSnapShot, ref procEntry));
                return null;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"GetChildProcesses exception: {e.Message}");
            }
            finally
            {
                if (IntPtr.Zero != handleToSnapShot)
                {
                    CloseHandle(handleToSnapShot);
                }
            }
            return null;
        }
    }
}
