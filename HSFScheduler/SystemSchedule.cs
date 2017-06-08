// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Utilities;
using MissionElements;
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

        public SystemSchedule(StateHistory allStates)
        {
            AllStates = new StateHistory(allStates);
            foreach (var eit in allStates.Events)
            {
                AllStates.Events.Push(new Event(eit));
            }
            //AllStates.Events = allStates.Events;
        }

        public SystemSchedule(SystemSchedule oldSchedule, Event emptyEvent)
        {
            AllStates = new StateHistory(oldSchedule.AllStates);
            AllStates.Events.Push(emptyEvent);
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
                if (access.Task != null)
                {
                    if (access.AccessStart <= newEventStartTime && newEventStartTime <= access.AccessEnd)
                        taskStarts.Add(access.Asset, newEventStartTime);
                    else if (access.AccessStart >= newEventStartTime && access.AccessStart <= newEventStartTime + SchedParameters.SimStepSeconds)
                        taskStarts.Add(access.Asset, access.AccessStart);
                    else
                    {
                        //Console.WriteLine("Event Start: " + newEventStartTime + " AccesStart: " + access.AccessStart + " AccessEnd: " + access.AccessEnd);
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
                else
                {
                    taskStarts.Add(access.Asset, newEventStartTime);
                    taskEnds.Add(access.Asset, newEventStartTime);
                    tasks.Add(access.Asset, null);
                    eventStarts.Add(access.Asset, newEventStartTime);
                    eventEnds.Add(access.Asset, newEventStartTime + SchedParameters.SimStepSeconds);
                }

            }
            Event eventToAdd = new Event(tasks, new SystemState(oldStates.GetLastState())); //all references
            eventToAdd.SetEventEnd(eventEnds);
            eventToAdd.SetTaskEnd(taskEnds);
            eventToAdd.SetEventStart(eventStarts);
            eventToAdd.SetTaskStart(taskStarts);
            AllStates = new StateHistory(oldStates, eventToAdd);
        }
        #endregion
        
        /// <summary>
        /// Determine if a task can be added to a schedule at the new start time
        /// </summary>
        /// <param name="newAccessList"></param>
        /// <param name="newTaskStartTime"></param>
        /// <returns></returns>
        public bool CanAddTasks(Stack<Access> newAccessList, double newTaskStartTime)
        {
            int count = 0;

	        foreach(var access in newAccessList)
            {
                if (!AllStates.isEmpty(access.Asset))
                {
                    if (AllStates.GetLastEvent().GetEventEnd(access.Asset) > newTaskStartTime)
                        return false;
                }

		        if(access.Task != null)
                {
				    count += AllStates.timesCompletedTask(access.Task);
			        if(count >= access.Task.MaxTimesToPerform)
				        return false;
		        }
	        }
	        return true;
        }

        #region Accessors
        public int GetTotalNumEvents()
        {
            return AllStates.size();
        }

        public SystemState GetSubsystemNewState()
        {
            return AllStates.GetLastState();
        }

        public Task GetSubsytemNewTask(Asset asset)
        {
            return AllStates.GetLastTask(asset);
        }

        //public StateHistory GetStateHistory(Asset asset)
        //{
        //    return AllStates.Find(item => item.Asset == asset);
        //}

        public double GetLastTaskStart()
        {
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
        #endregion 

        /// <summary>
        /// Determine if the first schedule value is greater than the second
        /// </summary>
        /// <param name="elem1"></param>
        /// <param name="elem2"></param>
        /// <returns></returns>
        bool SchedGreater(SystemSchedule elem1, SystemSchedule elem2)
        {
            return elem1.ScheduleValue > elem2.ScheduleValue;
        }

        /// <summary>
        /// Utilitiy method to write the schedule to csv file
        /// </summary>
        /// <param name="schedule"></param>
        /// <param name="scheduleWritePath"></param>
        public static void WriteSchedule(SystemSchedule schedule, String scheduleWritePath)
        {
            var csv = new StringBuilder();
            Dictionary<StateVarKey<double>, SortedList<double, double>> stateTimeDData = new Dictionary<StateVarKey<double>, SortedList<double, double>>();
            Dictionary<StateVarKey<int>, SortedList<double, int>> stateTimeIData = new Dictionary<StateVarKey<int>, SortedList<double, int>>();
            Dictionary<StateVarKey<int>, SortedList<double, int>> stateTimeBData = new Dictionary<StateVarKey<int>, SortedList<double, int>>(); // need 0s and 1 for matlab to read in csv
            Dictionary<StateVarKey<Matrix<double>>, SortedList<double, Matrix<double>>> stateTimeMData = new Dictionary<StateVarKey<Matrix<double>>, SortedList<double, Matrix<double>>>();
            Dictionary<StateVarKey<Quat>, SortedList<double, Quat>> stateTimeQData = new Dictionary<StateVarKey<Quat>, SortedList<double, Quat>>();
            string stateTimeData = "Time,";
            string stateData = "";
            csv.Clear();
            SystemState sysState = schedule.AllStates.Events.Peek().State;

            while(sysState != null) { 
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
                foreach (var kvpQuatProfile in sysState.Qdata)
                    foreach (var data in kvpQuatProfile.Value.Data)
                        if (!stateTimeQData.ContainsKey(kvpQuatProfile.Key))
                        {
                            var lt = new SortedList<double, Quat>();
                            lt.Add(data.Key, data.Value);
                            stateTimeQData.Add(kvpQuatProfile.Key, lt);
                        }
                        else if (!stateTimeQData[kvpQuatProfile.Key].ContainsKey(data.Key))
                            stateTimeQData[kvpQuatProfile.Key].Add(data.Key, data.Value);
                sysState = sysState.Previous;
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
            foreach (var list in stateTimeQData)
                writeStateVariable(list, scheduleWritePath);
        }
        
        /// <summary>
        /// Write out all the state variables in the schedule to file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="scheduleWritePath"></param>
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