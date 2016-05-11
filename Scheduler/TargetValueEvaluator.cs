using System;
using System.Collections.Generic;
using HSFSystem;
using MissionElements;
using Utilities;

namespace HSFScheduler
{
    public class TargetValueEvaluator : Evaluator
    {
        Dependencies Dependencies;
        public TargetValueEvaluator(Dependencies dependencies)
        {
            Dependencies = dependencies;
        }
        public override double Evaluate(SystemSchedule schedule)
        {
            double sum = 0;
            foreach(Event eit in schedule.AllStates.Events)
            {
                foreach(KeyValuePair<Asset, Task> assetTask in eit.Tasks)
                {
                    Task task = assetTask.Value;
                    sum += 5 - task.Target.Value;
                    if(task.Type == TaskType.COMM)
                        sum += (double)Dependencies.getDependencyFunc("EvalfromSSDR").DynamicInvoke(eit);
                }
            }
            return sum;
        }
    }
}
