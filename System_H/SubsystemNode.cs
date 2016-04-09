using System;
using System.Collections.Generic;
using Utilities;
using HSFSystem;
using UserModel;
using HSFUniverse;
using MissionElements;

//namespace HSFSystem
//{
namespace HSFSubsystem
{
    public class SubsystemNode
    {
        public bool IsChecked { get; private set; }
  //      public bool ScriptingEnabled { get; private set; }
        public Subsystem Subsystem { get; private set; }
        public Asset SubsystemAsset { get; private set; } //to get postion because Asset hold position of a group of subsystems
        public int NAsset { get; private set; }
        public List<SubsystemNode> PreceedingNodes { get; private set; }
        public PythonState PyState;
        //  public NodeDependencies SubsystemDependencies { get; set; }

        #region Constructors
        public SubsystemNode(Subsystem subsystem, Asset asset, Dependencies dependencies)
        {
            Subsystem = subsystem;
            SubsystemAsset = asset;
            foreach (KeyValuePair<string, Delegate> deps in subsystem.SubsystemDependencyFunctions)
            {
                dependencies.Add(deps.Key, deps.Value);
            }
        }

        public SubsystemNode(SubsystemNode other) //TODO: (morgan) Make sure we don't want to copy preceeding nodes
        {
            IsChecked = other.IsChecked;
      //      ScriptingEnabled = other.ScriptingEnabled;
            Subsystem = other.Subsystem;
            SubsystemAsset = other.SubsystemAsset;
            NAsset = other.NAsset;
       //     SubsystemDependencies = new NodeDependencies(other.SubsystemDependencies);
        }
        #endregion Constructors
        #region AddDependencies

       
        #endregion AddDependencies
        #region GettersAndSetters
        public void reset()
        {
            IsChecked = false;
        }
        //public void setThreadNum(int threadNum)
        //{
        //    SubsystemDependencies.ThreadNum = threadNum;
        //}
        //public void enableScriptingSupport()
        //{
        //    ScriptingEnabled = true;
        //    SubsystemDependencies.ScriptingEnabled = true;
        //}

        //public void disableScriptingSupport()
        //{
        //    ScriptingEnabled = false;
        //    SubsystemDependencies.ScriptingEnabled = false;
        //}

        //public void setPyState(PythonState state)
        //{
        //    SubsystemDependencies.PyState = DeepCopy.Copy<PythonState>(state);
        //}

        //public PythonState getPyState()
        //{
        //    return SubsystemDependencies.PyState;
        //}

        public void addPreceedingNode(SubsystemNode node)
        {
            PreceedingNodes.Add(node);
        }
        public void setDependencies(Dependencies deps)
        {
            throw new NotImplementedException();
        }
#endregion GettersAndSetters
        public bool canPerform(SystemState newState, Task task, Universe environment, double evalToTime, bool mustEvaluate, Dependencies dep)
            {
            //TODO: Make sure to go backwards through tree
                foreach (SubsystemNode nodeIt in PreceedingNodes)
                {
                    if (!(nodeIt.IsChecked))
                    {
                        if (!(nodeIt.canPerform(newState, task, environment, evalToTime, mustEvaluate, dep)))
                            return false;
                    }
                }
                if (IsChecked)
                {
                    IsChecked = true;
                    if (newState.TaskStart >= evalToTime && task != null)
                    {
                        return Subsystem.canPerform(newState.Previous, newState, task, SubsystemAsset.AssetDynamicState, environment, dep);
                    }
                    else
                    {
                        if (mustEvaluate)
                            return Subsystem.canExtend(newState, SubsystemAsset.AssetDynamicState, environment, evalToTime, dep);
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