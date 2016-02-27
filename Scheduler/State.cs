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
        public State Previous { get; private set; }

        /** The start of the event associated with this State */
        public double EventStart{get; set;}

        /** The start of the task associated with this State */
        public double TaskStart { get; set; }

        /** The end of the task associated with this State */
        public double TaskEnd { get; set; }

        /** The end of the event associated with this State */
        public double EventEnd { get; set; }

        /** The Dictionary of integer Profiles. */
        public Dictionary<StateVarKey<int>, HSFProfile<int>> Idata { get; private set; }

        /** The Dictionary of double precision Profiles. */
        public Dictionary<StateVarKey<double>, HSFProfile<double>> Ddata { get; private set; }

        /** The Dictionary of floating point value Profiles. */
        public Dictionary<StateVarKey<float>, HSFProfile<float>> Fdata { get; private set; }

        /** The Dictionary of boolean Profiles. */
        public Dictionary<StateVarKey<bool>, HSFProfile<bool>> Bdata { get; private set; }

        /** The Dictionary of Matrix Profiles. */
        public Dictionary<StateVarKey<Matrix>, HSFProfile<Matrix>> Mdata { get; private set; }

        /** The Dictionary of Quaternion Profiles. */
        public Dictionary<StateVarKey<Quat>, HSFProfile<Quat>> Qdata { get; private set; }


        /**
         * Creates an initial State
         */
        State()
        {
            Previous = null;
            EventStart = 0;
            EventEnd = 0;
            TaskEnd = 0;
            TaskStart = 0;
        }

        /**
         * Copy constructor for exact state copies
         */
        State(State initialStateToCopy)
        {
            Previous = initialStateToCopy.Previous;
            EventStart = initialStateToCopy.EventStart;
            TaskStart = initialStateToCopy.TaskStart;
            TaskEnd = initialStateToCopy.TaskEnd;
            EventEnd = initialStateToCopy.EventEnd;
            Idata = initialStateToCopy.Idata;
            Ddata = initialStateToCopy.Ddata;
            Fdata = initialStateToCopy.Fdata;
            Bdata = initialStateToCopy.Bdata;
            Mdata = initialStateToCopy.Mdata;
            Qdata = initialStateToCopy.Qdata;
        }

        /**
         * Creates a new State based on a previous State and a new Task start time
         */
        State(State previous, double newTaskStart)
        {
            Previous = previous;
            EventStart = previous.EventEnd; // start from end of previous State
            TaskStart = newTaskStart;
            TaskEnd = newTaskStart;
            EventEnd = newTaskStart;
        }


        //took out setters and getters becuase they're public fields

        /** TODO: figure out if this can all be done with dictionary stuff
         * Gets the last int value set for the given state variable key in the state. If no value is found
         * it checks the previous state, continuing all the way to the initial state.
         * @param key The integer state variable key that is being looked up.
         * @return A pair containing the last time the variable was set, and the integer value.
         */
         KeyValuePair<double, int> getLastValue(StateVarKey<int> key) {
            HSFProfile<int> valueOut;
		    if(Idata.Count != 0) { // Are there any Profiles in there?
                if (Idata.TryGetValue(key, out valueOut)) //see if our key is in there
                    return valueOut.Last(); //found it, return it TODO: return last value or pair?
		    }
            return Previous.getLastValue(key); //either no profiles or none that match our keys, try finding it in the previous one
        }
        /** 
         * Gets the integer value of the state at a certain time. If the exact time is not found, the data is
         * assumed to be on a zero-order hold, and the last value set is found.
         * @param key The integer state variable key that is being looked up.
         * @param time The time the value is looked up at.
         * @return A pair containing the last time the variable was set, and the integer value.
         */
        KeyValuePair<double, int> getValueAtTime(StateVarKey<int> key, double time){
            HSFProfile<int> valueOut;
            if (Idata.Count != 0)  { // Are there any Profiles in there?
                if (Idata.TryGetValue(key, out valueOut)) //see if our key is in there
                    return valueOut.DataAtTime(time);
            }
		    return  Previous.getValueAtTime(key, time); //either no profiles or none that match our keys, try finding it in the previous one
        }

        /** 
         * Returns the integer Profile matching the key given. If no Profile is found, it goes back one Event
         * and checks again until it gets to the initial state.
         * @param key The integer state variable key that is being looked up.
         * @return The Profile saved in the state.
         */
        HSFProfile<int> getProfile(StateVarKey<int> key){
            HSFProfile<int> valueOut;
		    if(Idata.Count != 0)  { // Are there any Profiles in there?
                if (Idata.TryGetValue(key, out valueOut)) //see if our key is in there
                    return valueOut;
            }
		    return Previous.getProfile(key); // This isn't the right profile, go back one and try it out!
	    }

        /** TODO: make sure valueOut is a good replacement for iterator.second
         * Returns the integer Profile for this state and all previous states merged into one Profile
         * @param key The integer state variable key that is being looked up.
         * @return The full Profile
         */
        HSFProfile<int> getFullProfile(StateVarKey<int> key){
            HSFProfile<int> valueOut = new HSFProfile<int>();
		    if(Idata.Count != 0)  { // Are there any Profiles in there?
                if (Idata.TryGetValue(key, out valueOut)) { //see if our key is in there
                    if (Previous != null) // Check whether we are at the first state
					    return HSFProfile<int>.MergeProfiles(valueOut, Previous.getFullProfile(key));
				    return valueOut;				
			    }
            }
		    if(Previous != null)
			    return Previous.getFullProfile(key); // If no data, return profile from previous states
		    return valueOut; //return empty profile
	   }

        /** 
         * Sets the integer Profile in the state with its matching key. If a Profile is found already under
         * that key, this will overwrite the old Profile.
         * @param key The integer state variable key that is being set.\
         * @param profIn The integer Profile being saved.
         */
        void setProfile(StateVarKey<int> key, HSFProfile<int> profIn){
            HSFProfile<int> valueOut;
            if (!Idata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Idata.Add(key, profIn); 
		    else { // Otherwise, erase whatever is there, and insert a new one.
			    Idata.Remove(key);
			    Idata.Add(key, profIn); 
		    }
        }

        /** 
	     * Adds a integer Profile value pair to the state with the given key. If no Profile exists, a new Profile is created
	     * with the corresponding key. If a Profile exists with that key, the pair is appended onto the end of the Profile. 
	     * Ensure that the Profile is still time ordered if this is the case.
	     * @param key The key corresponding to the state variable.
	     * @param pairIn The pair to be added to the integer Profile.
	     */
        void addValue(StateVarKey<int> key, KeyValuePair<double, int> pairIn){
            HSFProfile<int> valueIn = new HSFProfile<int>(pairIn);
            HSFProfile<int> valueOut;
            if (!Idata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Idata.Add(key, valueIn); 
		    else // Otherwise, add this data point to the existing Profile.
			    valueOut.Add(pairIn); //TODO: make sure this is ok. was formally iterator.second.data
	    }

        /** 
         * Adds a integer Profile to the state with the given key. If no Profile exists, a new Profile is created
         * with the corresponding key. If a Profile exists with that key, the Profile is appended onto the end of the Profile. 
         * @param key The key corresponding to the state variable.
         * @param profIn The Profile to be added to the integer Profile.
         */
        void addValue(StateVarKey<int> key, HSFProfile<int> profIn){
		    HSFProfile<int> valueOut;
            if (!Idata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
			    Idata.Add(key, profIn); 
		    else // Otherwise, add this data point to the existing Profile.
			    valueOut.Add(profIn);
	    }
               /** TODO: figure out if this can all be done with dictionary stuff
         * Gets the last int value set for the given state variable key in the state. If no value is found
         * it checks the previous state, continuing all the way to the initial state.
         * @param key The integer state variable key that is being looked up.
         * @return A pair containing the last time the variable was set, and the integer value.
         */
         KeyValuePair<double, double> getLastValue(StateVarKey<double> key) {
            HSFProfile<double> valueOut;
		    if(Ddata.Count != 0) { // Are there any Profiles in there?
                if (Ddata.TryGetValue(key, out valueOut)) //see if our key is in there
                    return valueOut.Last(); //found it, return it TODO: return last value or pair?
		    }
            return Previous.getLastValue(key); //either no profiles or none that match our keys, try finding it in the previous one
        }
        /** 
         * Gets the integer value of the state at a certain time. If the exact time is not found, the data is
         * assumed to be on a zero-order hold, and the last value set is found.
         * @param key The integer state variable key that is being looked up.
         * @param time The time the value is looked up at.
         * @return A pair containing the last time the variable was set, and the integer value.
         */
        KeyValuePair<double, double> getValueAtTime(StateVarKey<double> key, double time){
            HSFProfile<double> valueOut;
            if (Ddata.Count != 0)  { // Are there any Profiles in there?
                if (Ddata.TryGetValue(key, out valueOut)) //see if our key is in there
                    return valueOut.DataAtTime(time);
            }
		    return  Previous.getValueAtTime(key, time); //either no profiles or none that match our keys, try finding it in the previous one
        }

        /** 
         * Returns the integer Profile matching the key given. If no Profile is found, it goes back one Event
         * and checks again until it gets to the initial state.
         * @param key The integer state variable key that is being looked up.
         * @return The Profile saved in the state.
         */
        HSFProfile<double> getProfile(StateVarKey<double> key){
            HSFProfile<double> valueOut;
		    if(Ddata.Count != 0)  { // Are there any Profiles in there?
                if (Ddata.TryGetValue(key, out valueOut)) //see if our key is in there
                    return valueOut;
            }
		    return Previous.getProfile(key); // This isn't the right profile, go back one and try it out!
	    }

        /** TODO: make sure valueOut is a good replacement for iterator.second
         * Returns the integer Profile for this state and all previous states merged into one Profile
         * @param key The integer state variable key that is being looked up.
         * @return The full Profile
         */
        HSFProfile<double> getFullProfile(StateVarKey<double> key){
            HSFProfile<double> valueOut = new HSFProfile<double>();
		    if(Ddata.Count != 0)  { // Are there any Profiles in there?
                if (Ddata.TryGetValue(key, out valueOut)) { //see if our key is in there
                    if (Previous != null) // Check whether we are at the first state
					    return HSFProfile<double>.MergeProfiles(valueOut, Previous.getFullProfile(key));
				    return valueOut;				
			    }
            }
		    if(Previous != null)
			    return Previous.getFullProfile(key); // If no data, return profile from previous states
		    return valueOut; //return empty profile
	   }

        /** 
         * Sets the integer Profile in the state with its matching key. If a Profile is found already under
         * that key, this will overwrite the old Profile.
         * @param key The integer state variable key that is being set.\
         * @param profIn The integer Profile being saved.
         */
        void setProfile(StateVarKey<double> key, HSFProfile<double> profIn){
            HSFProfile<double> valueOut;
            if (!Ddata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Ddata.Add(key, profIn); 
		    else { // Otherwise, erase whatever is there, and insert a new one.
			    Ddata.Remove(key);
			    Ddata.Add(key, profIn); 
		    }
        }

        /** 
	     * Adds a integer Profile value pair to the state with the given key. If no Profile exists, a new Profile is created
	     * with the corresponding key. If a Profile exists with that key, the pair is appended onto the end of the Profile. 
	     * Ensure that the Profile is still time ordered if this is the case.
	     * @param key The key corresponding to the state variable.
	     * @param pairIn The pair to be added to the integer Profile.
	     */
        void addValue(StateVarKey<double> key, KeyValuePair<double, double> pairIn){
            HSFProfile<double> valueIn = new HSFProfile<double>(pairIn);
            HSFProfile<double>valueOut;
            if (!Ddata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Ddata.Add(key, valueIn); 
		    else // Otherwise, add this data point to the existing Profile.
			    valueOut.Add(pairIn); //TODO: make sure this is ok. was formally iterator.second.data
	    }

        /** 
         * Adds a integer Profile to the state with the given key. If no Profile exists, a new Profile is created
         * with the corresponding key. If a Profile exists with that key, the Profile is appended onto the end of the Profile. 
         * @param key The key corresponding to the state variable.
         * @param profIn The Profile to be added to the integer Profile.
         */
        void addValue(StateVarKey<double> key, HSFProfile<double> profIn){
		    HSFProfile<double> valueOut;
            if (!Ddata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
			    Ddata.Add(key, profIn); 
		    else // Otherwise, add this data point to the existing Profile.
			    valueOut.Add(profIn);
	    }
    }
}
