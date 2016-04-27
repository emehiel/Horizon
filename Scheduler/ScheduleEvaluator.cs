using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSFScheduler;

namespace HSFScheduler
{
    public abstract class ScheduleEvaluator
    {

        /**
         * Analyzes the given schedule and assigns a value to it.
         * @param schedule the schedule to be evaluated
         * @return the value of the schedule
         */
        public abstract double evaluate(SystemSchedule schedule);

    }
}
