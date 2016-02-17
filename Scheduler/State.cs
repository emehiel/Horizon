using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace HSFScheduler
{
    public class State
    {
        /** The previous state, upon which this state is based */
        public State _previous { get; private set; }

        /** The start of the event associated with this State */
        public double eventStart;

        /** The start of the task associated with this State */
        public double taskStart;

        /** The end of the task associated with this State */
        public double taskEnd;

        /** The end of the event associated with this State */
        public double eventEnd;

        /** The Dictionary of integer Profiles. */
        Dictionary<StateVarKey<int>, HSFProfile<int>> idata;

        /** The Dictionary of double precision Profiles. */
        Dictionary<StateVarKey<double>, HSFProfile<double>> ddata;

        /** The Dictionary of floating point value Profiles. */
        Dictionary<StateVarKey<float>, HSFProfile<float>> fdata;

        /** The Dictionary of boolean Profiles. */
        Dictionary<StateVarKey<bool>, HSFProfile<bool>> bdata;

        /** The Dictionary of Matrix Profiles. */
        Dictionary<StateVarKey<Matrix>, HSFProfile<Matrix>> mdata;

        /** The Dictionary of Quaternion Profiles. */
        Dictionary<StateVarKey<Quat>, HSFProfile<Quat>> qdata;


        /**
         * Creates an initial State
         */
        State()
        {

        }

        /**
         * Copy constructor for exact state copies
         */
        State(State initialStateToCopy)
        {

        }

        /**
         * Creates a new State based on a previous State and a new Task start time
         */
        State(State previous, double newTaskStart)
        {

        }


        //took out setters and getters becuase they're public fields

        /** TODO: figure out if this can all be done with dictionary stuff
         * Gets the last int value set for the given state variable key in the state. If no value is found
         * it checks the previous state, continuing all the way to the initial state.
         * @param key The integer state variable key that is being looked up.
         * @return A pair containing the last time the variable was set, and the integer value.
         */
        pair<double, int> getLastValue(const StateVarKey<int>& key) const {
		    if(!idata.empty()) { // Are there any Profiles in there?
			    map<StateVarKey<int>, Profile<int>>::const_iterator mIt = idata.find(key); // Find the Profile matching the key.
			    if(!(mIt == idata.end())) // Did you find a Profile with my key?
				    return (*mIt).second.getLastPair(); // We have the right Profile, find that value!
		    }
		    return (* previous).getLastValue(key); // This isn't the right profile, go back one and try it out!
        }
    }
}
