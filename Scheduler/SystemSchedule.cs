using System;
using System.Collections.Generic;
using System.Linq;
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

        public SystemSchedule(SystemSchedule oldSchedule, Stack<Access> newAccessList, double newTaskStartTime)
        {
            // TODO (EAM):  Changed this so we need to double check/test
            //int i = 0; //need a double iterator
            foreach(var assetSchedAccess in oldSchedule.AssetScheds.Zip(newAccessList, Tuple.Create))
            {
                Task task = assetSchedAccess.Item2.Task;
                if (task != null)
                {
                    Event eventToAdd = new Event(task, new SystemState(assetSchedAccess.Item1.getLastState(), newTaskStartTime));
                    AssetScheds.Add(new AssetSchedule(assetSchedAccess.Item1, eventToAdd));
                    //TODO: double check c# implementation above
                   // shared_ptr<Event> eventToAdd(new Event(*tIt, new State((*assSchedIt)->getLastState(), newTaskStartTime)));
                   // assetscheds.push_back(new assetSchedule(*assSchedIt, eventToAdd));
                }
                else
                    AssetScheds.Add(DeepCopy.Copy<AssetSchedule>(assetSchedAccess.I));
                i++;
            }
            
        }

        public bool canAddTasks(Stack<Access> newAccessList, double newTaskStartTime)
        {
            int count = 0;
            // vector<assetSchedule*>::iterator asIt2 = assetscheds.begin();
            //int asIt2 = 0;
	        foreach(var accessAssetSched in newAccessList.Zip(AssetScheds, Tuple.Create) {
		        if(accessAssetSched.Item1 != null)
                {
			        foreach(var assetSchedule in AssetScheds)
                    {
				        count += assetSchedule.timesCompletedTask(accessAssetSched.Item1.Task);
                    }
			        if(count >= accessAssetSched.Item1.Task.MaxTimesToPerform)
				        return false;
			        if(!accessAssetSched.Item2.isEmpty())
                    {
				        if(accessAssetSched.Item2.getLastState().EventEnd > newTaskStartTime)
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