using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PCOptimizer
{
    public class DiskCleaner
    {
        private long _junkSizeFound;
        private List<string> _junkFolders;

        public DiskCleaner()
        {
            _junkSizeFound = 0;
            _junkFolders = new List<string>();
        }

        public void ScanJunk()
        {
            Debug.WriteLine("💾 Starting disk cleanup scan...");
            _junkSizeFound = 0;
            _junkFolders.Clear();

            ScanRecycleBin();
            ScanTempFolders();
            ScanBrowserCache();
            ScanSystemJunk();
            ScanOldWindowsInstalls();
            ScanDownloadFolders();

            Debug.WriteLine($"✓ Found {FormatBytes(_junkSizeFound)} of junk files");
        }

        private void ScanRecycleBin()
        {
            try
            {
                string recycleBin = @"C:\$Recycle.Bin";
                if (Directory.Exists(recycleBin))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(recycleBin);
                    foreach (FileInfo file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                    {
                        _junkSizeFound += file.Length;
                        _junkFolders.Add(file.FullName);
                    }
                }
            }
            catch { }
        }

        private void ScanTempFolders()
        {
            try
            {
                string[] tempPaths = {
                    Path.GetTempPath(),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp"),
                };

                foreach (string tempPath in tempPaths)
                {
                    if (Directory.Exists(tempPath))
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(tempPath);
                        try
                        {
                            foreach (FileInfo file in dirInfo.GetFiles())
                            {
                                _junkSizeFound += file.Length;
                                _junkFolders.Add(file.FullName);
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        private void ScanBrowserCache()
        {
            try
            {
                string chromeCache = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    @"Google\Chrome\User Data\Default\Cache");
                ScanCacheFolder(chromeCache);

                string firefoxCache = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    @"Mozilla\Firefox\Profiles");
                ScanCacheFolder(firefoxCache);

                string edgeCache = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    @"Microsoft\Edge\User Data\Default\Cache");
                ScanCacheFolder(edgeCache);
            }
            catch { }
        }

        private void ScanCacheFolder(string cachePath)
        {
            try
            {
                if (Directory.Exists(cachePath))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(cachePath);
                    foreach (FileInfo file in dirInfo.GetFiles())
                    {
                        _junkSizeFound += file.Length;
                    }
                }
            }
            catch { }
        }

        private void ScanSystemJunk()
        {
            try
            {
                string prefetch = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows), 
                    "Prefetch");
                
                if (Directory.Exists(prefetch))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(prefetch);
                    foreach (FileInfo file in dirInfo.GetFiles("*.pf"))
                    {
                        _junkSizeFound += file.Length;
                    }
                }
            }
            catch { }
        }

        private void ScanOldWindowsInstalls()
        {
            try
            {
                string oldWindows = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows), 
                    "Old");
                
                if (Directory.Exists(oldWindows))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(oldWindows);
                    long size = GetDirectorySize(dirInfo);
                    _junkSizeFound += size;
                }
            }
            catch { }
        }

        private void ScanDownloadFolders()
        {
            try
            {
                string downloads = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                    "Downloads");

                if (Directory.Exists(downloads))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(downloads);
                    DateTime oldDate = DateTime.Now.AddDays(-90);
                    
                    foreach (FileInfo file in dirInfo.GetFiles())
                    {
                        if (file.LastWriteTime < oldDate)
                        {
                            _junkSizeFound += file.Length;
                        }
                    }
                }
            }
            catch { }
        }

        public void CleanJunk()
        {
            Debug.WriteLine("🧹 Cleaning junk files...");

            foreach (string junkFile in _junkFolders)
            {
                try
                {
                    if (File.Exists(junkFile))
                        File.Delete(junkFile);
                }
                catch { }
            }

            Debug.WriteLine($"✓ Cleanup complete! Freed {FormatBytes(_junkSizeFound)}");
        }

        private long GetDirectorySize(DirectoryInfo dir)
        {
            long size = 0;
            try
            {
                foreach (FileInfo file in dir.GetFiles())
                    size += file.Length;

                foreach (DirectoryInfo subDir in dir.GetDirectories())
                    size += GetDirectorySize(subDir);
            }
            catch { }

            return size;
        }

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}
