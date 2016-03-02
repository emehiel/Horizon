using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using HSFSystem;

//namespace HSFSystem
//{
    namespace HSFSubsystem
    {
        public class SubsystemNode
        {
            public bool IsChecked { get; private set; }
            public bool ScriptingEnabled { get; private set; }
            public Subsystem SubsystemField { get; private set; }
            public Asset SubsystemAsset { get; private set; }
            public int NAsset { get; private set; }
            public Stack<SubsystemNode> PreceedingNodes { get; private set; }
            public NodeDependencies SubsystemDependencies { get; set; }

            #region Constructors
            public SubsystemNode()
            {
                SubsystemDependencies = new NodeDependencies();
            }

            public SubsystemNode(Subsystem subsystem, Asset asset)
            {
                SubsystemField = subsystem;
                SubsystemAsset = asset;
            }

            public SubsystemNode(SubsystemNode other)
            {
                IsChecked = other.IsChecked;
                ScriptingEnabled = other.ScriptingEnabled;
                SubsystemField = other.SubsystemField;
                SubsystemAsset = other.SubsystemAsset;
                NAsset = other.NAsset;
                SubsystemDependencies = new NodeDependencies(other.SubsystemDependencies);
            }
            #endregion Constructors
            public void reset()
            {
                IsChecked = false;
            }
            public void setThreadNum(int threadNum)
            {
                SubsystemDependencies.setThreadNum();
            }
            #region AddDependencies

            public void addDependency(string key, IntDependency dep)
            {
                SubsystemDependencies.addDependency(key, dep);
            }

            public void addDependency(string key, DoubleDependency dep)
            {
                SubsystemDependencies.addDependency(key, dep);
            }

            public void addDependency(string key, FloatDependency dep)
            {
                SubsystemDependencies.addDependency(key, dep);
            }

            public void addDependency(string key, BoolDependency dep)
            {
                SubsystemDependencies.addDependency(key, dep);
            }

            public void addDependency(string key, MatrixDependency dep)
            {
                SubsystemDependencies.addDependency(key, dep);
            }

            public void addDependency(string key, QuatDependency dep)
            {
                SubsystemDependencies.addDependency(key, dep);
            }

            public void addIntScriptedDependency(string callKey, string key)
            {
                SubsystemDependencies.addIntScriptedDependency(callKey, key);
            }


            public void addDoubleScriptedDependency(string callKey, string key)
            {
                SubsystemDependencies.addDoubleScriptedDependency(callKey, key);
            }

            public void addFloatScriptedDependency(string callKey, string key)
            {
                SubsystemDependencies.addFloatScriptedDependency(callKey, key);
            }

            public void addBoolScriptedDependency(string callKey, string key)
            {
                SubsystemDependencies.addBoolScriptedDependency(callKey, key);
            }

            public void addMatrixScriptedDependency(string callKey, string key)
            {
                SubsystemDependencies.addMatrixScriptedDependency(callKey, key);
            }

            public void addQuatScriptedDependency(string callKey, string key)
            {
                SubsystemDependencies.addQuatScriptedDependency(callKey, key);
            }
            #endregion AddDependencies
            public void enableScriptingSupport()
            {
                ScriptingEnabled = true;
                SubsystemDependencies.enableScriptingSupport();
            }

            public void disableScriptingSupport()
            {
                ScriptingEnabled = false;
                SubsystemDependencies.disableScriptingSupport();
            }

            public void setPyState(pyState state)
            {
                SubsystemDependencies.setPyState(state);
            }

            public void getPyState()
            {
                return SubsystemDependencies.getPyState();
            }

            public void addPreceedingNode(SubsystemNode node)
            {
                PreceedingNodes.Push(node);
            }
            public void setDependencies(Dependencies deps)
            {
                throw new NotImplementedException();
            }
            public bool canPerform(State newState, Task task, Universe environment, double evalToTime, bool mustEvaluate)
            {
                foreach (SubsystemNode nodeIt in PreceedingNodes)
                {
                    if (!(nodeIt.IsChecked))
                    {
                        if (!(nodeIt.canPerform(newState, task, environment, evalToTime, mustEvaluate)))
                            return false;
                    }
                }
                if (IsChecked)
                {
                    IsChecked = true;
                    if (newState.getTaskStart() >= evalToTime && task != null)
                    {
                        return Subsystem.canPerform(newState.getPrevious(), newState, task, SubsystemAsset.getPos(), environment, SubsystemDependencies);
                    }
                    else
                    {
                        if (mustEvaluate)
                            return Subsystem.canExtend(newState, SubsystemAsset.getPos(), environment, evalToTime, SubsystemDependencies);
                        else
                            return true;
                    }
                }
                else
                    return true;
            }
        }// end class SubsystemNode
    }//end namespace Subsystem
//}// end namespace HSFSystem