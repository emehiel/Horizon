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
using System.IO;
using System.Reflection;

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

        //public override bool IsEvaluated
        //{
        //    get { return (bool)_pythonInstance.IsEvaluated; }
        //    set { _pythonInstance.IsEvaluated = (bool)value; }
        //}

        //public override SystemState _newState {
        //    get { return (SystemState)_pythonInstance._newState; }
        //    set { _pythonInstance._newState = value; }
        //}

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
        /// <param name="asset"></param>
        public ScriptedSubsystem(XmlNode scriptedSubXmlNode, Asset asset)
        {
            Asset = asset;
            GetSubNameFromXmlNode(scriptedSubXmlNode);

            string pythonFilePath ="", className = "";
            XmlParser.ParseScriptedSrc(scriptedSubXmlNode, ref pythonFilePath, ref className);

            //  I believe this was added by Jack B. for unit testing.  Still need to sort out IO issues, but with this commented out
            //  the execuitable will look for python files in the same directory as the .exe file is located.
            //  Need to do better specifying the input and output paths.
            //if (!pythonFilePath.StartsWith("..\\")) //patch work for nunit testing which struggles with relative paths
            //{
            //    string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
            //    pythonFilePath = Path.Combine(baselocation, @pythonFilePath);
            //}
            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();
            var ops = engine.Operations;
            var p = engine.GetSearchPaths();
            p.Add(AppDomain.CurrentDomain.BaseDirectory + "\\..\\..\\..\\PythonScripting");
            p.Add(AppDomain.CurrentDomain.BaseDirectory + "\\..\\..\\..\\");
            engine.SetSearchPaths(p);
            engine.ExecuteFile(pythonFilePath, scope);
            var pythonType = scope.GetVariable(className);
            _pythonInstance = ops.CreateInstance(pythonType, scriptedSubXmlNode, asset);
            Delegate depCollector = _pythonInstance.GetDependencyCollector();
            SubsystemDependencyFunctions = new Dictionary<string, Delegate>();
            SubsystemDependencyFunctions.Add("DepCollector", depCollector);
            DependentSubsystems = new List<Subsystem>();
        }
        #endregion

        #region Methods
        public override bool CanPerform(Event proposedEvent, Domain environment)
        {
            //if (IsEvaluated)
            //    return true;

            //// Check all dependent subsystems
            //foreach (var sub in DependentSubsystems)
            //{
            //    if (!sub.IsEvaluated)
            //        if (sub.CanPerform(proposedEvent, environment) == false)
            //            return false;
            //}

            //_task = proposedEvent.GetAssetTask(Asset); //Find the correct task for the subsystem
            //_newState = proposedEvent.State;
            //IsEvaluated = true;

            // Call the can perform method that is in the python class
            dynamic perform = _pythonInstance.CanPerform(proposedEvent, environment);
            return (bool)perform;
        }

        public override bool CanExtend(Event proposedEvent, Domain environment, double evalToTime)
        {
            dynamic extend = _pythonInstance.CanExtend(proposedEvent, environment, evalToTime);
            return (bool)extend;
        }

        public Delegate GetDepFn(string depFnName, ScriptedSubsystem depSub)
        {
            // Access the python instance, call DepFinder from python model, return the Delegate fn requested
            var pythonInstance = depSub._pythonInstance;
            Dictionary<String, Delegate> theBook = pythonInstance.DepFinder(depFnName);
            Delegate page = theBook[depFnName];
            return page;
        }
        #endregion
    }
}
