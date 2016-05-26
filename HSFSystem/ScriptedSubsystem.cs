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
using UserModel;
using HSFUniverse;
using System.Xml;

namespace HSFSubsystem
{
    public class ScriptedSubsystem : Subsystem
    {
        #region Attributes
        private dynamic _pythonInstance;
        private ScriptScope _pyScope;
        #endregion

        #region Constructors
        public ScriptedSubsystem(string filePath, string className, Dependency dependencies, Type collectorType)
        {
            Name = className;
      //      CollectorType = collectorType;
            var engine = Python.CreateEngine();
            _pyScope = engine.CreateScope();
            var ops = engine.Operations;
            engine.ExecuteFile(filePath, _pyScope);
            var pythonType = _pyScope.GetVariable(className);
            _pythonInstance = ops.CreateInstance(pythonType);
            Dictionary<string, Delegate> newDependencies = _pythonInstance.getDependencyDictionary();
            if(newDependencies.Count != 0)
                dependencies.Append(newDependencies);
            SubsystemDependencyFunctions = _pythonInstance.getDependencyDictionary();
        }
        public ScriptedSubsystem(XmlNode scriptedSubXmlNode, Dependency dependencies, Asset asset)
        {
            Asset = asset;
            GetSubNameFromXmlNode(scriptedSubXmlNode);
            DependentSubsystems = new List<Subsystem>();
            string pythonFilePath ="", className = "";
            XmlParser.ParseScriptedSrc(scriptedSubXmlNode, ref pythonFilePath, ref className);

            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();
            var ops = engine.Operations;
            engine.ExecuteFile(pythonFilePath, scope);
            var pythonType = scope.GetVariable(className);
            _pythonInstance = ops.CreateInstance(pythonType, scriptedSubXmlNode, asset);
            Dictionary<string, Delegate> newDependencies = _pythonInstance.GetDependencyDictionary();
            dependencies.Append(newDependencies);
        }
        #endregion

        #region Methods
        public override bool CanPerform(Event proposedEvent,  Universe environment)
        {
            foreach (var sub in DependentSubsystems)
            {
                if (!sub.IsEvaluated)
                    if (sub.CanPerform(proposedEvent, environment) == false)
                        return false;
            }
            _task = proposedEvent.GetAssetTask(Asset); //Find the correct task for the subsystem
            _newState = proposedEvent.State;
            _oldState = proposedEvent.State.Previous;
            //_pyScope.SetVariable("self._event", proposedEvent);
            //_pyScope.SetVariable("self._universe", environment);
            dynamic perform = _pythonInstance.canPerform(proposedEvent, environment );
            //This needs to be inside the python canPerform
            //IsEvaluated = true;
            //if ((bool)perform == false)
            //    return false;
            //var newProf = Convert.ChangeType(CollectorType, DependencyCollector());
            //newState.addValue(KEY, );
            return (bool)perform;
        }
        public override bool CanExtend(Event proposedEvent, Universe environment, double evalToTime)
        {
            dynamic extend = _pythonInstance.CanExtend(proposedEvent, environment, evalToTime);
            return (bool)extend;
        }

        //don't need this if it happens in python script
        //public virtual dynamic DependencyCollector(SystemState currentState)
        //{
        //    return PythonInstance.DependencyCollector(currentState);
        //}
        #endregion
    }
}
