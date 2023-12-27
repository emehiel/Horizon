using IronPython.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSFSystem
{
    public class ScriptedSubsystemHelper
    {
        public dynamic PythonInstance { get; set; }
        public ScriptedSubsystemHelper()
        {
            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();
            var ops = engine.Operations;
            // Search paths are for importing modules from python scripts, not for executing python subsystem files
            var p = engine.GetSearchPaths();
            p.Add(AppDomain.CurrentDomain.BaseDirectory + @"..\..\..\PythonSubs");
            p.Add(AppDomain.CurrentDomain.BaseDirectory + @"..\..\..\");
            p.Add(AppDomain.CurrentDomain.BaseDirectory + @"..\..\..\tools");

            engine.SetSearchPaths(p);
            engine.ExecuteFile(@"..\..\..\..\tools\HSF_Helper.py", scope);
            var pythonType = scope.GetVariable("HSFHelper");
            PythonInstance = ops.CreateInstance(pythonType);
        }
    }
}
