using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;


namespace Subsystem
{
		public class SubsystemNode{
			public bool IsChecked { get; private set; }
			public bool ScriptingEnabled {get; private set; }
			public Subsystem SubsystemType { get; private set; }
			public Asset SubystemAsset { get; private set; }
			public int NAsset { get; private set; }
			public List<SubsystemNode> PreceedingNodes { get; private set; } //should we just have a linked list of nodes?
			public NodeDependencies SubsystemDependencies { get; set; }

        public SubsystemNode()
            {
				SubsystemDependencies = new NodeDependencies();
			}
			
			public SubsystemNode(Subsystem subsystem, Asset asset){
				SubsystemType = subsystem;
				SubsystemAsset = asset;
			}
			
			public SubsystemNode(SubsystemNode other){
				IsChecked = other.IsChecked;
				ScriptingEnabled = other.ScriptingEnabled;
				SubsystemType = other.SubsystemType;
			    SubsystemAsset = other.SubsystemAsset;
				NAsset = other.NAsset;
				SubsystemDependencies = new NodeDependencies(other.SubsystemDependencies);
			}
			
			public void reset(){
				IsChecked = false;
			}
		    public void setThreadNum(int threadNum){
                SubsystemDependencies.setThreadNum();
            }
            
			public void addDependency(string key, IntDependency dep){
				SubsystemDependencies.addDependency(key, dep);
			}
			
			public void addDependency(string key, DoubleDependency dep){
				SubsystemDependencies.addDependency(key, dep);
			}
			
			public void addDependency(string key, FloatDependency dep){
				SubsystemDependencies.addDependency(key, dep);
			}
			
			public void addDependency(string key, BoolDependency dep){
				SubsystemDependencies.addDependency(key, dep);
			}
			
			public void addDependency(string key, MatrixDependency dep){
                SubsystemDependencies.addDependency(key, dep);
			}
			
			public void addDependency(string key, QuatDependency dep){
                SubsystemDependencies.addDependency(key, dep);
			}
			
			public void addIntScriptedDependency(string callKey, string key){
                SubsystemDependencies.addIntScriptedDependency(callKey, key);
			}
			
			public void addIntScriptedDependency(string callKey, string key) {
                SubsystemDependencies.addIntScriptedDependency(callKey, key);
			}
			
			public void addDoubleScriptedDependency(string callKey, string key) {
              SubsystemDependencies.addDoubleScriptedDependency(callKey, key);
			}
			
			public void addFloatScriptedDependency(string callKey, string key) {
               SubsystemDependencies.addFloatScriptedDependency(callKey, key);
			}
			
			public void addBoolScriptedDependency(string callKey, string key) {
                 SubsystemDependencies.addBoolScriptedDependency(callKey, key);
			}
			
			public void addMatrixScriptedDependency(string callKey, string key) {
                SubsystemDependencies.addMatrixScriptedDependency(callKey, key);
			}
			
			public void addQuatScriptedDependency(string callKey, string key) {
				SubsystemDependencies.addQuatScriptedDependency(callKey, key);
			}
			
			public void enableScriptingSupport() {
				_scriptingEnabled = true;
				SubsystemDependencies.enableScriptingSupport();
			}
			
			public void disableScriptingSupport(){
				_scriptingEnabled = false;
				SubsystemDependencies.disableScriptingSupport();
			}
			
			public void setPyState(pyState state){
				SubsystemDependencies.setPyState(state);
			}
			
			public void getPyState(){
				return SubsystemDependencies.getPyState();
			}
			
			public void addPreceedingNode(SubsystemNode node){
				PreceedingNodes.push_back(node); //TODO: still need to implement push_back
			}
			
			public bool canPerform(State newState, const Task task,
									 Universe environment, 
									 const double evalToTime, 
									 const bool mustEvaluate ){
				foreach (SubsystemNode nodeIt in _precedingNodes)
				{
					if (!(nodeIt.IsChecked))
					{
						if(!(nodeIt.canPerform(newState, task, environmnet, 
												evalToTime, mustEvaluate)))
							return false;
					}
				}
				if(IsChecked)
				{
					IsChecked = true;
					if(newState.getTaskStart() >= evalToTime && task != NULL)
						return _subsystem.canPerform(newState.getPrevious(), 
													newState, task,
													asset.getPos(), 
													environment, dependencies);
					else
					{
						if(mustEvaluate)
							return _subsystem.canExtend(newState, 
															asset.getPos(),
															environment, 
															evalToTime,
															dependencies);
						else
							return true;
					}
				}
				else
					return true;
			}
		}// end class SubsystemNode
}// end namespace System_H