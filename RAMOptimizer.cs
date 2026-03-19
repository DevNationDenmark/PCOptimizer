using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PCOptimizer
{
    public class RAMOptimizer
    {
        [DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(IntPtr proc, IntPtr min, IntPtr max);

        public void OptimizeMemory()
        {
            Debug.WriteLine("⚡ Optimizing RAM...");
            
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                Process currentProcess = Process.GetCurrentProcess();
                SetProcessWorkingSetSize(currentProcess.Handle, new IntPtr(-1), new IntPtr(-1));

                foreach (Process proc in Process.GetProcesses())
                {
                    try
                    {
                        if (proc.ProcessName != "System" && proc.ProcessName != "svchost")
                        {
                            SetProcessWorkingSetSize(proc.Handle, new IntPtr(-1), new IntPtr(-1));
                        }
                    }
                    catch { }
                }

                Debug.WriteLine("✓ RAM optimization complete");
            }
            catch { }
        }
    }
}
