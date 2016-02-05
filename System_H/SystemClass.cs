using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Universe;
namespace System_H
{
    class SystemClass
    {
        public List<Asset> _assets{get; private set;}
        public List<SubsystemNode> _subsystemNodes{get; private set;}
        public List<Constraint> _constraints{get; private set;}
        public Universe.Environment _environment{get; private set;}
        public int _threadNum{get; private set;}

        public SystemClass() { 
            
        }
        
        public SystemClass(List<Asset> assets, List<SubsystemNode> subsystems
                         List<Constraint> constraints, Universe.Environment environment){
            _assets = assets;
            _subsystemNodes = subsystems;
            _constraints = constraints;
            _environment = environment;
            foreach (SubsystemNode nIt in _subsystemNodes)
            {
                for(int nAsset = 0; nAsset<_assets.Count; nAsset++){
                    if(nIt._asset == _assets[nAsset])
                        nIt._nAsset = nAsset;
                }
            }
        }
        
        public SystemClass(System other){
            // First make a deep copy of the SubsystemNode's
            _assets = other.assets;
            _environment = other.environment;
            foreach(SubsystemNode oSubIt in other._subsystemNodes){
                _subsystemNodes.push_back(new SubstemNode(oSubIt));
            }
            // Now need to fill in the preceeding nodes
	        // Iterate through original subsystems
            var both = other._subsystemNodes.Zip(_subsystemNodes (a, b) => new 
                        {Other = a, This = b}));
            foreach(var it in both){
                List<SubsystemNode> preceedingNodes = both.Other._preceedingNodes();
                foreach(SubsystemNode prevSubIt in preceedingNodes){
                    
            }
            // Next create a copy of the constraints (using clone() interface) and resetting previous nodes
	        // In outer loop make a deep copy of the Constraint's
            foreach(Constraint oConIt in other.constraints){
                Constraint newConstraint oConIt.clone(); 
                // In inner loop reset the constrained nodes
		        // Loop through constrained nodes
                foreach(SubsystemNode oConNodeIt in oConIt._subsystemNodes()){//TODO: check this
                    //TODO: need to find index of current sub
                    //index = pos - other.subsystemNodes.beginNode()
                    //TODO: add preceding node at index
                }
                _constraints.push_back(newConstraint);
            }

                
            }
        }
        
        public bool canPerform(systemSchedule sysSched){
            foreach(SubsystemNode subNodeIt in _subsystemNodes){
                subNodeIt.reset();
            }
            foreach(Constraint constrainIt in _constraints){
                if(!checkSubs(constraintIt._subsystemNodes(), sysSched, true))
                    return false;
                if(!constrainIt.accepts(sysSched))
                    return false;
            }
            if(!checkSubs(subsystemNodes, sysSched, false))
                return false;
            
            return true;
        }
        
        public bool checkForCircularDependencies(){
            bool hasCircDep = flase;
            foreach(SubsystemNode nodeIt in _subsystemNodes){
                SubsystemNode currNode = nodeIt;
                hasCircDep |= checkSubForCircularDependencies(nodeIt, nodeIt);
                if(hasCircDep)
                    break;
            }
            return hasCircDep;
        }
        
        private bool checkSubs(List<SubstemNode> subNodeList, 
                               systemSchedule sySched, bool mustEvaluate){
            int subAssetNum;
            foreach(SubstemNode subNodeIt in subNodeList){
                subAssetNum = subNodeIt._nAsset;
                if(subNodeIt.canPerform(sysSched.getSubNewState(subAssetNum),
                                        sysSched.getSubNewTask(subAssetNum), 
                                        environment, 
                                        sysSched.getLatTastStart(),
                                        mustEvaluate))
                    return false;
            }
            return true;                           
        }
        
        private bool checkSubForCircularDependencies(SubsystemNode currNode,
                                                     SubsystemNode beginNode){
            bool hasCircDep = false;
            List<SubsystemNode> preceedingNodes = currNode._preceedingNodes;
            if(!preceedingNodes.Any()){ //might have to write an Any method
                foreach(SubsystemNode nodeIt in preceedingNodes){
                    hasCircDep |= nodeIt == beginNode;
                    if(hasCircDep)
                         break;
                    hasCircDep |= checkSubForCircularDependencies(nodeIt, beginNode);
                    if(hasCircDep)
                        break;
                }
            }
            return hasCircDep;
        }
/*        
        public List<Asset> getAssets(){
            
        }
        
        public List<SubsystemNode> getSubsystemNodes(){
            
        }
        
        public List<Constraint> getConstraints(){
            
        }
        
        public Environment getEnvironment(){
            
        }
        */

      

    }
}
