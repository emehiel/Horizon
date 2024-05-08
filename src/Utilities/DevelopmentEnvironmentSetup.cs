using System;
using System.IO;
using System.Diagnostics;
using System.Security.Policy;


namespace Utilities
{    public static class DevEnvironment
    {
        // Relative pathing set up:
        public static string ExecutablePath { get; } = Process.GetCurrentProcess().MainModule.FileName;
        //public static string ExecutablePath { get; } = Environment.ProcessPath; // {get; set;}
        public static string ExecutableDirectory { get; } = Path.GetDirectoryName(ExecutablePath); // Might not be executable directory{get; set;}

        // Currently, the main repository directory is 5 directories up from where the executable lives within the directory
        // (during Debug and Run mode). Released version(s) of Horizon will likely need to rework this pathing setup. This
        // currently works for development on MacOS and Windows on the newly migrated .NET8 Horizon. 
        public static string RepoDirectory { get; } = Path.GetFullPath(Path.Combine(ExecutableDirectory, @"../../../../"));

    }
}
