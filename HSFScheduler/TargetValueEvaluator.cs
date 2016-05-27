using System;
using System.Collections.Generic;
using HSFSystem;
using MissionElements;

namespace HSFScheduler
{
    public class TargetValueEvaluator : Evaluator
    {
        #region Attributes
        public Dependency Dependencies;
        #endregion

        #region Constructors
        public TargetValueEvaluator(Dependency dependencies)
        {
            Dependencies = dependencies;
        }
        #endregion

        #region Methods
        public override double Evaluate(SystemSchedule schedule)
        {
            double sum = 0;
            foreach(Event eit in schedule.AllStates.Events)
            {
                foreach(KeyValuePair<Asset, Task> assetTask in eit.Tasks)
                {
                    Task task = assetTask.Value;
                    sum += task.Target.Value;
                    if(task.Type == TaskType.COMM)
                        sum += (double)Dependencies.GetDependencyFunc("EvalfromSSDR").DynamicInvoke(eit);
                }
            }
            return sum;
        }
        #endregion
    }
}
