using System;
using System.Collections.Generic;
using Utilities;
using HSFUniverse;
using HSFSystem;
using MissionElements;
namespace HSFSubsystem
{
    public abstract class Subsystem : ISubsystem{
        public bool IsChecked { get; set; }
        public Asset Asset { get; set; }
        public List<ISubsystem> DepenedentSubsystems { get; protected set; } 
        public string Name { get; protected set; }
        public Dictionary<string, Delegate> SubsystemDependencyFunctions { get; private set; }
        public List<StateVarKey<int>> Ikeys { get; protected set; }
        public List<StateVarKey<double>> Dkeys { get; protected set; }
        public List<StateVarKey<float>> Fkeys { get; protected set; }
        public List<StateVarKey<bool>> Bkeys { get; protected set; }
        public List<StateVarKey<Matrix<double>>> Mkeys { get; protected set; }
        public List<StateVarKey<Quat>> Qkeys { get; protected set; }
        
        public Subsystem(string name) {
            Name = name;
        }
        public virtual Subsystem clone() {
            return DeepCopy.Copy<Subsystem>(this);
        }
        public virtual bool canPerform(SystemState oldState, ref SystemState newState,
                            Task task, DynamicState position,
                            Universe environment) //Dependencies dep); doesn't need dependecies anymoere
        {
            foreach (var sub in DepenedentSubsystems)
            {
                if (sub.canPerform(oldState, ref newState, task, position, environment) == false)
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

        void addKey(StateVarKey<int> keyIn) {
            Ikeys.Add(keyIn);
        }
        void addKey(StateVarKey<double> keyIn) {
            Dkeys.Add(keyIn);
        }
        void addKey(StateVarKey<float> keyIn) {
            Fkeys.Add(keyIn);
        }
        void addKey(StateVarKey<bool> keyIn) {
            Bkeys.Add(keyIn);
        }
        void addKey(StateVarKey<Matrix<double>> keyIn) {
            Mkeys.Add(keyIn);
        }
        void addKey(StateVarKey<Quat> keyIn) {
            Qkeys.Add(keyIn);
        }
    }
}//HSFSubsystem
//}//HSFSystem