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
        public StateHistory AllStates; //pop never gets used so just use list
        public double ScheduleValue;
        #endregion

        #region Constructors
        public SystemSchedule(SystemState initialstates) 
        {
            ScheduleValue = 0;
            AllStates = new StateHistory(initialstates);
        }

        public SystemSchedule(SystemSchedule oldSchedule, Stack<Access> newAccessList, double newTaskStartTime, StateHistory oldStates)
        {
            // TODO (EAM):  Changed this so we need to double check/test
            //int i = 0; //need a double iterator
            foreach(var assetSchedAccess in newAccessList)
            {
                Task task = DeepCopy.Copy<Task>(assetSchedAccess.Task);
                if (task != null)
                {
                    Event eventToAdd = new Event(task, new SystemState(assetSchedAccess.Item1.GetLastState(), newTaskStartTime));
                    AssetScheds.Add(new StateHistory(assetSchedAccess.Item1, eventToAdd, assetSchedAccess.Item2.Asset));
                    //TODO: double check c# implementation above
                   // shared_ptr<Event> eventToAdd(new Event(*tIt, new State((*assSchedIt)->getLastState(), newTaskStartTime)));
                   // assetscheds.push_back(new assetSchedule(*assSchedIt, eventToAdd));
                }
                else
                    AssetScheds.Add(DeepCopy.Copy<StateHistory>(assetSchedAccess.Item1));
            }
            
        }
        #endregion

        public bool CanAddTasks(Stack<Access> newAccessList, double newTaskStartTime)
        {
            int count = 0;
            // vector<assetSchedule*>::iterator asIt2 = assetscheds.begin();
            //int asIt2 = 0;
	        foreach(var accessAssetSched in newAccessList)
            {
		        if(accessAssetSched != null)
                {
				    count += AllStates.timesCompletedTask(accessAssetSched.Task);
			        if(count >= accessAssetSched.Task.MaxTimesToPerform)
				        return false;
			        if(!AllStates.isEmpty())
                    {
				        if(AllStates.GetLastState().EventEnd > newTaskStartTime)
					        return false;
			        }
		        }
	        }
	        return true;
        }

        public int getTotalNumEvents()
        {
            return AllStates.size();
        }

        public SystemState getSubsystemNewState()
        {
            return AllStates.GetLastState();
        }

        public Task getSubsytemNewTask(Asset asset)
        {
            return AllStates.GetLastTask(asset);
        }

        //public StateHistory GetStateHistory(Asset asset)
        //{
        //    return AllStates.Find(item => item.Asset == asset);
        //}

        public double getLastTaskStart()
        {
	        double lasttime = 0;
            foreach(var assetSchedule in AssetScheds)
		        if(!assetSchedule.isEmpty())
			        lasttime = lasttime > assetSchedule.GetLastState().TaskStart ? lasttime : assetSchedule.GetLastState().TaskStart;
	        return lasttime;
        }

        public SystemState GetEndState()
        {
            return AllStates.GetLastState();
        }

        bool schedGreater(SystemSchedule elem1, SystemSchedule elem2)
        {
            return elem1.ScheduleValue > elem2.ScheduleValue;
        }

    }
}