using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickPen.Actions
{
    public  class LetAppRunAtStartup
    {
        readonly string? AppDirectory = Environment.ProcessPath;
        readonly string RegistryKeyPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        readonly string AppName = "QuickPen";

        public void RegisterApp()
        {
            RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey
                (RegistryKeyPath, true);

            if (registryKey == null || AppDirectory == null) throw new ArgumentException("can't be null!");

            registryKey.SetValue(AppName, AppDirectory);
        }
    }
}
