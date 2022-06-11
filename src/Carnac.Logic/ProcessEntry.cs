using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace Carnac.Logic
{
    public class ProcessEntry
    {
        private Process parentProcess;
        private string parentOriginalTitle;
        private Process useChildProcess;

        private static readonly Dictionary<int, ProcessEntry> processList = new Dictionary<int, ProcessEntry>();

        /// <summary>
        /// Optimized to quickly find the a child process with a regex.
        /// Scanning is only done when the parent process's title-bar text changes, which is enough
        /// of a test for shells such as cmd and powershell.
        /// </summary>
        /// <param name="process"></param>
        /// <param name="filterRegex"></param>
        /// <returns></returns>
        public static bool HasChildProcessMatching(Process process, Regex filterRegex)
        {
            ProcessEntry processEntry = null;
            if (filterRegex != null)
            {
                // Scanning for child processes is expensive,
                // so only scan if the title bar of this parent process has changed
                // parent process is a shell such as cmd.exe or powershell.exe.
                // We don't check if it's "a shell" and do this processing for any similar process.
                var shouldScan = false;

                // Keep track of previous scans.  If it's the first time, add the entry and scan.
                if (!processList.TryGetValue(process.Id, out processEntry))
                {
                    processEntry = AddProcess(process);
                    shouldScan = true;
                    Debug.WriteLine("new entry");
                }
                else
                {
                    // If parent process has exited, rescan
                    if (processEntry.parentProcess.HasExited)
                    {
                        processList.Remove(process.Id);
                        processEntry = AddProcess(process);
                        shouldScan = true;
                        Debug.WriteLine("parent process exited");
                    }

                    // If there's an existing child process we're supposed to use, validate that it's still there, otherwise rescan
                    if (!shouldScan && processEntry.useChildProcess?.HasExited == true)
                    {
                        shouldScan = true;
                        Debug.WriteLine("child process exited");
                    }
                }

                // If there's no active child, and parent's title's changed, rescan.
                if (!shouldScan && !IsChildProcessActive(processEntry) && IsTitleChanged(processEntry))
                {
                    shouldScan = true;
                }

                if (shouldScan)
                {
                    // This is done rarely because scanning is expensive.
                    ScanChildren(filterRegex, processEntry);
                }
            }

            // Return true if a child process matching regex is found.
            return IsChildProcessActive(processEntry);
        }

        private static bool IsChildProcessActive(ProcessEntry processEntry)
        {
            return
                processEntry?.useChildProcess != null ||
                processEntry?.useChildProcess?.HasExited == true;
        }

        private static void ScanChildren(Regex filterRegex, ProcessEntry processEntry)
        {
            Debug.WriteLine("Scan children");
            processEntry.useChildProcess = null;
            processEntry.parentProcess.Refresh();
            processEntry.parentOriginalTitle = processEntry.parentProcess.MainWindowTitle;

            // If any children match the regex, then use that child process from now on.
            processEntry.useChildProcess = ProcessUtil.GetFirstChildMatching(processEntry.parentProcess.Id, filterRegex);

            if (processEntry.useChildProcess != null)
            {
                Debug.WriteLine($"  Using child: '{processEntry.useChildProcess?.ProcessName ?? string.Empty}'");
            }
        }

        private static bool IsTitleChanged(ProcessEntry processEntry)
        {
            // Needed for get get the latest MainWindowTitle.
            processEntry.parentProcess.Refresh();

            // Determine if the title of this parent process has changed.
            Debug.WriteLine($"     BEFORE: '{processEntry.parentOriginalTitle}'");
            Debug.WriteLine($"      AFTER: '{processEntry.parentProcess.MainWindowTitle}'");
            var titleHasChanged = processEntry.parentOriginalTitle != processEntry.parentProcess.MainWindowTitle;
            if (titleHasChanged)
            {
                Debug.WriteLine("title changed");
                return true;
            }

            return false;
        }

        static ProcessEntry AddProcess(Process process)
        {
            process.Refresh();
            var processEntry = new ProcessEntry { parentProcess = process, parentOriginalTitle = process.MainWindowTitle };
            processList.Add(process.Id, processEntry);
            return processEntry;
        }
    }
}