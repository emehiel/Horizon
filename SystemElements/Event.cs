using System;
using System.Collections.Generic;
using Utilities;
using MissionElements;

namespace MissionElements
{
    [Serializable]
    public class Event
    {
        /** The task that are to be performed by each asset. */
        public Dictionary<Asset, Task> Tasks { get; private set; }
        /** The time history of the State during the current Event. */
        public SystemState State { get; private set; }
      //  public IEnumerable<KeyValuePair<Asset, double>> TaskStartTimes { get; set; }

        /** The start of the event associated with this State */
        public Dictionary<Asset, double> EventStarts { get; private set; }

        /** The start of the task associated with this State */
        public Dictionary<Asset, double> TaskStarts { get; set; }

        /** The end of the task associated with this State */
        public Dictionary<Asset, double> TaskEnds { get; set; }

        /** The end of the event associated with this State */
        public Dictionary<Asset, double> EventEnds { get; set; }

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
        }
        /// <summary>
        /// New Event with a deep copy of the state.
        /// </summary>
        /// <param name="eventToCopyExactly"></param>
        public Event(Event eventToCopyExactly)
        {
            Tasks = eventToCopyExactly.Tasks;
            State = eventToCopyExactly.State.DeepClone();
            EventStarts = eventToCopyExactly.EventStarts;
            EventEnds = eventToCopyExactly.EventEnds;
            TaskStarts = eventToCopyExactly.TaskStarts;
            TaskEnds = eventToCopyExactly.TaskEnds;
        }

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
    }
}
