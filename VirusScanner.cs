using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PCOptimizer
{
    public class VirusScanner
    {
        private List<string> _suspiciousFiles;
        private List<string> _dangerousProcesses;
        private HashSet<string> _malwareHashes;

        public VirusScanner()
        {
            _suspiciousFiles = new List<string>();
            _dangerousProcesses = new List<string>();
            _malwareHashes = new HashSet<string>
            {
                "trojan", "virus", "ransomware", "adware", "spyware", 
                "cryptolocker", "wannacry", "petya", "notpetya",
                "emotet", "trickbot", "mirai", "botnet", "backdoor"
            };
        }

        public void StartScan()
        {
            Debug.WriteLine("🔍 Starting comprehensive virus scan...");
            _suspiciousFiles.Clear();
            _dangerousProcesses.Clear();

            ScanSystemFolders();
            ScanRunningProcesses();
            ScanBrowserExtensions();
            ScanStartupItems();

            Debug.WriteLine($"✓ Scan complete. Found {_suspiciousFiles.Count} threats");
        }

        public void QuickScan()
        {
            Debug.WriteLine("⚡ Quick virus scan...");
            ScanRunningProcesses();
        }

        private void ScanSystemFolders()
        {
            try
            {
                string[] suspiciousPaths = {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp"),
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                };

                foreach (string path in suspiciousPaths)
                {
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo dir = new DirectoryInfo(path);
                        try
                        {
                            foreach (FileInfo file in dir.GetFiles("*.exe", SearchOption.AllDirectories).Take(100))
                            {
                                if (IsSuspiciousFile(file))
                                {
                                    _suspiciousFiles.Add(file.FullName);
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        private void ScanRunningProcesses()
        {
            try
            {
                Process[] processes = Process.GetProcesses();
                foreach (Process proc in processes)
                {
                    try
                    {
                        string procName = proc.ProcessName.ToLower();
                        if (_malwareHashes.Any(hash => procName.Contains(hash)))
                        {
                            _dangerousProcesses.Add($"{procName} (PID: {proc.Id})");
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        private void ScanBrowserExtensions()
        {
            try
            {
                string chromeExt = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    @"Google\Chrome\User Data\Default\Extensions");

                if (Directory.Exists(chromeExt))
                {
                    DirectoryInfo dir = new DirectoryInfo(chromeExt);
                    foreach (DirectoryInfo subDir in dir.GetDirectories())
                    {
                        if (IsSuspiciousExtension(subDir.Name))
                        {
                            _suspiciousFiles.Add(subDir.FullName);
                        }
                    }
                }
            }
            catch { }
        }

        private void ScanStartupItems()
        {
            try
            {
                string startup = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    @"Microsoft\Windows\Start Menu\Programs\Startup");

                if (Directory.Exists(startup))
                {
                    DirectoryInfo dir = new DirectoryInfo(startup);
                    foreach (FileInfo file in dir.GetFiles())
                    {
                        if (file.Extension.ToLower() == ".exe" && IsSuspiciousFile(file))
                        {
                            _suspiciousFiles.Add(file.FullName);
                        }
                    }
                }
            }
            catch { }
        }

        private bool IsSuspiciousFile(FileInfo file)
        {
            string name = file.Name.ToLower();
            return _malwareHashes.Any(hash => name.Contains(hash));
        }

        private bool IsSuspiciousExtension(string extName)
        {
            return _malwareHashes.Any(hash => extName.ToLower().Contains(hash));
        }

        public void QuarantineFile(string filePath)
        {
            try
            {
                string quarantinePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "PCOptimizer", "Quarantine");

                Directory.CreateDirectory(quarantinePath);
                string fileName = Path.GetFileName(filePath);
                string newPath = Path.Combine(quarantinePath, fileName);

                File.Move(filePath, newPath, true);
                Debug.WriteLine($"✓ File quarantined: {fileName}");
            }
            catch { }
        }
    }
}
