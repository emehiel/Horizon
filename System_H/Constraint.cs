using System.Collections.Generic;
using HSFSubsystem;
using Utilities;
using MissionElements;

namespace HSFSystem
{
    public abstract class Constraint
    {
        //List of subsystem nodes on which the Constraint operates
        public Subsystem Subsystem { get; protected set; }
      //  public Guid ID;

        //// TODO (EAM): What is this used for? (MY) nothing I hope!
        //public void AddConstrainedSub(Subsystem node)
        //{
        //    Subsystems.Add(node);
        //}

        /**
         * Applies the constraint to the appropriate variables in the given state,
         * that contains updated state data for all the requested Subsystems.
         * @param state a partially updated state
         * @return true if the state passes the constraint check
         */
        
        public virtual bool Accepts(SystemState state)
        {
            return false;
        }
        
        public virtual Constraint clone()
        {
            return DeepCopy.Copy<Constraint>(this);
        }

    }
}