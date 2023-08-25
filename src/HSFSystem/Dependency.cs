// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System.Collections.Generic;
using System;

namespace HSFSystem
{
    /// <summary>
    /// A singleton class to hold dependency dictionary
    /// </summary>
    public class Dependency
    {
        static Dependency _instance = null;

        /// <summary>
        /// Call Key to dependency function dictionary
        /// </summary>
        private Dictionary<string, Delegate> DependencyFunctions;

        /// <summary>
        /// Private Constructor for singleton class
        /// </summary>
        private Dependency()
        {
            DependencyFunctions = new Dictionary<string, Delegate>();
        }

        /// <summary>
        /// Create a singleton instance of the Dependency dictionary
        /// </summary>
        public static Dependency Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new Dependency();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Add a new dependency function and its call key to the dictionary. A new dependency 
        /// added with the same callkey will be overwritten
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="func"></param>
        public void Add(string callKey, Delegate func)
        {
            if (DependencyFunctions.ContainsKey(callKey))
                DependencyFunctions.Remove(callKey); // Do this if yes
            DependencyFunctions.Add(callKey, func); // Always do this
        }

        /// <summary>
        /// Retrieve a specific Delegate dependency function from the dictionary.
        /// </summary>
        /// <param name="callKey"></param>
        /// <returns></returns>
        public Delegate GetDependencyFunc(string callKey)
        {
            Delegate ret;
            if(DependencyFunctions.TryGetValue(callKey, out ret))
                return ret;
             throw new KeyNotFoundException();
        }

        /// <summary>
        /// Append a Dictionary of dependency functions to the already existing dictionary
        /// </summary>
        /// <param name="newDeps"></param>
        public void Append(Dictionary<string, Delegate> newDeps)
        {
            foreach (var dep in newDeps)
            {
                Add(dep.Key, dep.Value);
            }
        }
    }
}

