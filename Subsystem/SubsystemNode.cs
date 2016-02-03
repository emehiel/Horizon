using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Horizon_Utilities;


namespace Subsystem
{
		public class SubsystemNode{
			private bool _isChecked {get; private set};
			private bool _scriptingEnabled;
			private Subsystem _subsystem {get; private set};
			private Asset _asset {get; private set};
			private int _nAsset {get; set};
			private List<SubsystemNode> _preceedingNodes {get;  private set}; //should we just have a linked list of nodes?
			private NodeDependencies _dependencies {get; set};
			
			public SubsystemNode()
            {
				_dependencies = new NodeDependencies();
			}
			
			public SubsystemNode(const Subsystem subsystem, Asset asset){
				_subsystem = subsystem;
				_asset = asset;
			}
			
			public SubsystemNode(const SubsystemNode other){
				_isChecked = other.isChecked;
				_scriptingEnabled = other.scriptingEnabled;
				_subsystem = other.subsystem;
				_asset = other.asset;
				_nAsset = other.nAsset;
				_dependencies = new NodeDependencies(other.dependencies);
			}
			
			public void reset(){
				_isChecked = false;
			}
		    public setThreadNum(int threadNum){
                dependencies.setThreadNum();
            }
            
			public void addDependency(string key, IntDependency dep){
				_dependencies.addDependency(key, dep);
			}
			
			public void addDependency(string key, DoubleDependency dep){
				_dependencies.addDependency(key, dep);
			}
			
			public void addDependency(string key, FloatDependency dep){
				_dependencies.addDependency(key, dep);
			}
			
			public void addDependency(string key, BoolDependency dep){
				_dependencies.addDependency(key, dep);
			}
			
			public void addDependency(string key, MatrixDependency dep){
				_dependencies.addDependency(key, dep);
			}
			
			public void addDependency(string key, QuatDependency dep){
				_dependencies.addDependency(key, dep);
			}
			
			public void addIntScriptedDependency(string callKey, string key){
				_dependencies.addIntScriptedDependency(callKey, key);
			}
			
			public void addIntScriptedDependency(string callKey, string key) {
				_dependencies.addIntScriptedDependency(callKey, key);
			}
			
			public void addDoubleScriptedDependency(string callKey, string key) {
				_dependencies.addDoubleScriptedDependency(callKey, key);
			}
			
			public void addFloatScriptedDependency(string callKey, string key) {
				_dependencies.addFloatScriptedDependency(callKey, key);
			}
			
			public void addBoolScriptedDependency(string callKey, string key) {
				_dependencies.addBoolScriptedDependency(callKey, key);
			}
			
			public void addMatrixScriptedDependency(string callKey, string key) {
				_dependencies.addMatrixScriptedDependency(callKey, key);
			}
			
			public void addQuatScriptedDependency(string callKey, string key) {
				_dependencies.addQuatScriptedDependency(callKey, key);
			}
			
			public void enableScriptingSupport() {
				_scriptingEnabled = true;
				_dependencies.enableScriptingSupport();
			}
			
			public void disableScriptingSupport(){
				_scriptingEnabled = false;
				_dependencies.disableScriptingSupport();
			}
			
			public void setPyState(pyState state){
				_dependencies.setPyState(state);
			}
			
			public void getPyState(){
				return _dependencies.getPyState();
			}
			
			public void addPreceedingNode(SubsystemNode node){
				_preceedingNodes.push_back(node); //TODO: still need to implement push_back
			}
			
			public bool canPerform(State newState, const Task task,
									 Environment environment, 
									 const double evalToTime, 
									 const bool mustEvaluate ){
				foreach (SubsystemNode nodeIt in _precedingNodes)
				{
					if (!(nodeIt.isChecked))
					{
						if(!(nodeIt.canPerform(newState, task, environmnet, 
												evalToTime, mustEvaluate)))
							return false;
					}
				}
				if(_isChecked)
				{
					_isChecked = true;
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