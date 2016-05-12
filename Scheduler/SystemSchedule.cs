using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;
using MissionElements;
using HSFSystem;
using UserModel;

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

        public SystemSchedule(SystemSchedule oldSchedule, Stack<Access> newAccessList, double newEventStartTime)
        {
            if (newAccessList != null)
            {
                Dictionary<Asset, Task> tasks = new Dictionary<Asset, Task>();
                Dictionary<Asset, double> taskStarts = new Dictionary<Asset, double>();
                Dictionary<Asset, double> taskEnds = new Dictionary<Asset, double>();
                Dictionary<Asset, double> eventStarts = new Dictionary<Asset, double>();
                Dictionary<Asset, double> eventEnds = new Dictionary<Asset, double>();

                foreach (var access in newAccessList)
                {
                    tasks.Add(access.Asset, access.Task);
                    if (access.AccessStart < newEventStartTime)
                        taskStarts.Add(access.Asset, newEventStartTime);
                    else
                    taskStarts.Add(access.Asset, access.AccessStart);
                    taskEnds.Add(access.Asset, access.AccessEnd);
                    eventStarts.Add(access.Asset, newEventStartTime);
                    eventEnds.Add(access.Asset, newEventStartTime + SchedParameters.SimStepSeconds);

                }
                Event eventToAdd = new Event(tasks, new SystemState(oldSchedule.GetEndState(), newEventStartTime));
                eventToAdd.SetEventEnd(eventEnds);
                eventToAdd.SetTaskEnd(taskEnds);
                eventToAdd.SetEventStart(eventStarts);
                eventToAdd.SetTaskStart(taskStarts);
                AllStates = new StateHistory(oldSchedule.AllStates, eventToAdd);
            }

            
            /* commented out because it doesn't work yet
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
            */
        }

        #endregion

        public bool CanAddTasks(Stack<Access> newAccessList, double newTaskStartTime)
        {
            //TODO: Mehiel double check
            //HSF v2.3:
            //size_t count = 0;
            //vector<assetSchedule*>::iterator asIt2 = assetscheds.begin();
            //for (vector <const Task*>::const_iterator tIt = newTaskList.begin(); tIt != newTaskList.end(); tIt++, asIt2++) {
            //    if (*tIt != NULL)
            //    {
            //        for (vector<assetSchedule*>::iterator asIt = assetscheds.begin(); asIt != assetscheds.end(); asIt++)
            //        {
            //            count += (*asIt)->timesCompletedTask(*tIt);
            //        }
            //        if (count >= (*tIt)->getMaxTimesPerformable())
            //            return false;
            //        if (!(*asIt2)->empty())
            //        {
            //            if ((*asIt2)->getLastState()->getEventEnd() > newTaskStartTime)
            //                return false;
            //        }
            //    }
            //}
            //return true;
            int count = 0;
            // vector<assetSchedule*>::iterator asIt2 = assetscheds.begin();
            //int asIt2 = 0;

	        foreach(var access in newAccessList)
            {
                if (!AllStates.isEmpty(access.Asset)) // the ait2 check
                    if (AllStates.GetLastEvent().GetEventEnd(access.Asset) > newTaskStartTime)
                        return false;
                
		        if(access.Task != null)
                {
				    count += AllStates.timesCompletedTask(access.Task);
			        if(count >= access.Task.MaxTimesToPerform)
				        return false;
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
            //TODO: Mehiel check morgan's work
            //double lasttime = 0;
            //   foreach(var assetSchedule in AssetScheds)
            // if(!assetSchedule.isEmpty())
            //  lasttime = lasttime > assetSchedule.GetLastState().TaskStart ? lasttime : assetSchedule.GetLastState().TaskStart;
            //return lasttime;
            double lasttime = 0;
            foreach (KeyValuePair<Asset, double> assetTaskStarts in AllStates.GetLastEvent().TaskStarts)
            {
                lasttime = lasttime > assetTaskStarts.Value ? lasttime : assetTaskStarts.Value;
            }
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