using System.Collections.Generic;
using System.Linq;
using HSFSubsystem;
using Utilities;
using MissionElements;

namespace HSFSystem
{
    public class SystemClass
    {
        public List<Asset> Assets{get; private set;}
        public List<Subsystem> Subsystems{get; private set;}
        public Stack<Constraint> Constraints{get; private set;}
        public HSFUniverse.Universe Environment{get; private set;}
        public int ThreadNum{get; private set;}

        
        public SystemClass(List<Asset> assets, List<Subsystem> subsystems,
                         Stack<Constraint> constraints, HSFUniverse.Universe environment){
            Assets = assets;
            Subsystems = subsystems;
            Constraints = constraints;
            Environment = environment;
            foreach (Subsystem nIt in subsystems)
            {
                Subsystems.Add(nIt);
            }
        }
        
        public SystemClass(SystemClass other){
            SystemClass copy = DeepCopy.Copy<SystemClass>(other);
            Assets = copy.Assets;
            Subsystems = copy.Subsystems;
            Constraints = copy.Constraints;
            Environment = copy.Environment;
            ThreadNum = copy.ThreadNum;
        }
        
        /*
        public bool checkForCircularDependencies(){
            bool hasCircDep = false;
            foreach(Subsystem nodeIt in Subsystems){
                Subsystem currNode = nodeIt;
                hasCircDep |= checkSubForCircularDependencies(nodeIt, nodeIt);
                if(hasCircDep)
                    break;
            }
            return hasCircDep;
        }
 */

        /*
        private bool checkSubForCircularDependencies(Subsystem currNode,
                                                     Subsystem beginNode){
            bool hasCircDep = false;
            Stack<Subsystem> preceedingNodes = currNode.PreceedingNodes;
            if(!preceedingNodes.Any()){ 
                foreach(Subsystem nodeIt in preceedingNodes){
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
            foreach(Subsystem subIt in Subsystems)
            {
                subIt.setDependencies(deps);
            }
        }
        */
        /*        
                public List<Asset> getAssets(){

                }

                public List<Subsystem> getSubsystems(){

                }

                public List<Constraint> getConstraints(){

                }

                public Environment getEnvironment(){

                }
                */



    }
}
