using System.Collections.Generic;
using Utilities;
using MissionElements;

namespace HSFScheduler
{
    public class SystemSchedule
    {
        public List<AssetSchedule> AssetScheds; //pop never gets used so just use list
        public double ScheduleValue;

        public SystemSchedule(List<SystemState> initialstates) 
        {
            ScheduleValue = 0;
            foreach(SystemState stIt in initialstates)
            {
                AssetScheds.Add(new AssetSchedule(stIt));
            }
        }

        public SystemSchedule(SystemSchedule oldSchedule, List<Task> newTaskList, double newTaskStartTime)
        {
            int i = 0; //need a double iterator
            foreach(AssetSchedule asIt in oldSchedule.AssetScheds)
            {
                Task tIt = newTaskList[i];
                if (tIt == null)
                {
                    Event eventToAdd = new Event(tIt, new SystemState(asIt.getLastState(), newTaskStartTime));
                    AssetScheds.Add(new AssetSchedule(asIt, eventToAdd));
                    //TODO: double check c# implementation above
                   // shared_ptr<Event> eventToAdd(new Event(*tIt, new State((*assSchedIt)->getLastState(), newTaskStartTime)));
                   // assetscheds.push_back(new assetSchedule(*assSchedIt, eventToAdd));
                }
                else
                    AssetScheds.Add(DeepCopy.Copy<AssetSchedule>(asIt));
                i++;
            }
            
        }

        public bool canAddTasks(Stack<Task> newTaskList, double newTaskStartTime)
        {
            int count = 0;
            // vector<assetSchedule*>::iterator asIt2 = assetscheds.begin();
            int asIt2 = 0;
	        foreach( Task tIt in newTaskList) {
		        if(tIt != null)
                {
			        foreach(AssetSchedule asIt in AssetScheds)
                    {
				        count += asIt.timesCompletedTask(tIt);
                    }
			        if(count >= tIt.MaxTimesToPerform)
				        return false;
			        if(!AssetScheds[asIt2].isEmpty())
                    {
				        if(AssetScheds[asIt2].getLastState().EventEnd > newTaskStartTime)
					        return false;
			        }
		        }
	        }
	        return true;
        }

        public int getTotalNumEvents()
        {
            int count = 0;
            foreach(AssetSchedule asIt in AssetScheds)
                count += asIt.size();
            return count;
        }

        public SystemState getSubNewState(int assetNum)
        {
            if (AssetScheds[assetNum].isEmpty())
                return AssetScheds[assetNum].InitialState;
            else
                return AssetScheds[assetNum].getLastState();
        }

        public Task getSubNewTask(int assetNum)
        {
            if (AssetScheds[assetNum].isEmpty())
                return null;
            else
                return AssetScheds[assetNum].getLastTask();
        }

        public double getLastTaskStart() {
	        double lasttime = 0;
            foreach(AssetSchedule aIt in AssetScheds)
		        if(!aIt.isEmpty())
			        lasttime = lasttime > aIt.getLastState().TaskStart ? lasttime : aIt.getLastState().TaskStart;
	        return lasttime;
        }

        public List<SystemState> getEndStates(){
	        List<SystemState> endStates = new List<SystemState>();
            foreach(AssetSchedule asIt in AssetScheds)
		        endStates.Add(asIt.getLastState());
	        return endStates;
        }

        bool schedGreater(SystemSchedule elem1, SystemSchedule elem2)
        {
            return elem1.ScheduleValue > elem2.ScheduleValue;
        }

    }
}