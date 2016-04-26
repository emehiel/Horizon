using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting;
using System.Dynamic;
using HSFSystem;
using MissionElements;
using HSFUniverse;

namespace HSFSubsystem
{
    public class ScriptedSubsystem : ISubsystem
    {
        public bool IsEvaluated { get; set; }
        public dynamic PythonInstance { get; private set; }
        public string Name { get; private set; }
        public Dictionary<string, Delegate> SubsystemDependencyFunctions;
        public ScriptedSubsystem(string filePath, string className, Dependencies dependencies, int initState)
        {
            Name = className;
            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();
            var ops = engine.Operations;
            engine.ExecuteFile(filePath, scope);
            var pythonType = scope.GetVariable(className);
            PythonInstance = ops.CreateInstance(pythonType);
            Dictionary<string, Delegate> newDependencies = PythonInstance.getDependencyDictionary();
            dependencies.Append(newDependencies);
            SubsystemDependencyFunctions = PythonInstance.getDependencyDictionary();
        }
        public bool canPerform(SystemState oldState, ref SystemState newState,
                          Task task, DynamicState position,
                          Universe environment)
        {
            throw new NotImplementedException();
        }
        public bool canExtend(SystemState newState, DynamicState position, Universe environment, double evalToTime)
        {
            throw new NotImplementedException();
        }

        public virtual dynamic DependencyCollector()
        {
            return PythonInstance.DependencyCollector();
        }

        public void CollectDependencyFuncs(Dependencies Deps, List<string> FuncNames)
        {
            foreach (var Func in FuncNames)
            {
                SubsystemDependencyFunctions.Add(Func, Deps.getDependencyFunc(Func));
            }
        }

    }
}
