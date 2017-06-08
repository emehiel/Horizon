// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using IronPython.Hosting;
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
        // A reference to the python scripted class
        protected dynamic _pythonInstance;

        // Overide the accessors in order to modify the python instance
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
        /// <summary>
        /// Constructor to initialize the python subsystem
        /// </summary>
        /// <param name="scriptedSubXmlNode"></param>
        /// <param name="dependencies"></param>
        /// <param name="asset"></param>
        public ScriptedSubsystem(XmlNode scriptedSubXmlNode, Dependency dependencies, Asset asset)
        {
            Asset = asset;
            GetSubNameFromXmlNode(scriptedSubXmlNode);
            string pythonFilePath ="", className = "";
            XmlParser.ParseScriptedSrc(scriptedSubXmlNode, ref pythonFilePath, ref className);
            
            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();
            var ops = engine.Operations;
            var p = engine.GetSearchPaths();
            p.Add(AppDomain.CurrentDomain.BaseDirectory + "\\..\\..\\..\\PythonScripting");
            engine.SetSearchPaths(p);
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

            // Check all dependent subsystems
            foreach (var sub in DependentSubsystems)
            {
                if (!sub.IsEvaluated)
                    if (sub.CanPerform(proposedEvent, environment) == false)
                        return false;
            }

            _task = proposedEvent.GetAssetTask(Asset); //Find the correct task for the subsystem
            _newState = proposedEvent.State;
            IsEvaluated = true;

            // Call the can perform method that is in the python class
            dynamic perform = _pythonInstance.CanPerform(proposedEvent, environment);
            return (bool)perform;
        }

        public override bool CanExtend(Event proposedEvent, Universe environment, double evalToTime)
        {
            dynamic extend = _pythonInstance.CanExtend(proposedEvent, environment, evalToTime);
            return (bool)extend;
        }
        #endregion
    }
}
