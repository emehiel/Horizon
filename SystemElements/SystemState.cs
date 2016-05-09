using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using System.Xml;
using UserModel;

namespace MissionElements
{
    public class SystemState
    {
        /** The previous state, upon which this state is based */
        public SystemState Previous { get; private set; }

        /** The Dictionary of integer Profiles. */
        public Dictionary<StateVarKey<int>, HSFProfile<int>> Idata { get; private set; }

        /** The Dictionary of double precision Profiles. */
        public Dictionary<StateVarKey<double>, HSFProfile<double>> Ddata { get; private set; }

        /** The Dictionary of floating point value Profiles. */
        //   public Dictionary<StateVarKey<float>, HSFProfile<float>> Fdata { get; private set; }

        /** The Dictionary of boolean Profiles. */
        public Dictionary<StateVarKey<bool>, HSFProfile<bool>> Bdata { get; private set; }

        /** The Dictionary of Matrix Profiles. */
        public Dictionary<StateVarKey<Matrix<double>>, HSFProfile<Matrix<double>>> Mdata { get; private set; }

        /** The Dictionary of Quaternion Profiles. */
        public Dictionary<StateVarKey<Quat>, HSFProfile<Quat>> Qdata { get; private set; }

        //public Dictionary<StateVarKey<T>, HSFProfile<T>> TData { get; set; }


        /**
         * Creates an initial State
         */
        public SystemState()
        {
            Previous = null;
            EventStart = 0;
            EventEnd = 0;
            TaskEnd = 0;
            TaskStart = 0;
            Idata = new Dictionary<StateVarKey<int>, HSFProfile<int>>();
            Ddata = new Dictionary<StateVarKey<double>, HSFProfile<double>>();
            Bdata = new Dictionary<StateVarKey<bool>, HSFProfile<bool>>();
            Mdata = new Dictionary<StateVarKey<Matrix<double>>, HSFProfile<Matrix<double>>>();
            Qdata = new Dictionary<StateVarKey<Quat>, HSFProfile<Quat>>();
        }

        /**
         * Copy constructor for exact state copies
         */
        public SystemState(SystemState initialStateToCopy)
        {
            SystemState newState = DeepCopy.Copy<SystemState>(initialStateToCopy);
            Previous = newState.Previous;
            EventStart = newState.EventStart;
            TaskStart = newState.TaskStart;
            TaskEnd = newState.TaskEnd;
            EventEnd = newState.EventEnd;
            Idata = newState.Idata;
            Ddata = newState.Ddata;
            //       Fdata = newState.Fdata;
            Bdata = newState.Bdata;
            Mdata = newState.Mdata;
            Qdata = newState.Qdata;
        }

        /**
         * Creates a new State based on a previous State and a new Task start time
         */
        public SystemState(SystemState previous, double newTaskStart)
        {
            Previous = previous;
            EventStart = previous.EventEnd; // start from end of previous State
            TaskStart = newTaskStart;
            TaskEnd = newTaskStart;
            EventEnd = newTaskStart;
            Idata = new Dictionary<StateVarKey<int>, HSFProfile<int>>();
            Ddata = new Dictionary<StateVarKey<double>, HSFProfile<double>>();
            Bdata = new Dictionary<StateVarKey<bool>, HSFProfile<bool>>();
            Mdata = new Dictionary<StateVarKey<Matrix<double>>, HSFProfile<Matrix<double>>>();
            Qdata = new Dictionary<StateVarKey<Quat>, HSFProfile<Quat>>();
        }


