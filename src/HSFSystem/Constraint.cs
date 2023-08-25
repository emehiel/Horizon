// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using MissionElements;

namespace HSFSystem
{
    [Serializable]
    public abstract class Constraint
    {
        // List of subsystem nodes on which the Constraint operates
        public List<Subsystem> Subsystems { get; protected set; }

        // Constraint Name for identification in post processsing
        public string Name;
       
        /// <summary>
        /// Applies the constraint to the appropriate variables in the given state,
        /// that contains updated state data for all the requested Subsystems.
        /// what the hell is this?
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public virtual bool Accepts(SystemState state)
        {
            return false;
        }
    }
}