using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace HSFScheduler
{
    public class Event
    {
        /** The task that is to be performed. */
        public Task Task { get; private set; }

        /** The time history of the Asset State during the current Event. */
        public SystemState State { get; private set; }

        /**
	     * Creates an Event, in which the Task was performed by an Asset, and the time history 
	     * of the pertinent State information was saved.
	     * @param task The Task that was performed.
	     * @param state The time history of the Asset State during the Event.
	     */
        public Event(Task task, SystemState state)
        {
            Task = task;
            State = state;
        }

    	public Event(Event eventToCopyExactly)
        {
            Event newEvent = DeepCopy.Copy<Event>(eventToCopyExactly);
            Task = newEvent.Task;
            State = newEvent.State;
        }
    }
}
