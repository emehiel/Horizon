using System;
using System.Collections.Generic;
using Utilities;
using HSFUniverse;
using HSFSystem;
using MissionElements;
using System.Xml;

namespace HSFSubsystem
{
    public abstract class Subsystem : ISubsystem{
        public bool IsChecked { get; set; }
        public Asset Asset { get; set; }
        public List<ISubsystem> DepenedentSubsystems { get; protected set; } 
        public string Name { get; protected set; }
        public Dictionary<string, Delegate> SubsystemDependencyFunctions { get; protected set; }
        public List<StateVarKey<int>> Ikeys { get; protected set; }
        public List<StateVarKey<double>> Dkeys { get; protected set; }
        public List<StateVarKey<float>> Fkeys { get; protected set; }
        public List<StateVarKey<bool>> Bkeys { get; protected set; }
        public List<StateVarKey<Matrix<double>>> Mkeys { get; protected set; }
        public List<StateVarKey<Quat>> Qkeys { get; protected set; }
        
        public Subsystem()
        {

        }
        public Subsystem(string name) {
            Name = name;
        }
        public Subsystem(XmlNode xmlNode)
        {

        }
        public virtual Subsystem clone() {
            return DeepCopy.Copy<Subsystem>(this);
        }
        public virtual bool canPerform(SystemState oldState, SystemState newState,
                            Task task, DynamicState position,
                            Universe environment) //Dependencies dep); doesn't need dependecies anymoere
        {
            foreach (var sub in DepenedentSubsystems)
            {
                if (sub.canPerform(oldState, newState, task, position, environment) == false)
                    return false;
                //use new state to update state. pass updated state to dependency collector
            }
            return true;
        }

        public abstract bool canExtend(SystemState newState, DynamicState position, Universe environment, double evalToTime); // Dependencies dep);

        //public virtual static HSFProfile<T> DependencyCollector() //in order to return profile needs to be a templated class
        //{
        //    throw new NotImplementedException();
        //}

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
    }
}//HSFSubsystem
//}//HSFSystem