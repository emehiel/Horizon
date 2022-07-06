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
    public class SystemStateIdea<T>
    {
        public SystemStateIdea<T> Previous { get; set; } = null;
        public Dictionary<string, HSFProfile<T>> Data { get; set; } = new Dictionary<string, HSFProfile<T>>();

        // Todo:  Rename this Merge?
        /** 
         * Adds a HSFProfile<T> to the state with the given key. If no Profile exists, a new Profile is created
         * with the corresponding key. If a Profile exists with that key, the Profile is appended onto the end of the Profile. 
         * @param key The key corresponding to the state variable.
         * @param profIn The Profile to be added to the boolean Profile.
         */
        public void AddValue(string key, HSFProfile<T> profIn)
        {
            HSFProfile<T> valueOut;
            if (!Data.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Data.Add(key, profIn);
            else // Otherwise, add this data point to the existing Profile.
                valueOut.Add(profIn);
        }

        /** 
	     * Adds a HSFProfile<T> value pair to the state with the given key. If no Profile exists, a new Profile is created
	     * with the corresponding key. If a Profile exists with that key, the pair is appended onto the end of the Profile. 
	     * Ensure that the Profile is still time ordered if this is the case.
	     * @param key The key corresponding to the state variable.
	     * @param pairIn The pair to be added to the boolean Profile.
	     */
        public void AddValue(string key, KeyValuePair<double, T> pairIn)
        {
            HSFProfile<T> valueIn = new HSFProfile<T>(pairIn);
            HSFProfile<T> valueOut;
            if (!Data.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Data.Add(key, valueIn);
            else // Otherwise, add this data point to the existing Profile.
                valueOut.Add(pairIn); //TODO: make sure this is ok. was formally iterator.second.data
        }

        /// <summary>
        /// combine two system states by adding the states from one into the other
        /// </summary>
        /// <param name="moreState"></param>
        public void Add(SystemStateIdea<T> moreState)
        {
            foreach (var data in moreState.Data)
                AddValue(data.Key, data.Value);
        }

        /// <summary>
        /// Returns the HSFProfile<T> for this state and all previous states merged into one Profile
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public HSFProfile<T> GetFullProfile(string key)
        {
            HSFProfile<T> valueOut = new HSFProfile<T>();
            if (Data.Count != 0)
            { // Are there any Profiles in there?
                if (Data.TryGetValue(key, out valueOut))
                { //see if our key is in there
                    if (Previous != null) // Check whether we are at the first state
                        return HSFProfile<T>.MergeProfiles(valueOut, Previous.GetFullProfile(key));
                    return valueOut;
                }
            }
            if (Previous != null)
                return Previous.GetFullProfile(key); // If no data, return profile from previous states
            return valueOut; //return empty profile
        }

        /// <summary>
        ///  Gets the last int value set for the given state variable key in the state. If no value is found
        ///  it checks the previous state, continuing all the way to the initial state.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValuePair<double, T> GetLastValue(string key)
        {
            HSFProfile<T> valueOut;
            if (Data.Count != 0)
            { // Are there any Profiles in there?
                if (Data.TryGetValue(key, out valueOut)) //see if our key is in there
                    return valueOut.Last(); //found it, return it TODO: return last value or pair?
            }
            return Previous.GetLastValue(key); //either no profiles or none that match our keys, try finding it in the previous one
        }

        /// <summary>
        /// Returns the HSFProfile<T> matching the key given. If no Profile is found, it goes back one Event
        /// and checks again until it gets to the initial state.
        /// </summary>
        /// <typeparam name="T">The type of the profile we are getting</typeparam>
        /// <param name="key">The state variable key that is being looked up.</param>
        /// <returns>Profile saved in the state.</returns>
        public HSFProfile<T> GetProfile(string key)
        {
            HSFProfile<T> valueOut;
            if (Data.Count != 0)
            {
                if (Data.TryGetValue(key, out valueOut))
                    return valueOut;
            }
            return Previous.GetProfile(key);
        }

        /// <summary>
        ///  Gets the <T> value of the state at a certain time.
        ///  If the exact time is not found, the data is assumed to be on a zero-order hold, and the last value set is found.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public KeyValuePair<double, T> GetValueAtTime(string key, double time)
        {
            HSFProfile<T> valueOut;
            if (Data.Count != 0)
            { // Are there any Profiles in there?
                if (Data.TryGetValue(key, out valueOut) && Data[key].LastTime() <= time) //see if our key is in there
                    return valueOut.DataAtTime(time);
            }
            return Previous.GetValueAtTime(key, time); //either no profiles or none that match our keys, try finding it in the previous one
        }

        public static SystemStateIdea<T> SetInitialSystemState(XmlNode ICNode, Asset asset)
        {
            // Set up Subsystem Nodes, first loop through the assets in the XML model input file

            // TODO:  Why is this a static method passing back a SystemState?  Shouldn't it just update the IC of the SystemState
            // TODO:  Get the HSFProfile<T> type, T from the XML Node?

            SystemStateIdea<T> state = new SystemStateIdea<T>();

            string type = ICNode.Attributes["type"].Value;
            string key = asset.Name + "." + ICNode.Attributes["key"].Value.ToLower();

            T val;
            Int32.TryParse(ICNode.Attributes["value"].Value, out val);
            //StateVarKey<int> svk = new StateVarKey<int>(key);
            state.AddValue(key, new KeyValuePair<double, T>(SimParameters.SimStartSeconds, val));

            return state;
        }

        /// <summary>
        /// Sets the integer Profile in the state with its matching key. If a Profile is found already under
        /// that key, this will overwrite the old Profile.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="profIn"></param>
        public void SetProfile(string key, HSFProfile<T> profIn)
        {
            HSFProfile<T> valueOut;
            if (!Data.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Data.Add(key, profIn);
            else
            { // Otherwise, erase whatever is there, and insert a new one.
                Data.Remove(key);
                Data.Add(key, profIn);
            }
        }

        public override string ToString()
        {
            string stateData = "";
            foreach (var data in Data)
            {
                stateData += data.Key + "," + data.Value.ToString();
            }
            return stateData;
        }
    }
}
