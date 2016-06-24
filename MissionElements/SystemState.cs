// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.Xml;
using UserModel;

namespace MissionElements
{
    [Serializable]
    public class SystemState
    {
        /** The previous state, upon which this state is based */
        public SystemState Previous { get; set; }

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


        /// <summary>
        /// Creates an initial state   
        /// </summary>
        public SystemState()
        {
            Previous = null;
            Idata = new Dictionary<StateVarKey<int>, HSFProfile<int>>();
            Ddata = new Dictionary<StateVarKey<double>, HSFProfile<double>>();
            Bdata = new Dictionary<StateVarKey<bool>, HSFProfile<bool>>();
            Mdata = new Dictionary<StateVarKey<Matrix<double>>, HSFProfile<Matrix<double>>>();
            Qdata = new Dictionary<StateVarKey<Quat>, HSFProfile<Quat>>();
        }


       /// <summary>
       /// Create a new state from a previous one
       /// </summary>
       /// <param name="previous"></param>
        public SystemState(SystemState previous)
        {
            Previous = previous;
            Idata = new Dictionary<StateVarKey<int>, HSFProfile<int>>();
            Ddata = new Dictionary<StateVarKey<double>, HSFProfile<double>>();
            Bdata = new Dictionary<StateVarKey<bool>, HSFProfile<bool>>();
            Mdata = new Dictionary<StateVarKey<Matrix<double>>, HSFProfile<Matrix<double>>>();
            Qdata = new Dictionary<StateVarKey<Quat>, HSFProfile<Quat>>();
        }
        /// <summary>
        /// Use this constructor to create a copy of another state
        /// </summary>
        /// <param name="state"></param>
        /// <param name="copy"> Only used to distinguish constructor calls by prototype</param>
        public SystemState(SystemState state, int copy)
        {
            Previous = state.Previous;
            Idata = new Dictionary<StateVarKey<int>, HSFProfile<int>>(state.Idata);
            Ddata = new Dictionary<StateVarKey<double>, HSFProfile<double>>(state.Ddata);
            Bdata = new Dictionary<StateVarKey<bool>, HSFProfile<bool>>(state.Bdata);
            Mdata = new Dictionary<StateVarKey<Matrix<double>>, HSFProfile<Matrix<double>>>(state.Mdata);
            Qdata = new Dictionary<StateVarKey<Quat>, HSFProfile<Quat>>(state.Qdata);
        }
        /// <summary>
        /// combine two system states by adding the states from one into the other
        /// </summary>
        /// <param name="moreState"></param>
        public void Add(SystemState moreState)
        {
            foreach (var data in moreState.Idata)
                AddValue(data.Key, data.Value);
            foreach (var data in moreState.Ddata)
                AddValue(data.Key, data.Value);
            foreach (var data in moreState.Bdata)
                AddValue(data.Key, data.Value);
            foreach (var data in moreState.Mdata)
                AddValue(data.Key, data.Value);
        }

        public override string ToString()
        {
            string stateData = "";
            foreach(var data in Idata)
            {
                stateData += data.Key.VarName + "," + data.Value.ToString();
            }
            return stateData;
        }

        /// <summary>
        ///  Gets the last int value set for the given state variable key in the state. If no value is found
        ///  it checks the previous state, continuing all the way to the initial state.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValuePair<double, int> GetLastValue(StateVarKey<int> key) {
            HSFProfile<int> valueOut;
            if (Idata.Count != 0) { // Are there any Profiles in there?
                if (Idata.TryGetValue(key, out valueOut)) //see if our key is in there
                    return valueOut.Last(); //found it, return it TODO: return last value or pair?
            }
            return Previous.GetLastValue(key); //either no profiles or none that match our keys, try finding it in the previous one
        }

