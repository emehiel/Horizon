using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;
using MissionElements;
using HSFSystem;

namespace HSFScheduler
{
    public class SystemSchedule
    {
        #region Attributes
        public List<AssetSchedule> AssetScheds; //pop never gets used so just use list
        public double ScheduleValue;
        #endregion

        #region Constructors
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
                    Event eventToAdd = new Event(task, new SystemState(assetSchedAccess.Item1.GetLastState(), newTaskStartTime));
                    AssetScheds.Add(new AssetSchedule(assetSchedAccess.Item1, eventToAdd, assetSchedAccess.Item2.Asset));
                    //TODO: double check c# implementation above
                   // shared_ptr<Event> eventToAdd(new Event(*tIt, new State((*assSchedIt)->getLastState(), newTaskStartTime)));
                   // assetscheds.push_back(new assetSchedule(*assSchedIt, eventToAdd));
                }
                else
                    AssetScheds.Add(DeepCopy.Copy<AssetSchedule>(assetSchedAccess.Item1));
            }
            
        }
        #endregion

        public bool CanAddTasks(Stack<Access> newAccessList, double newTaskStartTime)
        {
            int count = 0;
            // vector<assetSchedule*>::iterator asIt2 = assetscheds.begin();
            //int asIt2 = 0;
	        foreach(var accessAssetSched in newAccessList.Zip(AssetScheds, Tuple.Create))
            {
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
				        if(accessAssetSched.Item2.GetLastState().EventEnd > newTaskStartTime)
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

        public SystemState getSubsystemNewState(Asset asset)
        {
            return GetAssetSchedule(asset).GetLastState();
        }

        public Task getSubsytemNewTask(Asset asset)
        {
            return GetAssetSchedule(asset).GetLastTask();
        }

        public AssetSchedule GetAssetSchedule(Asset asset)
        {
            return AssetScheds.Find(item => item.Asset == asset);
        }

        public double getLastTaskStart()
        {
	        double lasttime = 0;
            foreach(var assetSchedule in AssetScheds)
		        if(!assetSchedule.isEmpty())
			        lasttime = lasttime > assetSchedule.GetLastState().TaskStart ? lasttime : assetSchedule.GetLastState().TaskStart;
	        return lasttime;
        }

        public List<SystemState> GetEndStates()
        {
	        List<SystemState> endStates = new List<SystemState>();
            foreach(AssetSchedule asIt in AssetScheds)
		        endStates.Add(asIt.GetLastState());
	        return endStates;
        }

        bool schedGreater(SystemSchedule elem1, SystemSchedule elem2)
        {
            return elem1.ScheduleValue > elem2.ScheduleValue;
        }

    }
}