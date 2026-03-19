using System;
using System.Diagnostics;
using System.Management;

namespace PCOptimizer
{
    public class SystemStats
    {
        public double CPUUsage { get; set; }
        public double RAMUsage { get; set; }
        public double DiskUsage { get; set; }
        public double Temperature { get; set; }
    }

    public class SystemMonitor
    {
        private PerformanceCounter _cpuCounter;
        private PerformanceCounter _ramCounter;

        public SystemMonitor()
        {
            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
                _ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use", true);
            }
            catch { }
        }

        public SystemStats GetSystemStats()
        {
            var stats = new SystemStats();

            try
            {
                stats.CPUUsage = _cpuCounter?.NextValue() ?? 0;
            }
            catch { stats.CPUUsage = 0; }

            try
            {
                stats.RAMUsage = _ramCounter?.NextValue() ?? 0;
            }
            catch { stats.RAMUsage = 0; }

            try
            {
                DriveInfo drive = new DriveInfo("C");
                if (drive.TotalSize > 0)
                {
                    stats.DiskUsage = (double)(drive.TotalSize - drive.AvailableFreeSpace) / drive.TotalSize * 100;
                }
            }
            catch { stats.DiskUsage = 0; }

            try
            {
                stats.Temperature = GetCPUTemperature();
            }
            catch { stats.Temperature = 0; }

            return stats;
        }

        private double GetCPUTemperature()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"\\.\root\WMI", "Select * from MSAcpi_ThermalZoneTemperature");
                ManagementObjectCollection collection = searcher.Get();

                foreach (ManagementObject obj in collection)
                {
                    double tempKelvin = Convert.ToDouble(obj["CurrentTemperature"].ToString()) / 10;
                    double tempCelsius = tempKelvin - 273.15;
                    return tempCelsius;
                }
            }
            catch { }

            return 0;
        }
    }
}
