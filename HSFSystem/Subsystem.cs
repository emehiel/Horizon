// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using Utilities;
using HSFUniverse;
using HSFSystem;
using MissionElements;
using System.Xml;


namespace HSFSubsystem
{
    [Serializable]
    public abstract class Subsystem : ISubsystem {
        #region Attributes
        public virtual bool IsEvaluated { get; set; }
        public Asset Asset { get; set; }
        public virtual List<Subsystem> DependentSubsystems { get; set; }
        public string Name { get; protected set; }
        //public static string DefaultSubName { get; protected set; }
        public virtual Dictionary<string, Delegate> SubsystemDependencyFunctions { get; set; }
        public List<StateVariableKey<int>> Ikeys { get; private set; } = new List<StateVariableKey<int>>();
        public List<StateVariableKey<double>> Dkeys { get; protected set; } = new List<StateVariableKey<double>>();
        public List<StateVariableKey<bool>> Bkeys { get; protected set; } = new List<StateVariableKey<bool>>();
        public List<StateVariableKey<Matrix<double>>> Mkeys { get; protected set; } = new List<StateVariableKey<Matrix<double>>>();
        public List<StateVariableKey<Quaternion>> Qkeys { get; protected set; } = new List<StateVariableKey<Quaternion>>();
        public List<StateVariableKey<Vector>> Vkeys { get; protected set; } = new List<StateVariableKey<Vector>>();
        
        // _newState and _task are here so the ScriptedSubsystem _pythonInstance can access them
        public virtual SystemState _newState { get; set; }
        public virtual Task _task { get; set; }
        #endregion Attributes

        #region Constructors

        public Subsystem()
        {

        }
        //public Subsystem(string name) {
        //    Name = name;
        //}
        public Subsystem(XmlNode xmlNode, Asset asset)
        {
            
        }
        public Subsystem(XmlNode xmlNode, Dependency deps, Asset asset)
        {
            
        }
        
        #endregion

        #region Methods
        //public virtual Subsystem Clone() {
        //    return DeepCopy.Copy<Subsystem>(this);
        //}

        /// <summary>
        /// The default canPerform method. 
        /// Should be used to check if all dependent subsystems can perform and extended by subsystem implementations.
        /// </summary>
        /// <param name="oldState"></param>
        /// <param name="newState"></param>
        /// <param name="tasks"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        public virtual bool CanPerform(Event proposedEvent, Domain environment)
        {
            //foreach (var sub in DependentSubsystems)
            //{
            //    if (!sub.IsEvaluated)// && !sub.GetType().Equals(typeof(ScriptedSubsystem)))
            //        if (sub.CanPerform(proposedEvent, environment) == false)
            //            return false;
            //}
            //_task = proposedEvent.GetAssetTask(Asset); //Find the correct task for the subsystem
            //_newState = proposedEvent.State;
            //IsEvaluated = true;
            return true;
        }

