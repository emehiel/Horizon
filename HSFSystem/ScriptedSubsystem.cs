// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

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
using Utilities;

namespace HSFSubsystem
{
    public class ScriptedSubsystem : Subsystem
    {
        #region Attributes
        protected dynamic _pythonInstance;
        public override List<Subsystem> DependentSubsystems
        {
            get { return ( List<Subsystem>)_pythonInstance.DependentSubsystems; }
            set { _pythonInstance.DependentSubsystems = (List<Subsystem>)value; }
        }
        public override Dictionary<string, Delegate> SubsystemDependencyFunctions
        {
            get { return (Dictionary<string, Delegate>)_pythonInstance.SubsystemDependencyFunctions; }
            set { _pythonInstance.SubsystemDependencyFunctions = (Dictionary<string, Delegate>)value; }
        }
        public override bool IsEvaluated
        {
            get { return (bool)_pythonInstance.IsEvaluated; }
            set { _pythonInstance.IsEvaluated = (bool)value; }
        }

        public override SystemState _newState {
            get { return (SystemState)_pythonInstance._newState; }
            set { _pythonInstance._newState = value; }
        }
        public override Task _task {
            get { return (Task)_pythonInstance._task; }
            set { _pythonInstance._task = value; }
        }
        #endregion

        #region Constructors
        public ScriptedSubsystem(XmlNode scriptedSubXmlNode, Dependency dependencies, Asset asset)
        {
            Asset = asset;
            GetSubNameFromXmlNode(scriptedSubXmlNode);
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
            DependentSubsystems = new List<Subsystem>();
            SubsystemDependencyFunctions = new Dictionary<string, Delegate>();
            Delegate depCollector = _pythonInstance.GetDependencyCollector();
            SubsystemDependencyFunctions.Add("DepCollector", depCollector);
        }
        #endregion

        #region Methods
        public override bool CanPerform(Event proposedEvent,  Universe environment)
        {
            if (IsEvaluated)
                return true;
            foreach (var sub in DependentSubsystems)
            {
                if (!sub.IsEvaluated)
                    if (sub.CanPerform(proposedEvent, environment) == false)
                        return false;
            }
            _task = proposedEvent.GetAssetTask(Asset); //Find the correct task for the subsystem
            _newState = proposedEvent.State;
            IsEvaluated = true;
            //_pyScope.SetVariable("self._event", proposedEvent);
            //_pyScope.SetVariable("self._universe", environment);
            dynamic perform = _pythonInstance.CanPerform(proposedEvent, environment);
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

        //protected override HSFProfile<double> DependencyCollector(Event currentEvent)
        //{
        //    if (SubsystemDependencyFunctions.Count == 0)
        //        throw new MissingMemberException("You may not call the dependency collector in your can perform because you have not specified any dependency functions for " + Name);
        //    HSFProfile<double> outProf = new HSFProfile<double>();
        //    foreach (var dep in SubsystemDependencyFunctions)
        //    {
        //        HSFProfile<double> temp = (HSFProfile<double>)dep.Value.DynamicInvoke(currentEvent);
        //        outProf = outProf + temp;
        //    }
        //    return outProf;
        //}

        //don't need this if it happens in python script
        //public virtual dynamic DependencyCollector(SystemState currentState)
        //{
        //    return PythonInstance.DependencyCollector(currentState);
        //}
        #endregion
    }
}
