using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using HSFScheduler;
using Universe;
namespace Subsystem
{
		class Subsystem{
			protected string _name{get; private set;}
            protected List<StateVarKey> ikeys { get; private set; }
            protected List<StateVarKey> dkeys { get; private set; }
            protected List<StateVarKey> fkeys { get; private set; }
            protected List<StateVarKey> bkeys { get; private set; }
            protected List<StateVarKey> mkeys { get; private set; }
            protected List<StateVarKey> qkeys { get; private set; }
			public Subsystem(){ //TODO: Do we want a default constructor?
			}
			public Subsystem(string name){
				_name = name
			}
            virtual Subsystem clone(){}
            virtual bool canPerform (State oldState, State newSTate, 
                                      Task task, DynamicState position,
                                      Environment environment, 
                                      NodeDependencies, dep){}
            virtual bool canExtend(State newState, DynamicState position, Environment environment, double evalToTime,
                                    NodeDependencies dependencies){}
                                    
            void addKey(StateVarKey<int> keyin){
                ikeys.push_back(keyIn);
            }
            void addKey(StateVarKey<double> keyin){
                dkeys.push_back(keyIn);
            }
            void addKey(StateVarKey<float> keyin){
                fkeys.push_back(keyIn);
            }
            void addKey(StateVarKey<bool> keyin){
                bkeys.push_back(keyIn);
            }
            void addKey(StateVarKey<Matrix> keyin){
                mkeys.push_back(keyIn);
            }
            void addKey(StateVarKey<Quat> keyin){
                qkeys.push_back(keyIn);
            }
		}
}