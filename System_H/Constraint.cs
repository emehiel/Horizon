using System.Collections.Generic;
using HSFSubsystem;
using Utilities;

namespace HSFSystem
{
    public class Constraint{
        //List of subsystem nodes on which the Constraint operates
        public List<Subsystem> SubsystemNodes{get; private set;}

        public void AddConstrainedSubNode(Subsystem node)
        {
            SubsystemNodes.Add(node);
        }

        /**
         * Applies the constraint to the appropriate variables in the given state,
         * that contains updated state data for all the requested Subsystems.
         * @param state a partially updated state
         * @return true if the state passes the constraint check
         */
         /*
        public virtual bool accepts(SystemSchedule sched)
        {
            return false;
        }
        */
        public virtual Constraint clone()
        {
            return DeepCopy.Copy<Constraint>(this);
        }

    }
}