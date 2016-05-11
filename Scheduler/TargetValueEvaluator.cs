using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSFScheduler
{
    public class TargetValueEvaluator : Evaluator
    {
        public override double Evaluate(SystemSchedule schedule)
        {
            double sum = 0;
            /* TODO: Commented out to compile

foreach (StateHistory asset in schedule.AssetScheds)
    foreach (Event envent in asset.Events)
    {
        sum += 5 - envent.Tasks.Target.Value;
        if (env.Task.Type == MissionElements.taskType.COMM)
        {

            double ts = env.State.TaskStart;
            double te = env.State.TaskEnd;
            double beginDataRatio = env.State.getValueAtTime()
        }
        
        }
        */
         return sum;
        }
    }
}
