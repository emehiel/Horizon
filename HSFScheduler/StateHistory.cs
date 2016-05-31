using System.Collections.Generic;
using System;
using Utilities;
using MissionElements;
using HSFSystem;

namespace HSFScheduler
{
    [Serializable]
    public class StateHistory
    {
        public SystemState InitialState { get; private set; }
        public Stack<Event> Events { get; private set; }
        //   public Asset Asset { get; private set; }

        /// <summary>
        ///  Creates a new empty schedule with the given initial state.
        /// </summary>
        /// <param name="initialState"></param>
        public StateHistory(SystemState initialState)
        {
            InitialState = initialState;
            Events = new Stack<Event>();
        }

        /// <summary>
        /// Creates a new endstate-safe schedule from the given schedule. (last state copied as deep copy, all others shallow copies)
        /// </summary>
        /// <param name="oldSchedule"></param>
        public StateHistory(StateHistory oldSchedule)
        {
            InitialState = oldSchedule.InitialState;
            Events = new Stack<Event>(oldSchedule.Events);
            //if (Events.Count != 0)
            //{
            //    Events.Pop();
            //    Events.Push(new Event(oldSchedule.Events.Peek()));
            //}
        }

        /// <summary>
        /// Creates a new assetSchedule from and old assetSchedule and a new Event
        /// </summary>
        /// <param name="oldSchedule"></param>
        /// <param name="newEvent"></param>
        public StateHistory(StateHistory oldSchedule, Event newEvent)
        {
            Events = new Stack<Event>(oldSchedule.Events);
            InitialState = oldSchedule.InitialState;  //Should maybe be a deep copy -->no
            Events.Push(newEvent);
        //    Asset = newAssetSched.Asset;
        }

        /// <summary>
        ///  Returns the last State in the schedule
        /// </summary>
        /// <returns></returns>
        public SystemState GetLastState()
        {
            if (!isEmpty()) 
            {
                return Events.Peek().State;
            }
            else
                return InitialState;
        }

        /// <summary>
        /// returns the last task in the schedule for a specific asset
        /// </summary>
        /// <returns></returns>
        public Task GetLastTask(Asset asset)
        {
            if (isEmpty() == false) //TODO: check that this is actually what we want to do.
            {
                return Events.Peek().GetAssetTask(asset);
            }
            else return null;
        }

        /// <summary>
        /// Return all the last tasks of all the assets in a dictionary of Asset, Task
        /// </summary>
        /// <returns></returns>
        public Dictionary<Asset, Task> GetLastTasks()
        {
            return GetLastEvent().Tasks;
        }

        /// <summary>
        /// Return the last event (last task of each asset in the system and the system state)
        /// </summary>
        /// <returns></returns>
        public Event GetLastEvent()
        {
            return Events.Peek();
        }

        /// <summary>
        /// Returns the number of times the specified task has been completed in this schedule for a specific asset
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public int timesCompletedTask(Asset asset, Task task)
        {
            int count = 0;
            KeyValuePair<Asset, Task> search = new KeyValuePair<Asset, Task>(asset, task);
            foreach(Event eit in Events)
            {
                foreach(KeyValuePair<Asset, Task> pair in eit.Tasks)
                {
                    if (pair.Equals(search))
                        count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Returns the number of times the specified task has been completed in this schedule for a specific asset
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public int timesCompletedTask(Task task)
        {
            int count = 0;
            foreach (Event eit in Events)
            {
               if (eit.Tasks.ContainsValue(task))
                    count++;
            }
            return count;
        }

        /// <summary>
        ///  Returns the number of events in the schedule for ALL assets
        /// </summary>
        /// <returns></returns>
        public int size()
        {
            return Events.Count;
        }

        /// <summary>
        /// Returns the number of events in the schedule for a specific asset
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public int size(Asset asset)
        {
            int count = 0;
            foreach (Event eit in Events)
            {
                if (eit.Tasks.ContainsKey(asset))
                    count++;
            }
            return count;
        }
        /// <summary>
        /// Returns true if the specified asset doesn't have a task (the asset isn't scheduled)
        /// </summary>
        /// <returns></returns>
        public bool isEmpty(Asset asset)
        {
            foreach(Event eit in Events)
            {
                if (eit.Tasks.ContainsKey(asset)) //something has been scheduled
                    return false;
            }
            return true;
        }

        /// <summary>
        /// returns true is no assets have any events
        /// </summary>
        /// <returns></returns>
        public bool isEmpty()
        {
            if (Events.Count == 0)
                return true;
            return false;
        }
    }
}