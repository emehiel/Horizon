using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace HSFScheduler
{
    /// <summary>
    /// An action to be performed at a target, with limitations and suggestions for scheduling.
    /// @author Einar Pehrson
    /// @author Eric Mehiel
    /// </summary>
    public class Task
    {
        /* the type of task being performed */
         public taskType Type { get; private set; }

        /* the target associated with the task */
        public Target Target { get; private set; }

        /** The maximum number of times the task should be performed by the ENTIRE SYSTEM (all assets count towards this)*/
        public int MaxTimesToPerform { get; private set; }


        /**
        * Creates a new task to be performed at the given target, with the given
        * scheduling limitations
        * @param Type the type of task to perform
        * @param Target the target at which the task is to be performed
        * @param MaxTimesToPerform the maximum number of times the task should be performed
        */
        // ToDo: convert taskType to an extensable enumeration
        public Task(taskType taskType, Target target, int maxTimesToPerform)
        {
            Type = taskType;
            Target = target;
            MaxTimesToPerform = maxTimesToPerform;
        }
    }

    public enum taskType { EMPTY, COMM, IMAGING }
}
