// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)


using MissionElements;
using HSFUniverse;

namespace HSFSystem
{
    public interface ISubsystem
    {
        /// <summary>
        /// Determine if the subsystem can perform the new task within the event.
        /// If it can, update the state to reflect the task being performed
        /// </summary>
        /// <param name="proposedEvent"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        bool CanPerform(Event proposedEvent, Domain environment);

        /// <summary>
        /// Determine if the susystem's state can progress in time in an "idle" task.
        /// </summary>
        /// <param name="proposedEvent"></param>
        /// <param name="environment"></param>
        /// <param name="evalToTime"></param>
        /// <returns></returns>
        bool CanExtend(Event proposedEvent, Domain environment, double evalToTime); 
    }
}
