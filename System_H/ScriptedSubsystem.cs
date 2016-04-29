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
using System.Xml;

namespace HSFSubsystem
{
    public class ScriptedSubsystem : Subsystem
    {
        #region Attributes
        public dynamic PythonInstance { get; private set; }
        public Type CollectorType { get; private set; }
        #endregion

        #region Constructors
        public ScriptedSubsystem(string filePath, string className, Dependencies dependencies, Type collectorType)
        {
            Name = className;
            CollectorType = collectorType;
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
        public ScriptedSubsystem(XmlNode scriptedSubXmlNode, Dependencies dependencies)
        {
            Name = scriptedSubXmlNode.Attributes["Name"].Value.ToString();
            // Subsytem is a ScriptedSubsytem, get the LUA functions for the Subsytem
            string pythonFilePath = scriptedSubXmlNode.Attributes["src"].Value.ToString();
            CollectorType = Type.GetType(scriptedSubXmlNode.Attributes["CollectorType"].Value.ToString());
            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();
            var ops = engine.Operations;
            engine.ExecuteFile(pythonFilePath, scope);
            var pythonType = scope.GetVariable(Name);
            PythonInstance = ops.CreateInstance(pythonType);
            Dictionary<string, Delegate> newDependencies = PythonInstance.getDependencyDictionary();
            dependencies.Append(newDependencies);
            SubsystemDependencyFunctions = PythonInstance.getDependencyDictionary();
        }
        #endregion

        #region Methods
        public override bool canPerform(SystemState oldState, SystemState newState,
                          Task task, DynamicState position,
                          Universe environment)
        {
            if (!base.canPerform(oldState, newState, task, position, environment)) //checks all the dependent subsystems
                return false;

            dynamic perform = PythonInstance.canPerform(oldState, newState, task, position, environment);
            //This needs to be inside the python canPerform
            //IsEvaluated = true;
            //if ((bool)perform == false)
            //    return false;
            //var newProf = Convert.ChangeType(CollectorType, DependencyCollector());
            //newState.addValue(KEY, );
            return true;
        }
        public override bool canExtend(SystemState newState, DynamicState position, Universe environment, double evalToTime)
        {
            throw new NotImplementedException();
        }

        public virtual dynamic DependencyCollector()
        {
            return PythonInstance.DependencyCollector();
        }
        #endregion
    }
}
