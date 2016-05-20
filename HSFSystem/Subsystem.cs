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
        public bool IsEvaluated { get; set; }
        public Asset Asset { get; set; }
        public List<Subsystem> DependentSubsystems { get; protected set; }
        public string Name { get; protected set; }
        public static string DefaultSubName { get; protected set; }
        public Dictionary<string, Delegate> SubsystemDependencyFunctions { get; protected set; }
        public List<StateVarKey<int>> Ikeys { get; protected set; }
        public List<StateVarKey<double>> Dkeys { get; protected set; }
        public List<StateVarKey<float>> Fkeys { get; protected set; }
        public List<StateVarKey<bool>> Bkeys { get; protected set; }
        public List<StateVarKey<Matrix<double>>> Mkeys { get; protected set; }
        public List<StateVarKey<Quat>> Qkeys { get; protected set; }
        protected SystemState _oldState;
        protected SystemState _newState;
        protected Task _task;
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
        public Subsystem(XmlNode xmlNode, Dependency deps, Asset asset)
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
        /// <param name="oldState"></param>
        /// <param name="newState"></param>
        /// <param name="tasks"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        public virtual bool canPerform(Event proposedEvent, Universe environment)
        {
            foreach (var sub in DependentSubsystems)
            {
                if(!sub.IsEvaluated)
                    if (sub.canPerform(proposedEvent, environment) == false)
                        return false;
            }
            _task = proposedEvent.GetAssetTask(Asset); //Find the correct task for the subsystem
            _newState = proposedEvent.State;
            _oldState = proposedEvent.State.Previous; //do we want to prevent user from modifying oldState on accident??
            IsEvaluated = true;
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
        public virtual bool canExtend(Event proposedEvent,  Universe environment, double evalToTime)
        {
            if (proposedEvent.GetEventEnd(Asset) < evalToTime)
                proposedEvent.SetEventEnd(Asset, evalToTime);
            return true;
        }

        //make a logger method
        /// <summary>
        /// Go to the dependency dictionary and grab all the dependency functions with FuncNames and add it to the 
        /// subsystem's SubsystemDependencyFunctions field
        /// </summary>
        /// <param name="Deps"></param>
        /// <param name="FuncNames"></param>
        public void CollectDependencyFuncs(Dependency Deps, List<string> FuncNames)
        {
            foreach (var Func in FuncNames)
            {
                SubsystemDependencyFunctions.Add(Func, Deps.getDependencyFunc(Func));
            }
        }
        /// <summary>
        /// Default Dependency Collector simply sums the results of the dependecy functions
        /// </summary>
        /// <param name="currentState"></param>
        /// <returns></returns>
        protected HSFProfile<double> DependencyCollector(Event currentEvent)
        {
            if (SubsystemDependencyFunctions.Count == 0)
                throw new MissingMemberException("You may not call the dependency collector in your can perform because you have not specified any dependency functions for " + Name);
            HSFProfile<double> outProf = new HSFProfile<double>();
            foreach (var dep in SubsystemDependencyFunctions)
            {
                HSFProfile<double> temp = (HSFProfile<double>)dep.Value.DynamicInvoke(currentEvent);
                outProf = outProf + temp;
            }
            return outProf;
        }
        /// <summary>
        /// Add all the dependent subsystems to the DependentSubsystems field
        /// </summary>
        /// <param name="deps"></param>
        //public void CollectDependentSubsystems(List<ISubsystem> deps)
        //{
        //    DepenedentSubsystems = deps;
        //}

        /// <summary>
        /// Find the subsystem name field from the XMLnode and create the name of format "Asset#.SubName
        /// </summary>
        /// <param name="subXmlNode"></param>
        public void getSubNameFromXmlNode(XmlNode subXmlNode)
        {
            string assetName = Asset.Name;
            if (subXmlNode.Attributes["subsystemName"] != null)
                Name = assetName + "." + subXmlNode.Attributes["subsystemName"].Value.ToString().ToLower();
            else if (DefaultSubName != null)
                Name = assetName + "." + DefaultSubName.ToLower() ;
            else if (subXmlNode.Attributes["type"] != null)
                Name = assetName + "." + subXmlNode.Attributes["type"].Value.ToString().ToLower();
            else
                throw new MissingMemberException("Missing a subsystemName or type field for subsystem!");
        }

        public static string parseNameFromXmlNode(XmlNode subXmlNode, string assetName)
        {
            string Name;
            if (subXmlNode.Attributes["subsystemName"] != null)
                Name = assetName + "." + subXmlNode.Attributes["subsystemName"].Value.ToString().ToLower();
            else if (DefaultSubName != null)
                Name = assetName + "." + DefaultSubName.ToLower() ;
            else if (subXmlNode.Attributes["type"] != null)
                Name = assetName + "." + subXmlNode.Attributes["type"].Value.ToString().ToLower();
            else
                throw new MissingMemberException("Missing a subsystemName or type field for subsystem!");
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