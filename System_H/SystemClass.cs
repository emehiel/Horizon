using System.Collections.Generic;
using System.Linq;
using HSFSubsystem;
using Utilities;

namespace HSFSystem
{
    public class SystemClass
    {
        public List<Asset> Assets{get; private set;}
        public List<SubsystemNode> SubsystemNodes{get; private set;}
        public Stack<Constraint> Constraints{get; private set;}
        public HSFUniverse.Universe Environment{get; private set;}
        public int ThreadNum{get; private set;}

        
        public SystemClass(List<Asset> assets, List<SubsystemNode> subsystems,
                         Stack<Constraint> constraints, HSFUniverse.Universe environment){
            Assets = assets;
            SubsystemNodes = subsystems;
            Constraints = constraints;
            Environment = environment;
            foreach (SubsystemNode nIt in subsystems)
            {
                SubsystemNodes.Add(nIt);
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
        /*
        public bool canPerform(SystemSchedule sysSched){
            // Iterate through Subsystem Nodes and set that they havent run
            foreach (SubsystemNode subNodeIt in SubsystemNodes){
                subNodeIt.reset();
            }
            // Iterate through constraints
            foreach (Constraint constraintIt in Constraints){
                if(!checkSubs(constraintIt.SubsystemNodes, sysSched, true))
                    return false;
                if(!constraintIt.accepts(sysSched))
                    return false;
            }
            // Check the remaining Subsystems that aren't included in any Constraints
            if (!checkSubs(SubsystemNodes, sysSched, false))
                return false;
            
            return true;
        }
        
        public bool checkForCircularDependencies(){
            bool hasCircDep = false;
            foreach(SubsystemNode nodeIt in SubsystemNodes){
                SubsystemNode currNode = nodeIt;
                hasCircDep |= checkSubForCircularDependencies(nodeIt, nodeIt);
                if(hasCircDep)
                    break;
            }
            return hasCircDep;
        }
 
        private bool checkSubs(List<SubsystemNode> subNodeList, 
                               SystemSchedule sySched, bool mustEvaluate){
            int subAssetNum;
            foreach(SubsystemNode subNodeIt in subNodeList){
                subAssetNum = subNodeIt.NAsset;
                if(subNodeIt.canPerform(sySched.getSubNewState(subAssetNum),
                                        sySched.getSubNewTask(subAssetNum), 
                                        Environment, 
                                        sySched.getLastTaskStart(),
                                        mustEvaluate))
                    return false;
            }
            return true;                           
        }
        
        private bool checkSubForCircularDependencies(SubsystemNode currNode,
                                                     SubsystemNode beginNode){
            bool hasCircDep = false;
            Stack<SubsystemNode> preceedingNodes = currNode.PreceedingNodes;
            if(!preceedingNodes.Any()){ 
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
        void setDependencies(Dependencies deps)
        {
            foreach(SubsystemNode subIt in SubsystemNodes)
            {
                subIt.setDependencies(deps);
            }
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
