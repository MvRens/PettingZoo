using System;
using System.IO;
using System.Reflection;

namespace PettingZoo.Core.Settings
{
    public static class PettingZooPaths
    {
        public static string AppDataRoot { get; }
        public static string InstallationRoot { get; }


        static PettingZooPaths()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (appDataPath == null)
                throw new IOException("Could not resolve application data path");

            AppDataRoot = Path.Combine(appDataPath, @"PettingZoo");
            if (!Directory.CreateDirectory(AppDataRoot).Exists)
                throw new IOException($"Failed to create directory: {AppDataRoot}");

            InstallationRoot = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location ?? Assembly.GetExecutingAssembly().Location)!;
        }
    }
}
