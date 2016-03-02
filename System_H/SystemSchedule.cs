using HSFScheduler;
using System.Collections.Generic;

namespace HSFSystem
{
    public class SystemSchedule
    {
        public List<AssetScheule> AssetSched;
        public double ScheduleValue;

        public sysystemSchedule(List<State> initialstates) 
        {
            ScheduleValue = 0;
            for (vector<State*>::const_iterator stIt = initialstates.begin(); stIt != initialstates.end(); stIt++)
            {
                assetscheds.push_back(new assetSchedule(*stIt));
            }
        }

        systemSchedule::systemSchedule(const systemSchedule* oldSchedule, const vector<const Task*>& newTaskList, double newTaskStartTime) : scheduleValue(0)
        {
            vector <const Task*>::const_iterator tIt = newTaskList.begin();
            for (vector<assetSchedule*>::const_iterator assSchedIt = oldSchedule->assetscheds.begin(); assSchedIt != oldSchedule->assetscheds.end(); assSchedIt++, tIt++)
            {
                if (*tIt != NULL)
                {
                    shared_ptr<Event> eventToAdd(new Event(*tIt, new State((*assSchedIt)->getLastState(), newTaskStartTime)));
                    assetscheds.push_back(new assetSchedule(*assSchedIt, eventToAdd));
                }
                else
                    assetscheds.push_back(new assetSchedule(*assSchedIt));
            }
        }

    }
}