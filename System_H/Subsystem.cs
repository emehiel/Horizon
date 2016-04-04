using System;
using System.Collections.Generic;
using Utilities;
//using HSFScheduler;
using HSFUniverse;
using HSFSystem;
    namespace HSFSubsystem
    {
    //TODO: Make subsystem abstract
        public class Subsystem {
            protected string Name { get; private set; }
            protected Stack<StateVarKey<int>> Ikeys { get; private set; }
            protected Stack<StateVarKey<double>> Dkeys { get; private set; }
            protected Stack<StateVarKey<float>> Fkeys { get; private set; }
            protected Stack<StateVarKey<bool>> Bkeys { get; private set; }
            protected Stack<StateVarKey<Matrix<double>>> Mkeys { get; private set; }
            protected Stack<StateVarKey<Quat>> Qkeys { get; private set; }

            public Subsystem(string name) {
                Name = name;
            }
            public virtual Subsystem clone() {
                throw new NotImplementedException();
            }
            public virtual bool canPerform(DynamicState oldState, DynamicState newSTate,
                                      Task task, DynamicState position,
                                      Universe environment,
                                      NodeDependencies dep) {
                throw new NotImplementedException();
            }
            public virtual bool canExtend(DynamicState newState, DynamicState position, Universe environment, double evalToTime,
                                    NodeDependencies dependencies) {
                throw new NotImplementedException();
            }

            void addKey(StateVarKey<int> keyIn) {
                Ikeys.Push(keyIn);
            }
            void addKey(StateVarKey<double> keyIn) {
                Dkeys.Push(keyIn);
            }
            void addKey(StateVarKey<float> keyIn) {
                Fkeys.Push(keyIn);
            }
            void addKey(StateVarKey<bool> keyIn) {
                Bkeys.Push(keyIn);
            }
            void addKey(StateVarKey<Matrix<double>> keyIn) {
                Mkeys.Push(keyIn);
            }
            void addKey(StateVarKey<Quat> keyIn) {
                Qkeys.Push(keyIn);
            }
        }
    }//HSFSubsystem
//}//HSFSystem