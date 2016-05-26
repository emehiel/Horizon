using System;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting;
using System.Dynamic;
namespace HSFSubsystem
{
    /// <summary> 
    /// Logger maintains the log data from each failed schedule, writes to csv for post processing and modifies the 
    /// subsystem checking order and constraint checking order to make sure failed subsystems and constraints are evaluated first
    /// </summary>
    public class Logger
    {
        public static void Report(string v)
        {
            Console.WriteLine("Failed Subsystem " + v);
        }
    }
}