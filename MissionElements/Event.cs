// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using Utilities;
using MissionElements;

namespace MissionElements
{
    [Serializable]
    public class Event
    {
        #region Attributes
        /// <summary>
        /// The task that are to be performed by each asset. */
        /// </summary>
        public Dictionary<Asset, Task> Tasks { get; private set; }

        /// <summary>
        /// The time history of the State during the current Event. 
        /// </summary>
        public SystemState State { get; private set; }

        /// <summary>
        /// The start of the event associated with this State 
        /// </summary>
        public Dictionary<Asset, double> EventStarts { get; private set; }

        /// <summary>
        /// The start of the task associated with this State 
        /// </summary>
        public Dictionary<Asset, double> TaskStarts { get; set; }

        /// <summary>
        ///The end of the task associated with this State 
        /// </summary>
        public Dictionary<Asset, double> TaskEnds { get; set; }

        /// <summary>
        /// The end of the event associated with this State 
        /// </summary>
        public Dictionary<Asset, double> EventEnds { get; set; }

        public int isEvaluated;
        #endregion

        #region Constructors
        /// <summary>
        ///  Creates an Event, in which the Task was performed by an Asset, and the time history 
        /// of the pertinent State information was saved.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="state"></param>
        public Event(Dictionary<Asset, Task> task, SystemState state)
        {
            Tasks = task;
            State = state; //Should this be a deep copy?
            EventStarts = new Dictionary<Asset, double>();
            EventEnds = new Dictionary<Asset, double>();
            TaskStarts = new Dictionary<Asset, double>();
            TaskEnds = new Dictionary<Asset, double>();
            isEvaluated = 0;
        }
        /// <summary>
        /// New Event with a deep copy of the state.
        /// </summary>
        /// <param name="eventToCopyExactly"></param>
        public Event(Event eventToCopyExactly)
        {
            Tasks = eventToCopyExactly.Tasks;
            State = DeepCopy.Copy(eventToCopyExactly.State);
            EventStarts = eventToCopyExactly.EventStarts;
            EventEnds = eventToCopyExactly.EventEnds;
            //EventStarts = DeepCopy.Copy(eventToCopyExactly.EventStarts);
            //EventEnds = DeepCopy.Copy(eventToCopyExactly.EventEnds);
            TaskStarts = DeepCopy.Copy(eventToCopyExactly.TaskStarts);
            TaskEnds = DeepCopy.Copy(eventToCopyExactly.TaskEnds);
        }
        #endregion

        #region Accessors
        /// <summary>
        /// Get the current task assigned to the asset
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public Task GetAssetTask(Asset asset)
        {
            Task currentTask;
            Tasks.TryGetValue(asset, out currentTask);
            return currentTask;
        }

        public double GetTaskStart(Asset asset)
        {
            double time;
            TaskStarts.TryGetValue(asset, out time);
            return time;
        }

        public double GetEventStart(Asset asset)
        {
            double time;
                
            EventStarts.TryGetValue(asset, out time);
            return time;
        }

        public double GetTaskEnd(Asset asset)
        {
            double time;
            TaskEnds.TryGetValue(asset, out time);
            return time;
        }

        public double GetEventEnd(Asset asset)
        {
            double time;
            EventEnds.TryGetValue(asset, out time);
            return time;
        }

        public void SetEventEnd(Asset asset, double te)
        {
            if (EventEnds.ContainsKey(asset))
                EventEnds.Remove(asset);
            EventEnds.Add(asset, te);
        }

        public void SetTaskEnd(Asset asset, double te)
        {
            if (TaskEnds.ContainsKey(asset))
                TaskEnds.Remove(asset);
            TaskEnds.Add(asset, te);
        }

        public void SetEventStart(Asset asset, double te)
        {
            if (EventEnds.ContainsKey(asset))
                EventEnds.Remove(asset);
            EventEnds.Add(asset, te);
        }

        public void SetTaskStart(Asset asset, double te)
        {
            if (TaskStarts.ContainsKey(asset))
                TaskStarts.Remove(asset);
            TaskStarts.Add(asset, te);
        }

        public void SetEventStart(Dictionary<Asset, double> eventStarts)
        {
            EventStarts = eventStarts;
        }

        public void SetTaskStart(Dictionary<Asset, double> taskStarts)
        {
            TaskStarts = taskStarts;
        }

        public void SetEventEnd(Dictionary<Asset, double> eventEnds)
        {
            EventEnds = eventEnds;
        }

        public void SetTaskEnd(Dictionary<Asset, double> taskEnds)
        {
            TaskEnds = taskEnds;
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Override of the Object ToString method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string eventString = "";
            foreach(var assetTask in Tasks)
            {
                eventString += assetTask.Key.Name + ":\t" + assetTask.Value.Target.ToString()+ "\t";
                eventString += "Task Start:\t" + GetTaskStart(assetTask.Key) + "\tEvent Start:\t" + GetEventStart(assetTask.Key) + "\t"; 
                eventString+= "Task End:\t" + GetTaskEnd(assetTask.Key) + "\tEvent End:\t" + GetEventEnd(assetTask.Key);
            }

            return eventString;
        }
        #endregion
    }
}
