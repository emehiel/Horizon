using System.Collections.Generic;
using HSFSubsystem;
using Utilities;

/*
#include "systemSchedule.h"
#include "horizon/SubsystemNode.h"
#include "horizon/log/Logger.h"

*/
namespace HSFSystem
{
    public class Constraint{
        //List of subsystem nodes on which the Constraint operates
        public List<SubsystemNode> SubsystemNodes{get; private set;}

        public void addConstrainedSubNode(SubsystemNode node){
            SubsystemNodes.Add(node);
        }


        public void AddConstrainedSubNode(SubsystemNode node)
        {
            SubsystemNodes.Add(node);
        }

        /**
         * Applies the constraint to the appropriate variables in the given state,
         * that contains updated state data for all the requested Subsystems.
         * @param state a partially updated state
         * @return true if the state passes the constraint check
         */
        public virtual bool accepts(SystemSchedule sched)
        {
            return false;
        }

        public virtual Constraint clone()
        {
            return DeepCopy.Copy<Constraint>(this);
        }

    }
}