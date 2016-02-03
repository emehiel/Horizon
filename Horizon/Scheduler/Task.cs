using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler
{
    /// <summary>
    /// An action to be performed at a target, with limitations and suggestions for scheduling.
    /// @author Einar Pehrson
    /// @author Eric Mehiel
    /// </summary>
    class Task
    {
        /* the type of task being performed */
        taskType Type { get; }

        /* the target associated with the task */
        Target Target { get; }

        /** The maximum number of times the task should be performed by the ENTIRE SYSTEM (all assets count towards this)*/
        int MaxTimesToPerform { get; }


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
