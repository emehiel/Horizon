using System.Collections.Generic;
using Utilities;
using MissionElements;
using HSFSystem;

namespace HSFScheduler
{
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
        }

        /// <summary>
        /// Creates a new endstate-safe schedule from the given schedule. (last state copied as deep copy, all others shallow copies)
        /// </summary>
        /// <param name="oldSchedule"></param>
        public StateHistory(StateHistory oldSchedule)
        {
            StateHistory newAssetSched = DeepCopy.Copy<StateHistory>(oldSchedule);
            InitialState = newAssetSched.InitialState;
            Events = newAssetSched.Events;
        }

        /// <summary>
        /// Creates a new assetSchedule from and old assetSchedule and a new Event shared pointer
        /// </summary>
        /// <param name="oldSchedule"></param>
        /// <param name="newEvent"></param>
        public StateHistory(StateHistory oldSchedule, Event newEvent)
        {
            StateHistory newAssetSched = DeepCopy.Copy<StateHistory>(oldSchedule);
            InitialState = newAssetSched.InitialState;
            Events = newAssetSched.Events;
            Events.Push(newEvent);
        //    Asset = newAssetSched.Asset;
        }

        /// <summary>
        ///  Returns the last State in the schedule
        /// </summary>
        /// <returns></returns>
        public SystemState GetLastState()
        {
            if (!isEmpty()) //TODO: check this is what we actually want to do
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
                return Events.Peek().getAssetTask(asset);
            }
            else return null;
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
        /// Returns whether the schedule is empty
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