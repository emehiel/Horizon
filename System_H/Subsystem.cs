using System;
using System.Collections.Generic;
using Utilities;
using HSFUniverse;
using HSFSystem;
using MissionElements;
    namespace HSFSubsystem
    {
        public abstract class Subsystem {
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
            throw new NotImplementedException();
        }
        public abstract bool canPerform(SystemState oldState, SystemState newSTate,
                                  Task task, DynamicState position,
                                  Universe environment,
                                  Dependencies dep);
        public abstract bool canExtend(SystemState newState, DynamicState position, Universe environment, double evalToTime,
                                Dependencies dep);

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