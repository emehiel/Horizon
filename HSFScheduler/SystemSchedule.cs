using System;
using System.Collections.Generic;
using System.Text;
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

        /// <summary>
        /// Copy constructor to preserve the without adding a task
        /// </summary>
        /// <param name="oldSchedule"></param>
        public SystemSchedule(SystemSchedule oldSchedule)
        {
             AllStates = new StateHistory(oldSchedule.AllStates);
           // AllStates = oldSchedule.AllStates;
        }

        public SystemSchedule(StateHistory allStates)
        {
            AllStates = allStates; 
        }

        public SystemSchedule(StateHistory oldStates, Stack<Access> newAccessList, double newEventStartTime)
        {
            Dictionary<Asset, Task> tasks = new Dictionary<Asset, Task>();
            Dictionary<Asset, double> taskStarts = new Dictionary<Asset, double>();
            Dictionary<Asset, double> taskEnds = new Dictionary<Asset, double>();
            Dictionary<Asset, double> eventStarts = new Dictionary<Asset, double>();
            Dictionary<Asset, double> eventEnds = new Dictionary<Asset, double>();

            foreach (var access in newAccessList)
            {

                if (access.AccessStart <= newEventStartTime && newEventStartTime <= access.AccessEnd)
                    taskStarts.Add(access.Asset, newEventStartTime);
                else if (access.AccessStart >= newEventStartTime && access.AccessStart <= newEventStartTime + SchedParameters.SimStepSeconds)
                    taskStarts.Add(access.Asset, access.AccessStart);
                else
                {
                    Console.WriteLine("Event Start: " + newEventStartTime + " AccesStart: " + access.AccessStart +" AccessEnd: " + access.AccessEnd);
                    taskStarts.Add(access.Asset, newEventStartTime);
                }
                tasks.Add(access.Asset, access.Task);
                if (access.AccessEnd > SimParameters.SimEndSeconds)
                    taskEnds.Add(access.Asset, SimParameters.SimEndSeconds);
                else
                    taskEnds.Add(access.Asset, access.AccessEnd);
                eventStarts.Add(access.Asset, newEventStartTime);
                if (newEventStartTime + SchedParameters.SimStepSeconds > SimParameters.SimEndSeconds)
                    eventEnds.Add(access.Asset, SchedParameters.SimStepSeconds);
                else
                    eventEnds.Add(access.Asset, newEventStartTime + SchedParameters.SimStepSeconds);
            }
            Event eventToAdd = new Event(tasks, new SystemState(oldStates.GetLastState())); //all references
            eventToAdd.SetEventEnd(eventEnds);
            eventToAdd.SetTaskEnd(taskEnds);
            eventToAdd.SetEventStart(eventStarts);
            eventToAdd.SetTaskStart(taskStarts);
            AllStates = new StateHistory(oldStates, eventToAdd);
            
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
                if (!AllStates.isEmpty(access.Asset))
                { // the ait2 check
                    if (AllStates.GetLastEvent().GetEventEnd(access.Asset) > newTaskStartTime)
                        return false;
                }

		        if(access.Task != null)
                {
				    count += AllStates.timesCompletedTask(access.Task);
			        if(count >= 1)// access.Task.MaxTimesToPerform)
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

        public static void WriteSchedule(SystemSchedule schedule, String scheduleWritePath)
        {
            var csv = new StringBuilder();
            Dictionary<StateVarKey<double>, SortedList<double, double>> stateTimeDData = new Dictionary<StateVarKey<double>, SortedList<double, double>>();
            Dictionary<StateVarKey<int>, SortedList<double, int>> stateTimeIData = new Dictionary<StateVarKey<int>, SortedList<double, int>>();
            Dictionary<StateVarKey<int>, SortedList<double, int>> stateTimeBData = new Dictionary<StateVarKey<int>, SortedList<double, int>>(); // need 0s and 1 for matlab to read in csv
            Dictionary<StateVarKey<Matrix<double>>, SortedList<double, Matrix<double>>> stateTimeMData = new Dictionary<StateVarKey<Matrix<double>>, SortedList<double, Matrix<double>>>();

            string stateTimeData = "Time,";
            string stateData = "";
            csv.Clear();


            

            while (schedule.AllStates.Events.Count > 0)
            {
                var currEvent = schedule.AllStates.Events.Pop();
                SystemState sysState = currEvent.State;
                foreach (var kvpDoubleProfile in sysState.Ddata)
                    foreach (var data in kvpDoubleProfile.Value.Data)
                        if (!stateTimeDData.ContainsKey(kvpDoubleProfile.Key))
                        {
                            var lt = new SortedList<double, double>();
                            lt.Add(data.Key, data.Value);
                            stateTimeDData.Add(kvpDoubleProfile.Key, lt);
                        }
                        else if (!stateTimeDData[kvpDoubleProfile.Key].ContainsKey(data.Key))
                            stateTimeDData[kvpDoubleProfile.Key].Add(data.Key, data.Value);
                        else
                            Console.WriteLine("idk");

                foreach (var kvpIntProfile in sysState.Idata)
                    foreach (var data in kvpIntProfile.Value.Data)
                        if (!stateTimeIData.ContainsKey(kvpIntProfile.Key))
                        {
                            var lt = new SortedList<double, int>();
                            lt.Add(data.Key, data.Value);
                            stateTimeIData.Add(kvpIntProfile.Key, lt);
                        }
                        else if (!stateTimeIData[kvpIntProfile.Key].ContainsKey(data.Key))
                            stateTimeIData[kvpIntProfile.Key].Add(data.Key, data.Value);

                foreach (var kvpBoolProfile in sysState.Bdata)
                    foreach (var data in kvpBoolProfile.Value.Data)
                        if (!stateTimeBData.ContainsKey(kvpBoolProfile.Key))
                        {
                            var lt = new SortedList<double, int>();
                            lt.Add(data.Key, (data.Value ? 1 : 0)); //convert to int for matlab to read in for csv
                            stateTimeBData.Add((StateVarKey<int>)kvpBoolProfile.Key, lt);
                        }
                        else if (!stateTimeBData[kvpBoolProfile.Key].ContainsKey(data.Key))
                            stateTimeBData[(StateVarKey<int>)kvpBoolProfile.Key].Add(data.Key, data.Value ? 1 : 0);

                foreach (var kvpMatrixProfile in sysState.Mdata)
                    foreach (var data in kvpMatrixProfile.Value.Data)
                        if (!stateTimeMData.ContainsKey(kvpMatrixProfile.Key))
                        {
                            var lt = new SortedList<double, Matrix<double>>();
                            lt.Add(data.Key, data.Value);
                            stateTimeMData.Add(kvpMatrixProfile.Key, lt);
                        }
                        else if (!stateTimeMData[kvpMatrixProfile.Key].ContainsKey(data.Key))
                            stateTimeMData[kvpMatrixProfile.Key].Add(data.Key, data.Value);




            }

            System.IO.Directory.CreateDirectory(scheduleWritePath);

            foreach(var list in stateTimeDData)
                writeStateVariable(list, scheduleWritePath);

            foreach(var list in stateTimeIData)
                writeStateVariable(list, scheduleWritePath);

            foreach (var list in stateTimeBData)
                writeStateVariable(list, scheduleWritePath);

            foreach (var list in stateTimeMData)
                writeStateVariable(list, scheduleWritePath);

        }
        
        static void writeStateVariable<T>(KeyValuePair<StateVarKey<T>, SortedList<double, T>> list, string scheduleWritePath)
        {
            var csv = new StringBuilder();
            string fileName = list.Key.VarName;

            string invalidChars = "";

            foreach (char c in System.IO.Path.GetInvalidPathChars())
                invalidChars += c;

            invalidChars += "(" + ")" + "/" + ".";

            foreach (char c in invalidChars)
                fileName = fileName.Replace(c, '_');

            csv.AppendLine("time" + "," + fileName);
            foreach (var k in list.Value)
                csv.AppendLine(k.Key + "," + k.Value);

            System.IO.File.WriteAllText(scheduleWritePath + "\\" + fileName + ".csv", csv.ToString());
            csv.Clear();
        }
    }
}