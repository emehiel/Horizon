using System;
using HSFSystem;

namespace HSFScheduler
{
    public abstract class Evaluator
    {
        /**
        * Analyzes the given schedule and assigns a value to it.
        * @param schedule the schedule to be evaluated
        * @return the value of the schedule
        */
        public abstract double Evaluate(SystemSchedule schedule);
    }
}
