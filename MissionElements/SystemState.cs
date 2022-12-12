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
        public Dictionary<StateVariableKey<int>, HSFProfile<int>> Idata { get; private set; } = new Dictionary<StateVariableKey<int>, HSFProfile<int>>();

        /** The Dictionary of double precision Profiles. */
        public Dictionary<StateVariableKey<double>, HSFProfile<double>> Ddata { get; private set; } = new Dictionary<StateVariableKey<double>, HSFProfile<double>>();

        /** The Dictionary of floating point value Profiles. */
        //   public Dictionary<StateVarKey<float>, HSFProfile<float>> Fdata { get; private set; }

        /** The Dictionary of boolean Profiles. */
        public Dictionary<StateVariableKey<bool>, HSFProfile<bool>> Bdata { get; private set; } = new Dictionary<StateVariableKey<bool>, HSFProfile<bool>>();

        /** The Dictionary of Matrix<double> Profiles. */
        public Dictionary<StateVariableKey<Matrix<double>>, HSFProfile<Matrix<double>>> Mdata { get; private set; } = new Dictionary<StateVariableKey<Matrix<double>>, HSFProfile<Matrix<double>>>();

        /** The Dictionary of Quaternion Profiles. */
        public Dictionary<StateVariableKey<Quaternion>, HSFProfile<Quaternion>> Qdata { get; private set; } = new Dictionary<StateVariableKey<Quaternion>, HSFProfile<Quaternion>>();


        /// <summary>
        /// Creates an initial state   
        /// </summary>
        public SystemState()
        {
            Previous = null;
            Idata = new Dictionary<StateVariableKey<int>, HSFProfile<int>>();
            Ddata = new Dictionary<StateVariableKey<double>, HSFProfile<double>>();
            Bdata = new Dictionary<StateVariableKey<bool>, HSFProfile<bool>>();
            Mdata = new Dictionary<StateVariableKey<Matrix<double>>, HSFProfile<Matrix<double>>>();
            Qdata = new Dictionary<StateVariableKey<Quaternion>, HSFProfile<Quaternion>>();
        }


       /// <summary>
       /// Create a new state from a previous one
       /// </summary>
       /// <param name="previous"></param>
        public SystemState(SystemState previous)
        {
            Previous = previous;
            Idata = new Dictionary<StateVariableKey<int>, HSFProfile<int>>();
            Ddata = new Dictionary<StateVariableKey<double>, HSFProfile<double>>();
            Bdata = new Dictionary<StateVariableKey<bool>, HSFProfile<bool>>();
            Mdata = new Dictionary<StateVariableKey<Matrix<double>>, HSFProfile<Matrix<double>>>();
            Qdata = new Dictionary<StateVariableKey<Quaternion>, HSFProfile<Quaternion>>();
        }
        /// <summary>
        /// Use this constructor to create a copy of another state
        /// </summary>
        /// <param name="state"></param>
        /// <param name="copy"> Only used to distinguish constructor calls by prototype</param>
        public SystemState(SystemState state, int copy)
        {
            Previous = state.Previous;
            Idata = new Dictionary<StateVariableKey<int>, HSFProfile<int>>(state.Idata);
            Ddata = new Dictionary<StateVariableKey<double>, HSFProfile<double>>(state.Ddata);
            Bdata = new Dictionary<StateVariableKey<bool>, HSFProfile<bool>>(state.Bdata);
            Mdata = new Dictionary<StateVariableKey<Matrix<double>>, HSFProfile<Matrix<double>>>(state.Mdata);
            Qdata = new Dictionary<StateVariableKey<Quaternion>, HSFProfile<Quaternion>>(state.Qdata);
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
            foreach (var data in moreState.Qdata)
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
        public KeyValuePair<double, int> GetLastValue(StateVariableKey<int> key) { //TODO Test
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
        public KeyValuePair<double, int> GetValueAtTime(StateVariableKey<int> key, double time) {
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
        public HSFProfile<T> GetProfile<T>(StateVariableKey<T> _key)
       {
           if (_key.GetType() == typeof(StateVariableKey<int>))
           {
               HSFProfile<int> valueOut;
               if (Idata.Count != 0)
               { // Are there any Profiles in there?
                   if (Idata.TryGetValue(_key, out valueOut)) //see if our key is in there
                       return (dynamic)valueOut;
               }
                return Previous.GetProfile(_key); // This isn't the right profile, go back one and try it out!
            }
            else if (_key.GetType() == typeof(StateVariableKey<double>))
           {
                HSFProfile<double> valueOut;
               if (Ddata.Count != 0)
               { // Are there any Profiles in there?
                   if (Ddata.TryGetValue(_key, out valueOut)) //see if our key is in there
                       return (dynamic)valueOut;
               }
               return Previous.GetProfile(_key);  // This isn't the right profile, go back one and try it out!
            }
           else if (_key.GetType() == typeof(StateVariableKey<bool>))
           {
               HSFProfile<bool> valueOut;
               if (Ddata.Count != 0)
               { // Are there any Profiles in there?
                   if (Bdata.TryGetValue(_key, out valueOut)) //see if our key is in there
                       return (dynamic)valueOut;
               }
                return Previous.GetProfile(_key); // This isn't the right profile, go back one and try it out!
            }
            else if (_key.GetType() == typeof(StateVariableKey<Matrix<double>>))
           {
               HSFProfile<Matrix<double>> valueOut;
               if (Ddata.Count != 0)
               { // Are there any Profiles in there?
                   if (Mdata.TryGetValue(_key, out valueOut)) //see if our key is in there
                       return (dynamic)valueOut;
               }
               return Previous.GetProfile(_key); // This isn't the right profile, go back one and try it out!

            }
            else if (_key.GetType() == typeof(StateVariableKey<Quaternion>))
           {
               HSFProfile<Quaternion> valueOut;
               if (Ddata.Count != 0)
               { // Are there any Profiles in there?
                   if (Qdata.TryGetValue(_key, out valueOut)) //see if our key is in there
                       return (dynamic)valueOut;
               }
               return Previous.GetProfile(_key); // This isn't the right profile, go back one and try it out!
            }
            return Previous.GetProfile(_key);  // This isn't the right profile, go back one and try it out!
        }

        ///////////  NOT REFERENCED OUTSIDE METHOD - REMOVE?  /////////////////////
        /// <summary>
        /// Returns the integer Profile for this state and all previous states merged into one Profile
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public HSFProfile<int> GetFullProfile(StateVariableKey<int> key) {
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
        public void SetProfile(StateVariableKey<int> key, HSFProfile<int> profIn) {
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
        void AddValue(StateVariableKey<int> key, KeyValuePair<double, int> pairIn) {
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
        public void AddValue(StateVariableKey<int> key, HSFProfile<int> profIn) {
            HSFProfile<int> valueOut;
            if (!Idata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Idata.Add(key, profIn);
            else // Otherwise, add this data point to the existing Profile.
                valueOut.Add(profIn);
        }

        // TODO:  Which AddValues() should we use, both?
        public void AddValues(StateVariableKey<int> stateVariableKey, List<(double Time, int Value)> stateValues)
        {
            foreach (var value in stateValues)
                AddValue(stateVariableKey, new KeyValuePair<double, int>(value.Time, value.Value));
        }
        /// <summary>
        ///  Gets the last int value set for the given state variable key in the state. If no value is found
        ///  it checks the previous state, continuing all the way to the initial state.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValuePair<double, double> GetLastValue(StateVariableKey<double> key) {
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
        public KeyValuePair<double, double> GetValueAtTime(StateVariableKey<double> key, double time) {
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
        public HSFProfile<double> GetFullProfile(StateVariableKey<double> key) {
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
        public void SetProfile(StateVariableKey<double> key, HSFProfile<double> profIn) {
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
        public void AddValue(StateVariableKey<double> key, KeyValuePair<double, double> pairIn) {
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
        public void AddValue(StateVariableKey<double> key, HSFProfile<double> profIn) {
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
        public KeyValuePair<double, bool> GetLastValue(StateVariableKey<bool> key)
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
        public KeyValuePair<double, bool> GetValueAtTime(StateVariableKey<bool> key, double time)
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
        public HSFProfile<bool> GetFullProfile(StateVariableKey<bool> key)
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
        public void SetProfile(StateVariableKey<bool> key, HSFProfile<bool> profIn)
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
        public void AddValue(StateVariableKey<bool> key, KeyValuePair<double, bool> pairIn)
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
        public void AddValue(StateVariableKey<bool> key, HSFProfile<bool> profIn)
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
        public KeyValuePair<double, Matrix<double>> GetLastValue(StateVariableKey<Matrix<double>> key)
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
        public KeyValuePair<double, Matrix<double>> GetValueAtTime(StateVariableKey<Matrix<double>> key, double time)
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
        public HSFProfile<Matrix<double>> GetFullProfile(StateVariableKey<Matrix<double>> key)
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
        public void SetProfile(StateVariableKey<Matrix<double>> key, HSFProfile<Matrix<double>> profIn)
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
        public void AddValue(StateVariableKey<Matrix<double>> key, KeyValuePair<double, Matrix<double>> pairIn)
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
        public void AddValue(StateVariableKey<Matrix<double>> key, HSFProfile<Matrix<double>> profIn)
        {
            HSFProfile<Matrix<double>> valueOut;
            if (!Mdata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Mdata.Add(key, profIn);
            else // Otherwise, add this data point to the existing Profile.
                valueOut.Add(profIn);
        }

        public void SetProfile(StateVariableKey<Quaternion> key, HSFProfile<Quaternion> profIn)
        {
            HSFProfile<Quaternion> valueOut;
            if (!Qdata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Qdata.Add(key, profIn);
            else
            { // Otherwise, erase whatever is there, and insert a new one.
                Qdata.Remove(key);
                Qdata.Add(key, profIn);
            }
        }

        /// <summary>
        /// Adds a Matrix Profile value pair to the state with the given key. If no Profile exists, a new Profile is created
        /// with the corresponding key. If a Profile exists with that key, the pair is appended onto the end of the Profile. </summary>
        /// Ensure that the Profile is still time ordered if this is the case.<param name="key"></param>
        /// <param name="pairIn"></param>
        public void AddValue(StateVariableKey<Quaternion> key, KeyValuePair<double, Quaternion> pairIn)
        {
            HSFProfile<Quaternion> valueIn = new HSFProfile<Quaternion>(pairIn);
            HSFProfile<Quaternion> valueOut = new HSFProfile<Quaternion>();
            if (!Qdata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Qdata.Add(key, valueIn);
            else // Otherwise, add this data point to the existing Profile.
                valueOut.Add(pairIn); //TODO: make sure this is ok. was formally iterator.second.data
        }

        /// <summary>
        /// Adds a Matrix Profile value pair to the state with the given key. If no Profile exists, a new Profile is created
        /// with the corresponding key. If a Profile exists with that key, the pair is appended onto the end of the Profile. </summary>
        /// Ensure that the Profile is still time ordered if this is the case.<param name="key"></param>
        /// <param name="pairIn"></param>
        public void AddValue(StateVariableKey<Quaternion> key, HSFProfile<Quaternion> profIn)
        {
            HSFProfile<Quaternion> valueOut;
            if (!Qdata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Qdata.Add(key, profIn);
            else // Otherwise, add this data point to the existing Profile.
                valueOut.Add(profIn);
        }

        public static SystemState SetInitialSystemState(XmlNode ICNode, Asset asset)
        {
            // Set up Subsystem Nodes, first loop through the assets in the XML model input file

            // TODO:  Why is this a static method passing back a SystemState?  Shouldn't it just update the IC of the SystemState

            SystemState state = new SystemState();

            string type = ICNode.Attributes["type"].Value;
            string key = asset.Name + "."+ICNode.Attributes["key"].Value.ToLower();
            if (type.Equals("Int"))
            {
                int val;
                Int32.TryParse(ICNode.Attributes["value"].Value, out val);
                StateVariableKey<int> svk = new StateVariableKey<int>(key);
                state.AddValue(svk, new KeyValuePair<double, int>(SimParameters.SimStartSeconds, val));
            }
            else if (type.Equals("Double"))
            {
                double val;
                Double.TryParse(ICNode.Attributes["value"].Value, out val);
                StateVariableKey<double> svk = new StateVariableKey<double>(key);
                state.AddValue(svk, new KeyValuePair<double, double>(SimParameters.SimStartSeconds, val));
            }
            else if (type.Equals("Bool"))
            {
                string val = ICNode.Attributes["value"].Value;
                bool val_ = false;
                if (val.ToLower().Equals("true") || val.Equals("1"))
                    val_ = true;
                StateVariableKey<bool> svk = new StateVariableKey<bool>(key);
                state.AddValue(svk, new KeyValuePair<double, bool>(SimParameters.SimStartSeconds, val_));
            }
            else if (type.Equals("Matrix"))
            {
                Matrix<double> val = new Matrix<double>(ICNode.Attributes["value"].Value);
                StateVariableKey<Matrix<double>> svk = new StateVariableKey<Matrix<double>>(key);
                state.AddValue(svk, new KeyValuePair<double, Matrix<double>>(SimParameters.SimStartSeconds, val));
            }
            else if (type.Equals("Quat"))
            {
                Quaternion val = new Quaternion(ICNode.Attributes["value"].Value);
                StateVariableKey<Quaternion>svk = new StateVariableKey<Quaternion>(key);
                state.AddValue(svk, new KeyValuePair<double, Quaternion>(SimParameters.SimStartSeconds, val));
            }
            return state;
        }
        
    }
}