        /** TODO: figure out if this can all be done with dictionary stuff
         * Gets the last int value set for the given state variable key in the state. If no value is found
         * it checks the previous state, continuing all the way to the initial state.
         * @param key The integer state variable key that is being looked up.
         * @return A pair containing the last time the variable was set, and the integer value.
         */
        public KeyValuePair<double, int> getLastValue(StateVarKey<int> key) {
            HSFProfile<int> valueOut;
            if (Idata.Count != 0) { // Are there any Profiles in there?
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
        public KeyValuePair<double, int> getValueAtTime(StateVarKey<int> key, double time) {
            HSFProfile<int> valueOut;
            if (Idata.Count != 0) { // Are there any Profiles in there?
                if (Idata.TryGetValue(key, out valueOut)) //see if our key is in there
                    return valueOut.DataAtTime(time);
            }
            return Previous.getValueAtTime(key, time); //either no profiles or none that match our keys, try finding it in the previous one
        }

        /// <summary>
        /// Returns the integer Profile matching the key given. If no Profile is found, it goes back one Event
        /// and checks again until it gets to the initial state.
        /// </summary>
        /// <typeparam name="T">The type of the profile we are getting</typeparam>
        /// <param name="_key">The state variable key that is being looked up.</param>
        /// <returns>Profile saved in the state.</returns>
        public HSFProfile<T> GetProfile<T>(StateVarKey<T> _key)
       {
           if (_key.GetType() == typeof(StateVarKey<int>))
           {
               HSFProfile<int> valueOut;
               if (Idata.Count != 0)
               { // Are there any Profiles in there?
                   if (Idata.TryGetValue(_key, out valueOut)) //see if our key is in there
                       return (dynamic)valueOut;
                   return Previous.GetProfile(_key); // This isn't the right profile, go back one and try it out!
               }
           }
           else if (_key.GetType() == typeof(StateVarKey<double>))
           {
                HSFProfile<double> valueOut;
               if (Ddata.Count != 0)
               { // Are there any Profiles in there?
                   if (Ddata.TryGetValue(_key, out valueOut)) //see if our key is in there
                       return (dynamic)valueOut;
                   return Previous.GetProfile(_key); // This isn't the right profile, go back one and try it out!
               }
           }
           else if (_key.GetType() == typeof(StateVarKey<bool>))
           {
               HSFProfile<bool> valueOut;
               if (Ddata.Count != 0)
               { // Are there any Profiles in there?
                   if (Bdata.TryGetValue(_key, out valueOut)) //see if our key is in there
                       return (dynamic)valueOut;
                   return Previous.GetProfile(_key); // This isn't the right profile, go back one and try it out!
               }
           }
           else if (_key.GetType() == typeof(StateVarKey<Matrix<double>>))
           {
               HSFProfile<Matrix<double>> valueOut;
               if (Ddata.Count != 0)
               { // Are there any Profiles in there?
                   if (Mdata.TryGetValue(_key, out valueOut)) //see if our key is in there
                       return (dynamic)valueOut;
                   return Previous.GetProfile(_key); // This isn't the right profile, go back one and try it out!
               }
           }
           else if (_key.GetType() == typeof(StateVarKey<Quat>))
           {
               HSFProfile<Quat> valueOut;
               if (Ddata.Count != 0)
               { // Are there any Profiles in there?
                   if (Qdata.TryGetValue(_key, out valueOut)) //see if our key is in there
                       return (dynamic)valueOut;
                   return Previous.GetProfile(_key); // This isn't the right profile, go back one and try it out!
               }
           }
           throw new ArgumentException("Profile Type Not Found");
       }

       /** TODO: make sure valueOut is a good replacement for iterator.second
        * Returns the integer Profile for this state and all previous states merged into one Profile
        * @param key The integer state variable key that is being looked up.
        * @return The full Profile
        */
        public HSFProfile<int> getFullProfile(StateVarKey<int> key) {
            HSFProfile<int> valueOut = new HSFProfile<int>();
            if (Idata.Count != 0) { // Are there any Profiles in there?
                if (Idata.TryGetValue(key, out valueOut)) { //see if our key is in there
                    if (Previous != null) // Check whether we are at the first state
                        return HSFProfile<int>.MergeProfiles(valueOut, Previous.getFullProfile(key));
                    return valueOut;
                }
            }
            if (Previous != null)
                return Previous.getFullProfile(key); // If no data, return profile from previous states
            return valueOut; //return empty profile
        }

        /** 
         * Sets the integer Profile in the state with its matching key. If a Profile is found already under
         * that key, this will overwrite the old Profile.
         * @param key The integer state variable key that is being set.\
         * @param profIn The integer Profile being saved.
         */
        public void setProfile(StateVarKey<int> key, HSFProfile<int> profIn) {
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
        void addValue(StateVarKey<int> key, KeyValuePair<double, int> pairIn) {
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
        public void addValue(StateVarKey<int> key, HSFProfile<int> profIn) {
            HSFProfile<int> valueOut;
            if (!Idata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Idata.Add(key, profIn);
            else // Otherwise, add this data point to the existing Profile.
                valueOut.Add(profIn);
        }
        /*      
        * Gets the last double value set for the given state variable key in the state. If no value is found
        * it checks the previous state, continuing all the way to the initial state.
        * @param key The double state variable key that is being looked up.
        * @return A pair containing the last time the variable was set, and the double value.
        */
        public KeyValuePair<double, double> getLastValue(StateVarKey<double> key) {
            HSFProfile<double> valueOut;
            if (Ddata.Count != 0) { // Are there any Profiles in there?
                if (Ddata.TryGetValue(key, out valueOut)) //see if our key is in there
                    return valueOut.Last(); //found it, return it TODO: return last value or pair?
            }
            return Previous.getLastValue(key); //either no profiles or none that match our keys, try finding it in the previous one
        }
        /** 
         * Gets the double value of the state at a certain time. If the exact time is not found, the data is
         * assumed to be on a zero-order hold, and the last value set is found.
         * @param key The double state variable key that is being looked up.
         * @param time The time the value is looked up at.
         * @return A pair containing the last time the variable was set, and the double value.
         */
        public KeyValuePair<double, double> getValueAtTime(StateVarKey<double> key, double time) {
            HSFProfile<double> valueOut;
            if (Ddata.Count != 0) { // Are there any Profiles in there?
                if (Ddata.TryGetValue(key, out valueOut)) //see if our key is in there
                    return valueOut.DataAtTime(time);
            }
            return Previous.getValueAtTime(key, time); //either no profiles or none that match our keys, try finding it in the previous one
        }

        /** TODO: make sure valueOut is a good replacement for iterator.second
         * Returns the double Profile for this state and all previous states merged into one Profile
         * @param key The double state variable key that is being looked up.
         * @return The full Profile
         */
        public HSFProfile<double> getFullProfile(StateVarKey<double> key) {
            HSFProfile<double> valueOut = new HSFProfile<double>();
            if (Ddata.Count != 0) { // Are there any Profiles in there?
                if (Ddata.TryGetValue(key, out valueOut)) { //see if our key is in there
                    if (Previous != null) // Check whether we are at the first state
                        return HSFProfile<double>.MergeProfiles(valueOut, Previous.getFullProfile(key));
                    return valueOut;
                }
            }
            if (Previous != null)
                return Previous.getFullProfile(key); // If no data, return profile from previous states
            return valueOut; //return empty profile
        }

        /** 
         * Sets the double Profile in the state with its matching key. If a Profile is found already under
         * that key, this will overwrite the old Profile.
         * @param key The double state variable key that is being set.\
         * @param profIn The double Profile being saved.
         */
        public void setProfile(StateVarKey<double> key, HSFProfile<double> profIn) {
            HSFProfile<double> valueOut;
            if (!Ddata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Ddata.Add(key, profIn);
            else { // Otherwise, erase whatever is there, and insert a new one.
                Ddata.Remove(key);
                Ddata.Add(key, profIn);
            }
        }

        /** 
	     * Adds a double Profile value pair to the state with the given key. If no Profile exists, a new Profile is created
	     * with the corresponding key. If a Profile exists with that key, the pair is appended onto the end of the Profile. 
	     * Ensure that the Profile is still time ordered if this is the case.
	     * @param key The key corresponding to the state variable.
	     * @param pairIn The pair to be added to the integer Profile.
	     */
        public void addValue(StateVarKey<double> key, KeyValuePair<double, double> pairIn) {
            HSFProfile<double> valueIn = new HSFProfile<double>(pairIn);
            HSFProfile<double> valueOut;
            if (!Ddata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Ddata.Add(key, valueIn);
            else // Otherwise, add this data point to the existing Profile.
                valueOut.Add(pairIn); //TODO: make sure this is ok. was formally iterator.second.data
        }

        /** 
         * Adds a double Profile to the state with the given key. If no Profile exists, a new Profile is created
         * with the corresponding key. If a Profile exists with that key, the Profile is appended onto the end of the Profile. 
         * @param key The key corresponding to the state variable.
         * @param profIn The Profile to be added to the double Profile.
         */
        public void addValue(StateVarKey<double> key, HSFProfile<double> profIn) {
            HSFProfile<double> valueOut;
            if (!Ddata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Ddata.Add(key, profIn);
            else // Otherwise, add this data point to the existing Profile.
                valueOut.Add(profIn);
        }
        /*      
        * Gets the last boolean value set for the given state variable key in the state. If no value is found
        * it checks the previous state, continuing all the way to the initial state.
        * @param key The boolean state variable key that is being looked up.
        * @return A pair containing the last time the variable was set, and the boolean value.
        */
        public KeyValuePair<double, bool> getLastValue(StateVarKey<bool> key)
        {
            HSFProfile<bool> valueOut;
            if (Bdata.Count != 0)
            { // Are there any Profiles in there?
                if (Bdata.TryGetValue(key, out valueOut)) //see if our key is in there
                    return valueOut.Last(); //found it, return it TODO: return last value or pair?
            }
            return Previous.getLastValue(key); //either no profiles or none that match our keys, try finding it in the previous one
        }
        /** 
         * Gets the boolean value of the state at a certain time. If the exact time is not found, the data is
         * assumed to be on a zero-order hold, and the last value set is found.
         * @param key The boolean state variable key that is being looked up.
         * @param time The time the value is looked up at.
         * @return A pair containing the last time the variable was set, and the boolean value.
         */
        public KeyValuePair<double, bool> getValueAtTime(StateVarKey<bool> key, double time)
        {
            HSFProfile<bool> valueOut;
            if (Bdata.Count != 0)
            { // Are there any Profiles in there?
                if (Bdata.TryGetValue(key, out valueOut)) //see if our key is in there
                    return valueOut.DataAtTime(time);
            }
            return Previous.getValueAtTime(key, time); //either no profiles or none that match our keys, try finding it in the previous one
        }

        /*
        * Returns the boolean Profile for this state and all previous states merged into one Profile
        * @param key The boolean state variable key that is being looked up.
        * @return The full Profile
        */
        public HSFProfile<bool> getFullProfile(StateVarKey<bool> key)
        {
            HSFProfile<bool> valueOut = new HSFProfile<bool>();
            if (Bdata.Count != 0)
            { // Are there any Profiles in there?
                if (Bdata.TryGetValue(key, out valueOut))
                { //see if our key is in there
                    if (Previous != null) // Check whether we are at the first state
                        return HSFProfile<bool>.MergeProfiles(valueOut, Previous.getFullProfile(key));
                    return valueOut;
                }
            }
            if (Previous != null)
                return Previous.getFullProfile(key); // If no data, return profile from previous states
            return valueOut; //return empty profile
        }

        /** 
         * Sets the boolean Profile in the state with its matching key. If a Profile is found already under
         * that key, this will overwrite the old Profile.
         * @param key The boolean state variable key that is being set.\
         * @param profIn The boolean Profile being saved.
         */
        public void setProfile(StateVarKey<bool> key, HSFProfile<bool> profIn)
        {
            HSFProfile<bool> valueOut;
            if (!Bdata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Bdata.Add(key, profIn);
            else { // Otherwise, erase whatever is there, and insert a new one.
                Bdata.Remove(key);
                Bdata.Add(key, profIn);
            }
        }

        /** 
	     * Adds a boolean Profile value pair to the state with the given key. If no Profile exists, a new Profile is created
	     * with the corresponding key. If a Profile exists with that key, the pair is appended onto the end of the Profile. 
	     * Ensure that the Profile is still time ordered if this is the case.
	     * @param key The key corresponding to the state variable.
	     * @param pairIn The pair to be added to the boolean Profile.
	     */
        public void addValue(StateVarKey<bool> key, KeyValuePair<double, bool> pairIn)
        {
            HSFProfile<bool> valueIn = new HSFProfile<bool>(pairIn);
            HSFProfile<bool> valueOut;
            if (!Bdata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Bdata.Add(key, valueIn);
            else // Otherwise, add this data point to the existing Profile.
                valueOut.Add(pairIn); //TODO: make sure this is ok. was formally iterator.second.data
        }

        /** 
         * Adds a boolean Profile to the state with the given key. If no Profile exists, a new Profile is created
         * with the corresponding key. If a Profile exists with that key, the Profile is appended onto the end of the Profile. 
         * @param key The key corresponding to the state variable.
         * @param profIn The Profile to be added to the boolean Profile.
         */
        public void addValue(StateVarKey<bool> key, HSFProfile<bool> profIn)
        {
            HSFProfile<bool> valueOut;
            if (!Bdata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Bdata.Add(key, profIn);
            else // Otherwise, add this data point to the existing Profile.
                valueOut.Add(profIn);
        }

        /**      
        * Gets the last Matrix value set for the given state variable key in the state. If no value is found
        * it checks the previous state, continuing all the way to the initial state.
        * @param key The Matrix state variable key that is being looked up.
        * @return A pair containing the last time the variable was set, and the Matrix value.
        */
        public KeyValuePair<double, Matrix<double>> getLastValue(StateVarKey<Matrix<double>> key)
        {
            HSFProfile<Matrix<double>> valueOut;
            if (Mdata.Count != 0)
            { // Are there any Profiles in there?
                if (Mdata.TryGetValue(key, out valueOut)) //see if our key is in there
                    return valueOut.Last(); //found it, return it TODO: return last value or pair?
            }
            return Previous.getLastValue(key); //either no profiles or none that match our keys, try finding it in the previous one
        }
        /** 
         * Gets the Matrix value of the state at a certain time. If the exact time is not found, the data is
         * assumed to be on a zero-order hold, and the last value set is found.
         * @param key The Matrix state variable key that is being looked up.
         * @param time The time the value is looked up at.
         * @return A pair containing the last time the variable was set, and the matrix value.
         */
        public KeyValuePair<double, Matrix<double>> getValueAtTime(StateVarKey<Matrix<double>> key, double time)
        {
            HSFProfile<Matrix<double>> valueOut;
            if (Mdata.Count != 0)
            { // Are there any Profiles in there?
                if (Mdata.TryGetValue(key, out valueOut)) //see if our key is in there
                    return valueOut.DataAtTime(time);
            }
            return Previous.getValueAtTime(key, time); //either no profiles or none that match our keys, try finding it in the previous one
        }


        /*
        * Returns the Matrix Profile for this state and all previous states merged into one Profile
        * @param key The Matrix state variable key that is being looked up.
        * @return The full Profile
        */
        public HSFProfile<Matrix<double>> getFullProfile(StateVarKey<Matrix<double>> key)
        {
            HSFProfile<Matrix<double>> valueOut = new HSFProfile<Matrix<double>>();
            if (Mdata.Count != 0)
            { // Are there any Profiles in there?
                if (Mdata.TryGetValue(key, out valueOut))
                { //see if our key is in there
                    if (Previous != null) // Check whether we are at the first state
                        return HSFProfile<Matrix<double>>.MergeProfiles(valueOut, Previous.getFullProfile(key));
                    return valueOut;
                }
            }
            if (Previous != null)
                return Previous.getFullProfile(key); // If no data, return profile from previous states
            return valueOut; //return empty profile
        }

        /** 
         * Sets the Matrix Profile in the state with its matching key. If a Profile is found already under
         * that key, this will overwrite the old Profile.
         * @param key The Matrix state variable key that is being set.\
         * @param profIn The Matrix Profile being saved.
         */
        public void setProfile(StateVarKey<Matrix<double>> key, HSFProfile<Matrix<double>> profIn)
        {
            HSFProfile<Matrix<double>> valueOut;
            if (!Mdata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Mdata.Add(key, profIn);
            else { // Otherwise, erase whatever is there, and insert a new one.
                Mdata.Remove(key);
                Mdata.Add(key, profIn);
            }
        }

        /** 
	     * Adds a Matrix Profile value pair to the state with the given key. If no Profile exists, a new Profile is created
	     * with the corresponding key. If a Profile exists with that key, the pair is appended onto the end of the Profile. 
	     * Ensure that the Profile is still time ordered if this is the case.
	     * @param key The key corresponding to the state variable.
	     * @param pairIn The pair to be added to the matrix Profile.
	     */
        public void addValue(StateVarKey<Matrix<double>> key, KeyValuePair<double, Matrix<double>> pairIn)
        {
            HSFProfile<Matrix<double>> valueIn = new HSFProfile<Matrix<double>>(pairIn);
            HSFProfile<Matrix<double>> valueOut = new HSFProfile<Matrix<double>>();
            if (!Mdata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Mdata.Add(key, valueIn);
            else // Otherwise, add this data point to the existing Profile.
                valueOut.Add(pairIn); //TODO: make sure this is ok. was formally iterator.second.data
        }

        /** 
         * Adds a boolean Profile to the state with the given key. If no Profile exists, a new Profile is created
         * with the corresponding key. If a Profile exists with that key, the Profile is appended onto the end of the Profile. 
         * @param key The key corresponding to the state variable.
         * @param profIn The Profile to be added to the boolean Profile.
         */
        public void addValue(StateVarKey<Matrix<double>> key, HSFProfile<Matrix<double>> profIn)
        {
            HSFProfile<Matrix<double>> valueOut;
            if (!Mdata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Mdata.Add(key, profIn);
            else // Otherwise, add this data point to the existing Profile.
                valueOut.Add(profIn);
        }
        //maybe add this functionality to subsystem? but then we would need to pass the state to the constructor
        public static SystemState setInitialSystemState(List<XmlNode> ICNodes)
        {
            // Set up Subsystem Nodes, first loop through the assets in the XML model input file
            //int n = modelInputXMLNode.ChildNode("ASSET");
            SystemState state = new SystemState();
            foreach (XmlNode ICNode in ICNodes)
            {
                string type = ICNode.Attributes["type"].Value.ToString();
                string key = ICNode.Attributes["key"].Value.ToString();
                if (type.Equals("Int"))
                {
                    int val;
                    Int32.TryParse(ICNode.Attributes["value"].Value.ToString(), out val);
                    StateVarKey<int> svk = new StateVarKey<int>(key);
                    state.addValue(svk, new KeyValuePair<double, int>(SimParameters.SimStartSeconds, val));
                }
                //else if (type.Equals("Float"))
                //{
                //    float val;
                //    float.TryParse(ICNode.Attributes["value"].Value.ToString(), out val);
                //    StateVarKey<double> svk = new StateVarKey<double>(key);
                //    state.addValue(svk, new KeyValuePair<double, double>(SimParameters.SimStartSeconds, val));
                //}
                else if (type.Equals("Double"))
                {
                    double val;
                    Double.TryParse(ICNode.Attributes["value"].ToString(), out val);
                    StateVarKey<double> svk = new StateVarKey<double>(key);
                    state.addValue(svk, new KeyValuePair<double, double>(SimParameters.SimStartSeconds, val));
                }
                else if (type.Equals("Bool"))
                {
                    string val = ICNode.Attributes["value"].Value.ToString();
                    bool val_ = false;
                    if (val.Equals("True") || val.Equals("1"))
                        val_ = true;
                    StateVarKey<bool> svk = new StateVarKey<bool>(key);
                    state.addValue(svk, new KeyValuePair<double, bool>(SimParameters.SimStartSeconds, val_));
                }
                else if (type.Equals("Matrix"))
                {
                    Matrix<double> val = new Matrix<double>(ICNode.Attributes["value"].ToString());
                    StateVarKey<Matrix<double>> svk = new StateVarKey<Matrix<double>>(key);
                    state.addValue(svk, new KeyValuePair<double, Matrix<double>>(SimParameters.SimStartSeconds, val));
                }
                else if (type.Equals("Quat"))
                {
                    // Quaternions still need an initializer from a string, like Matrices
                }
            }
            return state;
        }
        
    }
}
