using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subsystem
{
		class Subsystem{
			protected const string _name{get; private set;}
            protected List<StateVarKey<int>> ikeys {get; private set}
            protected List<StateVarKey<double>> dkeys {get; private set}
            protected List<StateVarKey<float>> fkeys {get; private set}
            protected List<StateVarKey<bool>> bkeys {get; private set}
            protected List<StateVarKey<Matrix>> mkeys {get; private set}
            protected List<StateVarKey<Quat>> qkeys {get; private set}
			public Subsystem(){ //TODO: Do we want a default constructor?
			}
			public Subsystem(const string name){
				_name = name
			}
            virtual Subsystem clone(){}
            virtual bool can perform (const State oldState, State newSTate, 
                                      const Task tash, DynamicState position,
                                      Environment environment, 
                                      NodeDependencies, dep){}
            virtual bool can Extend(State newState, DynamicState position,
                                    Environment environment,
                                    const double evalToTime,
                                    NodeDependencies dependencies){}
                                    
            void addKey(const StateVarKey<int> keyin){
                ikeys.push_back(keyIn);
            }
            void addKey(const StateVarKey<double> keyin){
                dkeys.push_back(keyIn);
            }
            void addKey(const StateVarKey<float> keyin){
                fkeys.push_back(keyIn);
            }
            void addKey(const StateVarKey<bool> keyin){
                bkeys.push_back(keyIn);
            }
            void addKey(const StateVarKey<Matrix> keyin){
                mkeys.push_back(keyIn);
            }
            void addKey(const StateVarKey<Quat> keyin){
                qkeys.push_back(keyIn);
            }
		}
}