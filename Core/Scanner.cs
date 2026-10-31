using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace StartupAudit
{
    class Scanner
    {
        public List<StartupItem> Scan()
        {
            var list = new List<StartupItem>();
            ScanRegistry(list, Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "HKLM Run");
            ScanRegistry(list, Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "HKCU Run");
            ScanFolder(list, Environment.GetFolderPath(Environment.SpecialFolder.Startup), "启动文件夹");
            ScanScheduledTasks(list);
            return list;
        }

        void ScanRegistry(List<StartupItem> list, RegistryKey root, string sub, string src)
        {
            RegistryKey k = null;
            try
            {
                k = root.OpenSubKey(sub);
                if (k == null) return;
                foreach (string name in k.GetValueNames())
                {
                    list.Add(new StartupItem
                    {
                        Name = name,
                        Path = k.GetValue(name).ToString(),
                        Source = src,
                        SafeToDisable = IsSafe(name)
                    });
                }
            }
            catch { }
            finally { if (k != null) k.Close(); }
        }

        void ScanFolder(List<StartupItem> list, string dir, string src)
        {
            if (!System.IO.Directory.Exists(dir)) return;
            foreach (string f in System.IO.Directory.GetFiles(dir))
            {
                string name = System.IO.Path.GetFileName(f);
                if (name.StartsWith(".")) continue;
                if (name == "desktop.ini" || name == "thumbs.db") continue;
                list.Add(new StartupItem
                {
                    Name = name,
                    Path = f,
                    Source = src,
                    SafeToDisable = IsSafe(name)
                });
            }
        }

        void ScanScheduledTasks(List<StartupItem> list)
        {
            var psi = new System.Diagnostics.ProcessStartInfo();
            psi.FileName = "schtasks";
            psi.Arguments = "/query /fo csv /nh";
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            var p = System.Diagnostics.Process.Start(psi);
            string line;
            while ((line = p.StandardOutput.ReadLine()) != null)
            {
                if (line.Contains("Microsoft")) continue;
                var parts = line.Split(',');
                if (parts.Length < 2) continue;
                string name = parts[0].Trim('"');
                string taskToRun = parts[1].Trim('"');
                list.Add(new StartupItem
                {
                    Name = name,
                    Path = taskToRun,
                    Source = "计划任务",
                    SafeToDisable = true
                });
            }
            p.WaitForExit();
        }

        bool IsSafe(string name)
        {
            string n = name.ToLower();
            if (n.Contains("microsoft") || n.Contains("windows") || n.Contains("driver"))
                return false;
            if (n.Contains("security") || n.Contains("health"))
                return false;
            return true;
        }
    }
}
