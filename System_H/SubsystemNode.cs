using System;
using System.Collections.Generic;
using Utilities;
using HSFSystem;
using UserModel;
using HSFScheduler;
using HSFUniverse;

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
        #region AddDependencies

        public void addDependency(string key, Dictionary<string, HSFProfile<int>> dep)
        {
            SubsystemDependencies.addDependency(key, dep);
        }

        public void addDependency(string key, Dictionary<string, HSFProfile<double>> dep)
        {
            SubsystemDependencies.addDependency(key, dep);
        }

        //public void addDependency(string key, FloatDependency dep)
        //{
        //    SubsystemDependencies.addDependency(key, dep);
        //}

        public void addDependency(string key, Dictionary<string, HSFProfile<bool>> dep)
        {
            SubsystemDependencies.addDependency(key, dep);
        }

        public void addDependency(string key, Dictionary<string, HSFProfile<Matrix<double>>> dep)
        {
            SubsystemDependencies.addDependency(key, dep);
        }

        public void addDependency(string key, Dictionary<string, HSFProfile<Quat>> dep)
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
        #region GettersAndSetters
        public void reset()
        {
            IsChecked = false;
        }
        public void setThreadNum(int threadNum)
        {
            SubsystemDependencies.ThreadNum = threadNum;
        }
        public void enableScriptingSupport()
        {
            ScriptingEnabled = true;
            SubsystemDependencies.ScriptingEnabled = true;
        }

        public void disableScriptingSupport()
        {
            ScriptingEnabled = false;
            SubsystemDependencies.ScriptingEnabled = false;
        }

        public void setPyState(PythonState state)
        {
            SubsystemDependencies.PyState = DeepCopy.Copy<PythonState>(state);
        }

        public PythonState getPyState()
        {
            return SubsystemDependencies.PyState;
        }

        public void addPreceedingNode(SubsystemNode node)
        {
            PreceedingNodes.Push(node);
        }
        public void setDependencies(Dependencies deps)
        {
            throw new NotImplementedException();
        }
#endregion GettersAndSetters
        public bool canPerform(State newState, Task task, Universe environment, double evalToTime, bool mustEvaluate)
            {
            //TODO: Make sure to go backwards through tree
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
                    if (newState.TaskStart >= evalToTime && task != null)
                    {
                        return SubsystemField.canPerform(newState.Previous, newState, task, SubsystemAsset.AssetPosition, environment, SubsystemDependencies);
                    }
                    else
                    {
                        if (mustEvaluate)
                            return SubsystemField.canExtend(newState, SubsystemAsset.AssetPosition, environment, evalToTime, SubsystemDependencies);
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