        /// <summary>
        ///  Gets the integer value of the state at a certain time.
        ///  If the exact time is not found, the data is assumed to be on a zero-order hold, and the last value set is found.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public KeyValuePair<double, int> GetValueAtTime(StateVarKey<int> key, double time) {
            HSFProfile<int> valueOut;
            if (Idata.Count != 0) { // Are there any Profiles in there?
                if (Idata.TryGetValue(key, out valueOut) && Idata[key].LastTime() <= time) //see if our key is in there
                    return valueOut.DataAtTime(time);
            }
            return Previous.GetValueAtTime(key, time); //either no profiles or none that match our keys, try finding it in the previous one
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
               }
                return Previous.GetProfile(_key); // This isn't the right profile, go back one and try it out!
            }
            else if (_key.GetType() == typeof(StateVarKey<double>))
           {
                HSFProfile<double> valueOut;
               if (Ddata.Count != 0)
               { // Are there any Profiles in there?
                   if (Ddata.TryGetValue(_key, out valueOut)) //see if our key is in there
                       return (dynamic)valueOut;
               }
               return Previous.GetProfile(_key);  // This isn't the right profile, go back one and try it out!
            }
           else if (_key.GetType() == typeof(StateVarKey<bool>))
           {
               HSFProfile<bool> valueOut;
               if (Ddata.Count != 0)
               { // Are there any Profiles in there?
                   if (Bdata.TryGetValue(_key, out valueOut)) //see if our key is in there
                       return (dynamic)valueOut;
               }
                return Previous.GetProfile(_key); // This isn't the right profile, go back one and try it out!
            }
            else if (_key.GetType() == typeof(StateVarKey<Matrix<double>>))
           {
               HSFProfile<Matrix<double>> valueOut;
               if (Ddata.Count != 0)
               { // Are there any Profiles in there?
                   if (Mdata.TryGetValue(_key, out valueOut)) //see if our key is in there
                       return (dynamic)valueOut;
               }
               return Previous.GetProfile(_key); // This isn't the right profile, go back one and try it out!

            }
            else if (_key.GetType() == typeof(StateVarKey<Quat>))
           {
               HSFProfile<Quat> valueOut;
               if (Ddata.Count != 0)
               { // Are there any Profiles in there?
                   if (Qdata.TryGetValue(_key, out valueOut)) //see if our key is in there
                       return (dynamic)valueOut;
               }
               return Previous.GetProfile(_key); // This isn't the right profile, go back one and try it out!
            }
            throw new ArgumentException("Profile Type Not Found");
       }

        /// <summary>
        /// Returns the integer Profile for this state and all previous states merged into one Profile
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public HSFProfile<int> GetFullProfile(StateVarKey<int> key) {
            HSFProfile<int> valueOut = new HSFProfile<int>();
            if (Idata.Count != 0) { // Are there any Profiles in there?
                if (Idata.TryGetValue(key, out valueOut)) { //see if our key is in there
                    if (Previous != null) // Check whether we are at the first state
                        return HSFProfile<int>.MergeProfiles(valueOut, Previous.GetFullProfile(key));
                    return valueOut;
                }
            }
            if (Previous != null)
                return Previous.GetFullProfile(key); // If no data, return profile from previous states
            return valueOut; //return empty profile
        }

        /// <summary>
        /// Sets the integer Profile in the state with its matching key. If a Profile is found already under
        /// that key, this will overwrite the old Profile.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="profIn"></param>
        public void SetProfile(StateVarKey<int> key, HSFProfile<int> profIn) {
            HSFProfile<int> valueOut;
            if (!Idata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Idata.Add(key, profIn);
            else { // Otherwise, erase whatever is there, and insert a new one.
                Idata.Remove(key);
                Idata.Add(key, profIn);
            }
        }

        /// <summary>
        /// Adds a Matrix Profile value pair to the state with the given key. If no Profile exists, a new Profile is created
        /// with the corresponding key. If a Profile exists with that key, the pair is appended onto the end of the Profile. </summary>
        /// Ensure that the Profile is still time ordered if this is the case.<param name="key"></param>
        /// <param name="pairIn"></param>
        void AddValue(StateVarKey<int> key, KeyValuePair<double, int> pairIn) {
            HSFProfile<int> valueIn = new HSFProfile<int>(pairIn);
            HSFProfile<int> valueOut;
            if (!Idata.TryGetValue(key, out valueOut))
            { // If there's no Profile matching that key, insert a new one.
                Idata.Add(key, valueIn);
            }
            else // Otherwise, add this data point to the existing Profile.
                valueOut.Add(pairIn); //TODO: make sure this is ok. was formally iterator.second.data
        }

        /// <summary>
        /// Adds a Matrix Profile value pair to the state with the given key. If no Profile exists, a new Profile is created
        /// with the corresponding key. If a Profile exists with that key, the pair is appended onto the end of the Profile. </summary>
        /// Ensure that the Profile is still time ordered if this is the case.<param name="key"></param>
        /// <param name="pairIn"></param>
        public void AddValue(StateVarKey<int> key, HSFProfile<int> profIn) {
            HSFProfile<int> valueOut;
            if (!Idata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Idata.Add(key, profIn);
            else // Otherwise, add this data point to the existing Profile.
                valueOut.Add(profIn);
        }
        /// <summary>
        ///  Gets the last int value set for the given state variable key in the state. If no value is found
        ///  it checks the previous state, continuing all the way to the initial state.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValuePair<double, double> GetLastValue(StateVarKey<double> key) {
            HSFProfile<double> valueOut;
            if (Ddata.Count != 0) { // Are there any Profiles in there?
                if (Ddata.TryGetValue(key, out valueOut)) //see if our key is in there
                    return valueOut.Last(); //found it, return it TODO: return last value or pair?
            }
            return Previous.GetLastValue(key); //either no profiles or none that match our keys, try finding it in the previous one
        }

        /// <summary>
        ///  Gets the integer value of the state at a certain time.
        ///  If the exact time is not found, the data is assumed to be on a zero-order hold, and the last value set is found.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public KeyValuePair<double, double> GetValueAtTime(StateVarKey<double> key, double time) {
            HSFProfile<double> valueOut;
            if (Ddata.Count != 0) { // Are there any Profiles in there?
                if (Ddata.TryGetValue(key, out valueOut) && Ddata[key].LastTime() <= time) //see if our key is in there
                    return valueOut.DataAtTime(time);
            }
            return Previous.GetValueAtTime(key, time); //either no profiles or none that match our keys, try finding it in the previous one
        }

        /// <summary>
        /// Returns the integer Profile for this state and all previous states merged into one Profile
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public HSFProfile<double> GetFullProfile(StateVarKey<double> key) {
            HSFProfile<double> valueOut = new HSFProfile<double>();
            if (Ddata.Count != 0) { // Are there any Profiles in there?
                if (Ddata.TryGetValue(key, out valueOut)) { //see if our key is in there
                    if (Previous != null) // Check whether we are at the first state
                        return HSFProfile<double>.MergeProfiles(valueOut, Previous.GetFullProfile(key));
                    return valueOut;
                }
            }
            if (Previous != null)
                return Previous.GetFullProfile(key); // If no data, return profile from previous states
            return valueOut; //return empty profile
        }

        /// <summary>
        /// Sets the integer Profile in the state with its matching key. If a Profile is found already under
        /// that key, this will overwrite the old Profile.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="profIn"></param>
        public void SetProfile(StateVarKey<double> key, HSFProfile<double> profIn) {
            HSFProfile<double> valueOut;
            if (!Ddata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Ddata.Add(key, profIn);
            else { // Otherwise, erase whatever is there, and insert a new one.
                Ddata.Remove(key);
                Ddata.Add(key, profIn);
            }
        }

        /// <summary>
        /// Adds a double Profile value pair to the state with the given key. If no Profile exists, a new Profile is created
        /// with the corresponding key. If a Profile exists with that key, the pair is appended onto the end of the Profile. 
        /// </summary>
        /// Ensure that the Profile is still time ordered if this is the case.<param name="key"></param>
        /// <param name="pairIn"></param>
        public void AddValue(StateVarKey<double> key, KeyValuePair<double, double> pairIn) {
            HSFProfile<double> valueIn = new HSFProfile<double>(pairIn);
            HSFProfile<double> valueOut;
            if (!Ddata.TryGetValue(key, out valueOut))
            { // If there's no Profile matching that key, insert a new one.
                Ddata.Add(key, valueIn);
            }
            else // Otherwise, add this data point to the existing Profile.
                valueOut.Add(pairIn); //TODO: make sure this is ok. was formally iterator.second.data
        }

        /// <summary>
        /// Adds a Matrix Profile value pair to the state with the given key. If no Profile exists, a new Profile is created
        /// with the corresponding key. If a Profile exists with that key, the pair is appended onto the end of the Profile. </summary>
        /// Ensure that the Profile is still time ordered if this is the case.<param name="key"></param>
        /// <param name="pairIn"></param>
        public void AddValue(StateVarKey<double> key, HSFProfile<double> profIn) {
            HSFProfile<double> valueOut;
            if (!Ddata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Ddata.Add(key, profIn);
            else // Otherwise, add this data point to the existing Profile.
                valueOut.Add(profIn);
        }
        //public void addValue(StateVarKey<double> key, IronPython.Runtime.PythonTuple profIn)
        //{
        //    HSFProfile<double> valueOut;
        //    if (!Ddata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
        //        Ddata.Add(key, profIn);
        //    else // Otherwise, add this data point to the existing Profile.
        //        valueOut.Add(profIn);
        //}

        /// <summary>
        ///  Gets the last int value set for the given state variable key in the state. If no value is found
        ///  it checks the previous state, continuing all the way to the initial state.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValuePair<double, bool> GetLastValue(StateVarKey<bool> key)
        {
            HSFProfile<bool> valueOut;
            if (Bdata.Count != 0)
            { // Are there any Profiles in there?
                if (Bdata.TryGetValue(key, out valueOut)) //see if our key is in there
                    return valueOut.Last(); //found it, return it TODO: return last value or pair?
            }
            return Previous.GetLastValue(key); //either no profiles or none that match our keys, try finding it in the previous one
        }

        /// <summary>
        ///  Gets the integer value of the state at a certain time.
        ///  If the exact time is not found, the data is assumed to be on a zero-order hold, and the last value set is found.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public KeyValuePair<double, bool> GetValueAtTime(StateVarKey<bool> key, double time)
        {
            HSFProfile<bool> valueOut;
            if (Bdata.Count != 0)
            { // Are there any Profiles in there?
                if (Bdata.TryGetValue(key, out valueOut) && Bdata[key].LastTime() <= time) //see if our key is in there
                    return valueOut.DataAtTime(time);
            }
            return Previous.GetValueAtTime(key, time); //either no profiles or none that match our keys, try finding it in the previous one
        }

        /// <summary>
        /// Returns the integer Profile for this state and all previous states merged into one Profile
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public HSFProfile<bool> GetFullProfile(StateVarKey<bool> key)
        {
            HSFProfile<bool> valueOut = new HSFProfile<bool>();
            if (Bdata.Count != 0)
            { // Are there any Profiles in there?
                if (Bdata.TryGetValue(key, out valueOut))
                { //see if our key is in there
                    if (Previous != null) // Check whether we are at the first state
                        return HSFProfile<bool>.MergeProfiles(valueOut, Previous.GetFullProfile(key));
                    return valueOut;
                }
            }
            if (Previous != null)
                return Previous.GetFullProfile(key); // If no data, return profile from previous states
            return valueOut; //return empty profile
        }

        /// <summary>
        /// Sets the integer Profile in the state with its matching key. If a Profile is found already under
        /// that key, this will overwrite the old Profile.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="profIn"></param>
        public void SetProfile(StateVarKey<bool> key, HSFProfile<bool> profIn)
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
        public void AddValue(StateVarKey<bool> key, KeyValuePair<double, bool> pairIn)
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
        public void AddValue(StateVarKey<bool> key, HSFProfile<bool> profIn)
        {
            HSFProfile<bool> valueOut;
            if (!Bdata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Bdata.Add(key, profIn);
            else // Otherwise, add this data point to the existing Profile.
                valueOut.Add(profIn);
        }

        /// <summary>
        ///  Gets the last int value set for the given state variable key in the state. If no value is found
        ///  it checks the previous state, continuing all the way to the initial state.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValuePair<double, Matrix<double>> GetLastValue(StateVarKey<Matrix<double>> key)
        {
            HSFProfile<Matrix<double>> valueOut;
            if (Mdata.Count != 0)
            { // Are there any Profiles in there?
                if (Mdata.TryGetValue(key, out valueOut)) //see if our key is in there
                    return valueOut.Last(); //found it, return it TODO: return last value or pair?
            }
            return Previous.GetLastValue(key); //either no profiles or none that match our keys, try finding it in the previous one
        }

        /// <summary>
        ///  Gets the integer value of the state at a certain time.
        ///  If the exact time is not found, the data is assumed to be on a zero-order hold, and the last value set is found.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public KeyValuePair<double, Matrix<double>> GetValueAtTime(StateVarKey<Matrix<double>> key, double time)
        {
            HSFProfile<Matrix<double>> valueOut;
            if (Mdata.Count != 0)
            { // Are there any Profiles in there?
                if (Mdata.TryGetValue(key, out valueOut) && Mdata[key].LastTime() <= time) //see if our key is in there
                    return valueOut.DataAtTime(time);
            }
            return Previous.GetValueAtTime(key, time); //either no profiles or none that match our keys, try finding it in the previous one
        }

        /// <summary>
        /// Returns the integer Profile for this state and all previous states merged into one Profile
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public HSFProfile<Matrix<double>> GetFullProfile(StateVarKey<Matrix<double>> key)
        {
            HSFProfile<Matrix<double>> valueOut = new HSFProfile<Matrix<double>>();
            if (Mdata.Count != 0)
            { // Are there any Profiles in there?
                if (Mdata.TryGetValue(key, out valueOut))
                { //see if our key is in there
                    if (Previous != null) // Check whether we are at the first state
                        return HSFProfile<Matrix<double>>.MergeProfiles(valueOut, Previous.GetFullProfile(key));
                    return valueOut;
                }
            }
            if (Previous != null)
                return Previous.GetFullProfile(key); // If no data, return profile from previous states
            return valueOut; //return empty profile
        }

        /// <summary>
        /// Sets the integer Profile in the state with its matching key. If a Profile is found already under
        /// that key, this will overwrite the old Profile.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="profIn"></param>
        public void SetProfile(StateVarKey<Matrix<double>> key, HSFProfile<Matrix<double>> profIn)
        {
            HSFProfile<Matrix<double>> valueOut;
            if (!Mdata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Mdata.Add(key, profIn);
            else { // Otherwise, erase whatever is there, and insert a new one.
                Mdata.Remove(key);
                Mdata.Add(key, profIn);
            }
        }

        /// <summary>
        /// Adds a Matrix Profile value pair to the state with the given key. If no Profile exists, a new Profile is created
        /// with the corresponding key. If a Profile exists with that key, the pair is appended onto the end of the Profile. </summary>
        /// Ensure that the Profile is still time ordered if this is the case.<param name="key"></param>
        /// <param name="pairIn"></param>
        public void AddValue(StateVarKey<Matrix<double>> key, KeyValuePair<double, Matrix<double>> pairIn)
        {
            HSFProfile<Matrix<double>> valueIn = new HSFProfile<Matrix<double>>(pairIn);
            HSFProfile<Matrix<double>> valueOut = new HSFProfile<Matrix<double>>();
            if (!Mdata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Mdata.Add(key, valueIn);
            else // Otherwise, add this data point to the existing Profile.
                valueOut.Add(pairIn); //TODO: make sure this is ok. was formally iterator.second.data
        }

        /// <summary>
        /// Adds a Matrix Profile value pair to the state with the given key. If no Profile exists, a new Profile is created
        /// with the corresponding key. If a Profile exists with that key, the pair is appended onto the end of the Profile. </summary>
        /// Ensure that the Profile is still time ordered if this is the case.<param name="key"></param>
        /// <param name="pairIn"></param>
        public void AddValue(StateVarKey<Matrix<double>> key, HSFProfile<Matrix<double>> profIn)
        {
            HSFProfile<Matrix<double>> valueOut;
            if (!Mdata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Mdata.Add(key, profIn);
            else // Otherwise, add this data point to the existing Profile.
                valueOut.Add(profIn);
        }
        //maybe add this functionality to subsystem? but then we would need to pass the state to the constructor
        public static SystemState setInitialSystemState(List<XmlNode> ICNodes, Asset asset)
        {
            // Set up Subsystem Nodes, first loop through the assets in the XML model input file
            //int n = modelInputXMLNode.ChildNode("ASSET");
            SystemState state = new SystemState();
            foreach (XmlNode ICNode in ICNodes)
            {
                string type = ICNode.Attributes["type"].Value;
                string key = asset.Name + "."+ICNode.Attributes["key"].Value.ToLower();
                if (type.Equals("Int"))
                {
                    int val;
                    Int32.TryParse(ICNode.Attributes["value"].Value, out val);
                    StateVarKey<int> svk = new StateVarKey<int>(key);
                    state.AddValue(svk, new KeyValuePair<double, int>(SimParameters.SimStartSeconds, val));
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
                    Double.TryParse(ICNode.Attributes["value"].Value, out val);
                    StateVarKey<double> svk = new StateVarKey<double>(key);
                    state.AddValue(svk, new KeyValuePair<double, double>(SimParameters.SimStartSeconds, val));
                }
                else if (type.Equals("Bool"))
                {
                    string val = ICNode.Attributes["value"].Value;
                    bool val_ = false;
                    if (val.Equals("True") || val.Equals("1"))
                        val_ = true;
                    StateVarKey<bool> svk = new StateVarKey<bool>(key);
                    state.AddValue(svk, new KeyValuePair<double, bool>(SimParameters.SimStartSeconds, val_));
                }
                else if (type.Equals("Matrix"))
                {
                    Matrix<double> val = new Matrix<double>(ICNode.Attributes["value"].Value);
                    StateVarKey<Matrix<double>> svk = new StateVarKey<Matrix<double>>(key);
                    state.AddValue(svk, new KeyValuePair<double, Matrix<double>>(SimParameters.SimStartSeconds, val));
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