        /// <summary>
        /// This method tracks four things:
        /// 1.  Ensure all dependents Subsystems are evaluated before the current Subsystem is evaluates and set the
        ///     IsEvaluated status.
        /// 2.  Calls the CanPerform() method for the subsystem when all dependent subsystems have been evlauted
        /// 3.  Calls the CanPerform() method when a subsystem has no dependent subsystems
        /// 4.  If a Subsystem CanPerform() method returns false, colapse the nested call to false
        /// </summary>
        /// <param name="proposedEvent"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        public bool CheckDependentSubsystems(Event proposedEvent, Domain environment)
        {
            if (DependentSubsystems.Count == 0)
            {
                IsEvaluated = true;
                _task = proposedEvent.GetAssetTask(Asset); //Find the correct task for the subsystem
                _newState = proposedEvent.State;
                return this.CanPerform(proposedEvent, environment);
            }
            else
            {
                foreach (var sub in DependentSubsystems)
                {
                    if (!sub.IsEvaluated)// && !sub.GetType().Equals(typeof(ScriptedSubsystem)))
                    {
                        if (sub.CheckDependentSubsystems(proposedEvent, environment))
                        {
                            IsEvaluated = true;
                            _task = proposedEvent.GetAssetTask(Asset); //Find the correct task for the subsystem
                            _newState = proposedEvent.State;
                            if (!CanPerform(proposedEvent, environment))
                                return false;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return true;
            }

        }
        /// <summary>
        /// The default canExtend function. May be over written for additional functionality.
        /// </summary>
        /// <param name="newState"></param>
        /// <param name="position"></param>
        /// <param name="environment"></param>
        /// <param name="evalToTime"></param>
        /// <returns></returns>
        public virtual bool CanExtend(Event proposedEvent, Domain environment, double evalToTime)
        {
            if (proposedEvent.GetEventEnd(Asset) < evalToTime)
                proposedEvent.SetEventEnd(Asset, evalToTime);
            return true;
        }

        /// <summary>
        /// Add the dependency collector to the dependency dictionary
        /// </summary>
        /// <param name="Deps"></param>
        /// <param name="FuncNames"></param>
        public void AddDependencyCollector()
        {
            SubsystemDependencyFunctions.Add("DepCollector", new Func<Event, HSFProfile<double>>(DependencyCollector));
        }

        /// <summary>
        /// Default Dependency Collector simply sums the results of the dependecy functions
        /// </summary>
        /// <param name="currentState"></param>
        /// <returns></returns>
        public virtual HSFProfile<double> DependencyCollector(Event currentEvent)
        {
            if (SubsystemDependencyFunctions.Count == 0)
                throw new MissingMemberException("You may not call the dependency collector in your can perform because you have not specified any dependency functions for " + Name);
            HSFProfile<double> outProf = new HSFProfile<double>();
            foreach (var dep in SubsystemDependencyFunctions)
            {
                if (!dep.Key.Equals("DepCollector"))
                {
                    HSFProfile<double> temp = (HSFProfile<double>)dep.Value.DynamicInvoke(currentEvent);
                    outProf = outProf + temp;
                }
            }
            return outProf;
        }

        /// <summary>
        /// Find the subsystem name field from the XMLnode and create the name of format "Asset#.SubName
        /// </summary>
        /// <param name="subXmlNode"></param>
        public void GetSubNameFromXmlNode(XmlNode subXmlNode)
        {
            string assetName = Asset.Name;
            if (subXmlNode.Attributes["subsystemName"] != null)
                Name = assetName + "." + subXmlNode.Attributes["subsystemName"].Value.ToString().ToLower();
            //else if (DefaultSubName != null)
            //    Name = assetName + "." + DefaultSubName.ToLower() ;
            //else if (subXmlNode.Attributes["type"] != null)
            //    Name = assetName + "." + subXmlNode.Attributes["type"].Value.ToString().ToLower();
            else
                throw new ArgumentException($"Missing a subsystem name attribute for subsystem in {assetName}!");
        }

        /// <summary>
        /// Method to get subsystem name from xml node.
        /// </summary>
        /// <param name="subXmlNode"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static string parseNameFromXmlNode(XmlNode subXmlNode, string assetName)
        {
            string Name;
            if (subXmlNode.Attributes["subsystemName"] != null)
                Name = assetName + "." + subXmlNode.Attributes["subsystemName"].Value.ToString().ToLower();
            //else if (DefaultSubName != null)
            //    Name = assetName + "." + DefaultSubName.ToLower() ;
            //else if (subXmlNode.Attributes["type"] != null)
            //    Name = assetName + "." + subXmlNode.Attributes["type"].Value.ToString().ToLower();
            else
                throw new ArgumentException($"Missing a subsystem name attribute for subsystem in {assetName}!");
            return Name;
        }

        //public void getInitialStateFromXmlNode(XmlNode ICXmlNode)
        //{
        //    Type keyType = Type.GetType(ICXmlNode.Attributes["type"].Value.ToString());
        //    string key = ICXmlNode.Attributes["key"].Value.ToString();
        //    string value = ICXmlNode.Attributes["value"].ToString();
        //    if(keyType)
        //    StateVarKey <keyType.GetType()> = new StateVarKey<keyType.GetType() > (key);
        //    .ChangeType(value, keyType);
        //}

        /// <summary>
        /// Method to get subsystem state at a given time. Should be used for writing out state data
        /// </summary>
        /// <param name="currentSystemState"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public SystemState getSubStateAtTime(SystemState currentSystemState, double time)
        {
            SystemState state = new SystemState();
            foreach(var key in Ikeys)
            {
                state.Idata.Add(key, new HSFProfile<int>(time, currentSystemState.GetValueAtTime(key, time).Value));
            }
            foreach (var key in Bkeys)
            {
                state.Bdata.Add(key, new HSFProfile<bool>(time, currentSystemState.GetValueAtTime(key, time).Value));
            }
            foreach (var key in Dkeys)
            {
                state.Ddata.Add(key, new HSFProfile<double>(time, currentSystemState.GetValueAtTime(key, time).Value));
            }
            foreach (var key in Mkeys)
            {
                state.Mdata.Add(key, new HSFProfile<Matrix<double>>(time, currentSystemState.GetValueAtTime(key, time).Value));
            }
            return state;
        }
        
        // Add keys depending on the type of the key
        public void addKey(StateVarKey<int> keyIn) {
            if (Ikeys == null) //Only construct what you need
            {
                Ikeys = new List<StateVarKey<int>>();
            }
            Ikeys.Add(keyIn);
        }

        public void addKey(StateVarKey<double> keyIn) {
            if (Dkeys == null) //Only construct what you need
            {
                Dkeys = new List<StateVarKey<double>>();
            }
            Dkeys.Add(keyIn);
        }

        public void addKey(StateVarKey<float> keyIn) {
            if (Fkeys == null) //Only construct what you need
            {
                Fkeys = new List<StateVarKey<float>>();
            }
            Fkeys.Add(keyIn);
        }

        public void addKey(StateVarKey<bool> keyIn) {
            if (Bkeys == null) //Only construct what you need
            {
                Bkeys = new List<StateVarKey<bool>>();
            }
            Bkeys.Add(keyIn);
        }

        public void addKey(StateVarKey<Matrix<double>> keyIn) {
            if (Mkeys == null) //Only construct what you need
            {
                Mkeys = new List<StateVarKey<Matrix<double>>>();
            }
            Mkeys.Add(keyIn);
        }

        public void addKey(StateVarKey<Quaternion> keyIn) {
            if (Qkeys == null) //Only construct what you need
            {
                Qkeys = new List<StateVarKey<Quaternion>>();
            }
            Qkeys.Add(keyIn);
        }
        #endregion
    }
}//HSFSubsystem