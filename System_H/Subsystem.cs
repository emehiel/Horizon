using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using HSFScheduler;
using Universe;
//namespace HSFSystem
//{
    namespace HSFSubsystem
    {
        public class Subsystem {
            protected string Name { get; private set; }
            protected Stack<StateVarKey<int>> Ikeys { get; private set; }
            protected Stack<StateVarKey<double>> Dkeys { get; private set; }
            protected Stack<StateVarKey<float>> Fkeys { get; private set; }
            protected Stack<StateVarKey<bool>> Bkeys { get; private set; }
            protected Stack<StateVarKey<Matrix>> Mkeys { get; private set; }
            protected Stack<StateVarKey<Quat>> Qkeys { get; private set; }

            public Subsystem() { //TODO: Do we want a default constructor?
            }
            public Subsystem(string name) {
                Name = name;
            }
            virtual Subsystem clone() { }
            virtual bool canPerform(State oldState, State newSTate,
                                      Task task, DynamicState position,
                                      Environment environment,
                                      NodeDependencies, dep) {
                throw new NotImplementedException();
            }
            virtual bool canExtend(State newState, DynamicState position, Environment environment, double evalToTime,
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
            void addKey(StateVarKey<Matrix> keyIn) {
                Mkeys.Push(keyIn);
            }
            void addKey(StateVarKey<Quat> keyIn) {
                Qkeys.Push(keyIn);
            }
        }
    }//HSFSubsystem
//}//HSFSystem