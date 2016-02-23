using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Universe;
using Subsystem;
using Utilities;

namespace HSFSystem
{
    class SystemClass
    {
        public List<Asset> Assets{get; private set;}
        public Stack<SubsystemNode> SubsystemNodes{get; private set;}
        public Stack<Constraint> Constraints{get; private set;}
        public Universe.Universe Environment{get; private set;}
        public int ThreadNum{get; private set;}

        
        public SystemClass(List<Asset> assets, Stack<SubsystemNode> subsystems,
                         Stack<Constraint> constraints, Universe.Universe environment){
            Assets = assets;
            SubsystemNodes = subsystems;
            Constraints = constraints;
            Environment = environment;
            foreach (SubsystemNode nIt in subsystems)
            {
                SubsystemNodes.Push(nIt);
            }
        }
        
        public SystemClass(SystemClass other){
            SystemClass copy = DeepCopy.Copy<SystemClass>(other);
            Assets = copy.Assets;
            SubsystemNodes = copy.SubsystemNodes;
            Constraints = copy.Constraints;
            Environment = copy.Environment;
            ThreadNum = copy.ThreadNum;
        }
        
        public bool canPerform(systemSchedule sysSched){
            foreach(SubsystemNode subNodeIt in SubsystemNodes){
                subNodeIt.reset();
            }
            foreach(Constraint constrainIt in Constraints){
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
            foreach(SubsystemNode nodeIt in SubsystemNodes){
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
