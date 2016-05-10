using System;
using System.Collections.Generic;
using Utilities;
using MissionElements;

namespace MissionElements
{
    public class Event
    {
        /** The task that are to be performed by each asset. */
        public Dictionary<Asset, Task> Tasks { get; private set; }
     //   public Access ;
        /** The time history of the State during the current Event. */
        public SystemState State { get; private set; }
        public IEnumerable<KeyValuePair<Asset, double>> TaskStartTimes { get; set; }

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
        }

        public double GetTaskStart(Asset asset)
        {
            throw new NotImplementedException();
        }

        public Event(Event eventToCopyExactly)
        {
            Event newEvent = DeepCopy.Copy<Event>(eventToCopyExactly);
            Tasks = newEvent.Tasks;
            State = newEvent.State;
        }

        public Task getAssetTask(Asset asset)
        {
            Task currentTask;
            Tasks.TryGetValue(asset, out currentTask);
            return currentTask;
        }

        public double GetEventStart(Asset asset)
        {
            throw new NotImplementedException();
        }

        public double GetEventEnd(Asset asset)
        {
            throw new NotImplementedException();
        }

        public void SetEventEnd(Asset asset, double evalToTime)
        {
            throw new NotImplementedException();
        }

        public double GetTaskEnd(Asset asset)
        {
            throw new NotImplementedException();
        }

        public void SetTaskEnd(Asset asset, double te)
        {
            throw new NotImplementedException();
        }
    }
}
