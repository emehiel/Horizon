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
        private ScriptScope _pyScope;
        protected Event _event;
        protected Universe _universe;
     //   public Type CollectorType { get; private set; }
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
            PythonInstance = ops.CreateInstance(pythonType);
            Dictionary<string, Delegate> newDependencies = PythonInstance.getDependencyDictionary();
            if(newDependencies.Count != 0)
                dependencies.Append(newDependencies);
            SubsystemDependencyFunctions = PythonInstance.getDependencyDictionary();
        }
        public ScriptedSubsystem(XmlNode scriptedSubXmlNode, Dependency dependencies, Asset asset)
        {
            Asset = asset;
            GetSubNameFromXmlNode(scriptedSubXmlNode);
            DependentSubsystems = new List<Subsystem>();
            string pythonFilePath, className;
            if (scriptedSubXmlNode.Attributes["src"] == null)
                throw new MissingFieldException("No source file location found in XmlNode " + Name);
            pythonFilePath = scriptedSubXmlNode.Attributes["src"].Value.ToString();
            //if(scriptedSubXmlNode.Attributes["collectorType"] == null)
            //    CollectorType = Type.GetType(scriptedSubXmlNode.Attributes["CollectorType"].Value.ToString());
            if(scriptedSubXmlNode.Attributes["className"] == null)
                throw new MissingFieldException("No class name found in XmlNode " + Name);
            className = scriptedSubXmlNode.Attributes["className"].Value.ToString();

            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();
            var ops = engine.Operations;
            engine.ExecuteFile(pythonFilePath, scope);
            var pythonType = scope.GetVariable(className);
            var inputs = new object[] { scriptedSubXmlNode, dependencies, asset };
            PythonInstance = ops.CreateInstance(pythonType, scriptedSubXmlNode, asset);
            Dictionary<string, Delegate> newDependencies = PythonInstance.GetDependencyDictionary();
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
            dynamic perform = PythonInstance.canPerform(proposedEvent, environment );
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
            dynamic extend = PythonInstance.CanExtend(proposedEvent, environment, evalToTime);
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
