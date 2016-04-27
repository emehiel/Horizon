using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSFScheduler
{
    class TargetValueSchedulerEvaluator : ScheduleEvaluator
    {
        public override double evaluate(SystemSchedule schedule)
        {
            double sum = 0;
            foreach (AssetSchedule asset in schedule.AssetScheds)
                foreach (Event envent in asset.Events)
                {
                    sum += 5 - envent.Task.Target.Value;
                    /*if (env.Task.Type == MissionElements.taskType.COMM)
                    {
                       
                        double ts = env.State.TaskStart;
                        double te = env.State.TaskEnd;
                        double beginDataRatio = env.State.getValueAtTime()
                    }
                    */
                }
            return sum;

        }
    }
}
