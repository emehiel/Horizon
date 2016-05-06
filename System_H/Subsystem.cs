using System;
using System.Collections.Generic;
using Utilities;
using HSFUniverse;
using HSFSystem;
using MissionElements;
using System.Xml;

namespace HSFSubsystem
{
    public abstract class Subsystem : ISubsystem {
        #region Attributes
        public bool IsEvaluated { get; set; }
        public Asset Asset { get; set; }
        public List<ISubsystem> DepenedentSubsystems { get; protected set; }
        public string Name { get; protected set; }
        public static string DefaultSubName { get; protected set; }
        public Dictionary<string, Delegate> SubsystemDependencyFunctions { get; protected set; }
        public List<StateVarKey<int>> Ikeys { get; protected set; }
        public List<StateVarKey<double>> Dkeys { get; protected set; }
        public List<StateVarKey<float>> Fkeys { get; protected set; }
        public List<StateVarKey<bool>> Bkeys { get; protected set; }
        public List<StateVarKey<Matrix<double>>> Mkeys { get; protected set; }
        public List<StateVarKey<Quat>> Qkeys { get; protected set; }
        private SystemState _oldState;
        private SystemState _newState;
        private Task _task;
        #endregion Attributes

        #region Constructors
        public Subsystem()
        {

        }
        public Subsystem(string name) {
            Name = name;
        }
        public Subsystem(XmlNode xmlNode, Asset asset)
        {

        }
        public Subsystem(XmlNode xmlNode, Dependencies deps, Asset asset)
        {

        }
        #endregion

        #region Methods
        public virtual Subsystem clone() {
            return DeepCopy.Copy<Subsystem>(this);
        }
        /// <summary>
        /// The default canPerform method. 
        /// Should be used to check if all dependent subsystems can perform and extended by subsystem implementations.
        /// </summary>
        /// <param name="oldState">The previous state of the syste</param>
        /// <param name="newState">The new state of the system determined by the Subsystem</param>
        /// <param name="task"></param>
        /// <param name="position"></param>
        /// <param name="environment"></param>
        /// <param name="allStates"></param>
        /// <returns></returns>
        public virtual bool canPerform(List<SystemState> oldStates, List<SystemState> newStates,
                            Task task, DynamicState position,
                            Universe environment)
        {
            foreach (var sub in DepenedentSubsystems)
            {
                if (sub.canPerform(oldStates, newStates, task, position, environment) == false)
                    return false;
            }
            _oldState = newState.previous.getLastValue()
            return true;
        }
        /// <summary>
        /// The default canExtend function. May be over written for additional functionality.
        /// </summary>
        /// <param name="newState"></param>
        /// <param name="position"></param>
        /// <param name="environment"></param>
        /// <param name="evalToTime"></param>
        /// <returns></returns>
        public virtual bool canExtend(List<SystemState> newStates, DynamicState position, Universe environment, double evalToTime)
        {
            SystemState newState = newStates.Find(item => item.Asset == asset);
            if (newState.EventEnd < evalToTime)
                newState.EventEnd = evalToTime;
            return true;
        }

        //make a logger method
        public void CollectDependencyFuncs(Dependencies Deps, List<string> FuncNames)
        {
            foreach (var Func in FuncNames)
            {
                SubsystemDependencyFunctions.Add(Func, Deps.getDependencyFunc(Func));
            }
        }

        public void CollectDependenctSubsystems(List<ISubsystem> deps)
        {
            DepenedentSubsystems = deps;
        }

        public void getSubNameFromXmlNode(XmlNode subXmlNode)
        {
            if (subXmlNode.Attributes["subsystemName"] != null)
                Name = subXmlNode.Attributes["subsystemName"].Value.ToString();
            else if (DefaultSubName != null)
                Name = DefaultSubName;
            else if (subXmlNode.Attributes["type"] != null)
                Name = subXmlNode.Attributes["type"].Value.ToString();
            else
                throw new MissingMemberException("Missing a subsystemName or type field for subsystem!");
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
        public void addKey(StateVarKey<Quat> keyIn) {
            if (Qkeys == null) //Only construct what you need
            {
                Qkeys = new List<StateVarKey<Quat>>();
            }
            Qkeys.Add(keyIn);
        }

        #endregion
    }
}//HSFSubsystem