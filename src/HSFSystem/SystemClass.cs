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
        #region Attributes
        public List<Asset> Assets { get; private set; }
        public List<Subsystem> Subsystems{get; private set;}
        public List<Constraint> Constraints{get; private set;}
        public Domain Environment{get; private set;}
        public int ThreadNum{get; private set;}
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor for the system class
        /// </summary>
        /// <param name="assets"></param>
        /// <param name="subsystems"></param>
        /// <param name="constraints"></param>
        /// <param name="environment"></param>
        public SystemClass(List<Asset> assets, List<Subsystem> subsystems, List<Constraint> constraints, Domain environment)
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

        /// <summary>
        /// Copy Constructor for the System Class
        /// </summary>
        /// <param name="other"></param>
        public SystemClass(SystemClass other){
            SystemClass copy = DeepCopy.Copy<SystemClass>(other);
            Assets = copy.Assets;
            Subsystems = copy.Subsystems;
            Constraints = copy.Constraints;
            Environment = copy.Environment;
            ThreadNum = copy.ThreadNum;
        }
        #endregion

        #region methods
        /// <summary>
        /// Check for circular dependencies
        /// </summary>
        /// <returns></returns>
        public bool CheckForCircularDependencies()
        {
            bool hasCircDep = false;
            foreach(Subsystem nodeIt in Subsystems){
                Subsystem currNode = nodeIt;
                hasCircDep |= CheckSubForCircularDependencies(nodeIt, nodeIt);
                if(hasCircDep)
                    break;
            }
            return hasCircDep;
        }

        /// <summary>
        /// Recursivley used by CheckForCircularDependencies()
        /// </summary>
        /// <param name="currSub"></param>
        /// <param name="beginSub"></param>
        /// <returns></returns>
        private bool CheckSubForCircularDependencies(Subsystem currSub, Subsystem beginSub)
        {
            bool hasCircDep = false;
            List<Subsystem> depSubs = currSub.DependentSubsystems;
            if(depSubs.Any()){ 
                foreach(Subsystem sub in depSubs){
                    hasCircDep |= sub == beginSub;
                    if(hasCircDep)
                         break;
                    hasCircDep |= CheckSubForCircularDependencies(sub, beginSub);
                    if(hasCircDep)
                        break;
                }
            }
            return hasCircDep;
        }
        #endregion
    }
}
