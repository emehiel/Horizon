// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Linq;
using HSFSubsystem;
using Utilities;
using MissionElements;
using HSFUniverse;

namespace HSFSystem
{
    [Serializable]
    public class SystemClass
    {
        public List<Asset> Assets { get; private set; }
        public List<Subsystem> Subsystems{get; private set;}
        public List<Constraint> Constraints{get; private set;}
        public Universe Environment{get; private set;}
        public int ThreadNum{get; private set;}

        
        public SystemClass(List<Asset> assets, List<Subsystem> subsystems,
                         List<Constraint> constraints, Universe environment)
        {
            Assets = assets;
            Constraints = constraints;
            Environment = environment;
            Subsystems = new List<Subsystem>();
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
 

        
        private bool checkSubForCircularDependencies(Subsystem currSub,
                                                     Subsystem beginSub){
            bool hasCircDep = false;
            List<Subsystem> depSubs = currSub.DependentSubsystems;
            if(depSubs.Any()){ 
                foreach(Subsystem sub in depSubs){
                    hasCircDep |= sub == beginSub;
                    if(hasCircDep)
                         break;
                    hasCircDep |= checkSubForCircularDependencies(sub, beginSub);
                    if(hasCircDep)
                        break;
                }
            }
            return hasCircDep;
        }
        //I hope this never gets used ayways
        //void setDependencies(Dependencies deps)
        //{
        //    foreach(Subsystem subIt in Subsystems)
        //    {
        //        subIt.setDependencies(deps);
        //    }
        //}
        
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
