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
        public SystemState PreviousState { get; set; } = null;

        /** The Dictionary of integer Profiles. */
        public Dictionary<StateVariableKey<int>, HSFProfile<int>> Idata { get; private set; } = new Dictionary<StateVariableKey<int>, HSFProfile<int>>();

        /** The Dictionary of double precision Profiles. */
        public Dictionary<StateVariableKey<double>, HSFProfile<double>> Ddata { get; private set; } = new Dictionary<StateVariableKey<double>, HSFProfile<double>>();

        /** The Dictionary of boolean Profiles. */
        public Dictionary<StateVariableKey<bool>, HSFProfile<bool>> Bdata { get; private set; } = new Dictionary<StateVariableKey<bool>, HSFProfile<bool>>();

        /** The Dictionary of Matrix Profiles. */
        public Dictionary<StateVariableKey<Matrix<double>>, HSFProfile<Matrix<double>>> Mdata { get; private set; } = new Dictionary<StateVariableKey<Matrix<double>>, HSFProfile<Matrix<double>>>();

        /** The Dictionary of Quaternion Profiles. */
        public Dictionary<StateVariableKey<Quaternion>, HSFProfile<Quaternion>> Qdata { get; private set; } = new Dictionary<StateVariableKey<Quaternion>, HSFProfile<Quaternion>>();

        /** The Dictionary of Quaternion Profiles. */
        public Dictionary<StateVariableKey<Vector>, HSFProfile<Vector>> Vdata { get; private set; } = new Dictionary<StateVariableKey<Vector>, HSFProfile<Vector>>();

        /// <summary>
        /// Creates an initial state   
        /// </summary>
        public SystemState()
        {
            PreviousState = null;
        }


       /// <summary>
       /// Create a new state from a previous one
       /// </summary>
       /// <param name="previous"></param>
        public SystemState(SystemState previousState)
        {
            PreviousState = previousState;
        }
        /// <summary>
        /// Use this constructor to create a copy of another state
        /// </summary>
        /// <param name="state"></param>
        /// <param name="copy"> Only used to distinguish constructor calls by prototype</param>
        public SystemState(SystemState state, bool copy)
        {
            PreviousState = state.PreviousState;
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
                AddValues(data.Key, data.Value);
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
                stateData += data.Key.VariableName + "," + data.Value.ToString();
            }
            return stateData;
        }

        /// <summary>
        ///  Gets the last int value set for the given state variable key in the state. If no value is found
        ///  it checks the previous state, continuing all the way to the initial state.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public (double Time, int Value) GetLastValue(StateVariableKey<int> key) { //TODO Test
            HSFProfile<int> valueOut;
            if (Idata.Count != 0) { // Are there any Profiles in there?
                if (Idata.TryGetValue(key, out valueOut)) //see if our key is in there
                    return valueOut.Last(); //found it, return it TODO: return last value or pair?
            }
            return PreviousState.GetLastValue(key); //either no profiles or none that match our keys, try finding it in the previous one
        }

        /// <summary>
        ///  Gets the integer value of the state at a certain time.
        ///  If the exact time is not found, the data is assumed to be on a zero-order hold, and the last value set is found.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public (double Time, int Value) GetValueAtTime(StateVariableKey<int> key, double time) {
            HSFProfile<int> valueOut;
            if (Idata.Count != 0) { // Are there any Profiles in there?
                if (Idata.TryGetValue(key, out valueOut) && Idata[key].LastTime() <= time) //see if our key is in there
                    return valueOut.DataAtTime(time);
            }
            return PreviousState.GetValueAtTime(key, time); //either no profiles or none that match our keys, try finding it in the previous one
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
                return PreviousState.GetProfile(_key); // This isn't the right profile, go back one and try it out!
            }
            else if (_key.GetType() == typeof(StateVariableKey<double>))
           {
                HSFProfile<double> valueOut;
               if (Ddata.Count != 0)
               { // Are there any Profiles in there?
                   if (Ddata.TryGetValue(_key, out valueOut)) //see if our key is in there
                       return (dynamic)valueOut;
               }
               return PreviousState.GetProfile(_key);  // This isn't the right profile, go back one and try it out!
            }
           else if (_key.GetType() == typeof(StateVariableKey<bool>))
           {
               HSFProfile<bool> valueOut;
               if (Ddata.Count != 0)
               { // Are there any Profiles in there?
                   if (Bdata.TryGetValue(_key, out valueOut)) //see if our key is in there
                       return (dynamic)valueOut;
               }
                return PreviousState.GetProfile(_key); // This isn't the right profile, go back one and try it out!
            }
            else if (_key.GetType() == typeof(StateVariableKey<Matrix<double>>))
           {
               HSFProfile<Matrix<double>> valueOut;
               if (Ddata.Count != 0)
               { // Are there any Profiles in there?
                   if (Mdata.TryGetValue(_key, out valueOut)) //see if our key is in there
                       return (dynamic)valueOut;
               }
               return PreviousState.GetProfile(_key); // This isn't the right profile, go back one and try it out!

            }
            else if (_key.GetType() == typeof(StateVariableKey<Quaternion>))
           {
               HSFProfile<Quaternion> valueOut;
               if (Ddata.Count != 0)
               { // Are there any Profiles in there?
                   if (Qdata.TryGetValue(_key, out valueOut)) //see if our key is in there
                       return (dynamic)valueOut;
               }
               return PreviousState.GetProfile(_key); // This isn't the right profile, go back one and try it out!
            }
            throw new ArgumentException("Profile Type Not Found");
       }

        /// <summary>
        /// Returns the integer Profile for this state and all previous states merged into one Profile
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public HSFProfile<int> GetFullProfile(StateVariableKey<int> key) {
            HSFProfile<int> valueOut = new HSFProfile<int>();
            if (Idata.Count != 0) { // Are there any Profiles in there?
                if (Idata.TryGetValue(key, out valueOut)) { //see if our key is in there
                    if (PreviousState != null) // Check whether we are at the first state
                        return HSFProfile<int>.MergeProfiles(valueOut, PreviousState.GetFullProfile(key));
                    return valueOut;
                }
            }
            if (PreviousState != null)
                return PreviousState.GetFullProfile(key); // If no data, return profile from previous states
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
        public void AddValue(StateVariableKey<int> stateVariableKey, double time, int stateValue)
        {
            if (Idata.TryGetValue(stateVariableKey, out HSFProfile<int> valueOut)) // If there's a Profile matching that key,add this data point to the existing Profile.
                valueOut[time] = stateValue;
            else // Otherwise, insert a new one.
                Idata[stateVariableKey] = new HSFProfile<int>(time, stateValue);
        }
        public void AddValue(StateVariableKey<double> stateVariableKey, double time, double stateValue)
        {
            if (Ddata.TryGetValue(stateVariableKey, out HSFProfile<double> valueOut)) // If there's a Profile matching that key,add this data point to the existing Profile.
                valueOut[time] = stateValue;
            else // Otherwise, insert a new one.
                Ddata[stateVariableKey] = new HSFProfile<double>(time, stateValue);
        }
        public void AddValue(StateVariableKey<bool> stateVariableKey, double time, bool stateValue)
        {
            if (Bdata.TryGetValue(stateVariableKey, out HSFProfile<bool> valueOut)) // If there's a Profile matching that key,add this data point to the existing Profile.
                valueOut[time] = stateValue;
            else // Otherwise, insert a new one.
                Bdata[stateVariableKey] = new HSFProfile<bool>(time, stateValue);
        }
        public void AddValue(StateVariableKey<Matrix<double>> stateVariableKey, double time, Matrix<double> stateValue)
        {
            if (Mdata.TryGetValue(stateVariableKey, out HSFProfile<Matrix<double>> valueOut)) // If there's a Profile matching that key,add this data point to the existing Profile.
                valueOut[time] = stateValue;
            else // Otherwise, insert a new one.
                Mdata[stateVariableKey] = new HSFProfile<Matrix<double>>(time, stateValue);
        }
        public void AddValue(StateVariableKey<Quaternion> stateVariableKey, double time, Quaternion stateValue)
        {
            if (Qdata.TryGetValue(stateVariableKey, out HSFProfile<Quaternion> valueOut)) // If there's a Profile matching that key,add this data point to the existing Profile.
                valueOut[time] = stateValue;
            else // Otherwise, insert a new one.
                Qdata[stateVariableKey] = new HSFProfile<Quaternion>(time, stateValue);
        }
        public void AddValue(StateVariableKey<Vector> stateVariableKey, double time, Vector stateValue)
        {
            if (Vdata.TryGetValue(stateVariableKey, out HSFProfile<Vector> valueOut)) // If there's a Profile matching that key,add this data point to the existing Profile.
                valueOut[time] = stateValue;
            else // Otherwise, insert a new one.
                Vdata[stateVariableKey] = new HSFProfile<Vector>(time, stateValue);
        }
        /// <summary>
        /// Adds values to an existing integer state variable.  If the state variable key does not exist
        /// in Idata, a new HSFProfile<int> is added to Idata with the values in stateValues
        /// </summary>
        /// <param name="stateVariableKey">The StateVariableKey for the Idata</param>
        /// <param name="stateValues">The integer values to add to the state stored in an HSFProfile<int></param>
        public void AddValues(StateVariableKey<int> stateVariableKey, HSFProfile<int> stateValues)
        {
            foreach (var stateValue in stateValues.Data)
                AddValue(stateVariableKey, stateValue.Key, stateValue.Value);
        }
        /// <summary>
        ///  Gets the last int value set for the given state variable key in the state. If no value is found
        ///  it checks the previous state, continuing all the way to the initial state.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public (double Time, double Value) GetLastValue(StateVariableKey<double> key) {
            HSFProfile<double> valueOut;
            if (Ddata.Count != 0) { // Are there any Profiles in there?
                if (Ddata.TryGetValue(key, out valueOut)) //see if our key is in there
                    return valueOut.Last(); //found it, return it TODO: return last value or pair?
            }
            return PreviousState.GetLastValue(key); //either no profiles or none that match our keys, try finding it in the previous one
        }

        /// <summary>
        ///  Gets the integer value of the state at a certain time.
        ///  If the exact time is not found, the data is assumed to be on a zero-order hold, and the last value set is found.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public (double Time, double Value) GetValueAtTime(StateVariableKey<double> key, double time) {
            HSFProfile<double> valueOut;
            if (Ddata.Count != 0) { // Are there any Profiles in there?
                if (Ddata.TryGetValue(key, out valueOut) && Ddata[key].LastTime() <= time) //see if our key is in there
                    return valueOut.DataAtTime(time);
            }
            return PreviousState.GetValueAtTime(key, time); //either no profiles or none that match our keys, try finding it in the previous one
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
                    if (PreviousState != null) // Check whether we are at the first state
                        return HSFProfile<double>.MergeProfiles(valueOut, PreviousState.GetFullProfile(key));
                    return valueOut;
                }
            }
            if (PreviousState != null)
                return PreviousState.GetFullProfile(key); // If no data, return profile from previous states
            return valueOut; //return empty profile
        }

        /// <summary>
        /// Sets the integer Profile in the state with its matching key. If a Profile is found already under
        /// that key, this will overwrite the old Profile.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="profIn"></param>
        public void SetProfile(StateVariableKey<double> key, HSFProfile<double> profIn)
        {
            HSFProfile<double> valueOut;
            if (!Ddata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Ddata.Add(key, profIn);
            else
            { // Otherwise, erase whatever is there, and insert a new one.
                Ddata.Remove(key);
                Ddata.Add(key, profIn);
            }
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
        public (double Time, bool Value) GetLastValue(StateVariableKey<bool> key)
        {
            HSFProfile<bool> valueOut;
            if (Bdata.Count != 0)
            { // Are there any Profiles in there?
                if (Bdata.TryGetValue(key, out valueOut)) //see if our key is in there
                    return valueOut.Last(); //found it, return it TODO: return last value or pair?
            }
            return PreviousState.GetLastValue(key); //either no profiles or none that match our keys, try finding it in the previous one
        }

        /// <summary>
        ///  Gets the integer value of the state at a certain time.
        ///  If the exact time is not found, the data is assumed to be on a zero-order hold, and the last value set is found.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public (double Time, bool Value) GetValueAtTime(StateVariableKey<bool> key, double time)
        {
            HSFProfile<bool> valueOut;
            if (Bdata.Count != 0)
            { // Are there any Profiles in there?
                if (Bdata.TryGetValue(key, out valueOut) && Bdata[key].LastTime() <= time) //see if our key is in there
                    return valueOut.DataAtTime(time);
            }
            return PreviousState.GetValueAtTime(key, time); //either no profiles or none that match our keys, try finding it in the previous one
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
                    if (PreviousState != null) // Check whether we are at the first state
                        return HSFProfile<bool>.MergeProfiles(valueOut, PreviousState.GetFullProfile(key));
                    return valueOut;
                }
            }
            if (PreviousState != null)
                return PreviousState.GetFullProfile(key); // If no data, return profile from previous states
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
        public (double Time, Matrix<double> Value) GetLastValue(StateVariableKey<Matrix<double>> key)
        {
            HSFProfile<Matrix<double>> valueOut;
            if (Mdata.Count != 0)
            { // Are there any Profiles in there?
                if (Mdata.TryGetValue(key, out valueOut)) //see if our key is in there
                    return valueOut.Last(); //found it, return it TODO: return last value or pair?
            }
            return PreviousState.GetLastValue(key); //either no profiles or none that match our keys, try finding it in the previous one
        }

        /// <summary>
        ///  Gets the integer value of the state at a certain time.
        ///  If the exact time is not found, the data is assumed to be on a zero-order hold, and the last value set is found.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public (double Time, Matrix<double> Value) GetValueAtTime(StateVariableKey<Matrix<double>> key, double time)
        {
            HSFProfile<Matrix<double>> valueOut;
            if (Mdata.Count != 0)
            { // Are there any Profiles in there?
                if (Mdata.TryGetValue(key, out valueOut) && Mdata[key].LastTime() <= time) //see if our key is in there
                    return valueOut.DataAtTime(time);
            }
            return PreviousState.GetValueAtTime(key, time); //either no profiles or none that match our keys, try finding it in the previous one
        }

        /// <summary>
        ///  Gets the integer value of the state at a certain time.
        ///  If the exact time is not found, the data is assumed to be on a zero-order hold, and the last value set is found.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public (double Time, Vector Value) GetValueAtTime(StateVariableKey<Vector> key, double time)
        {
            HSFProfile<Vector> valueOut;
            if (Vdata.Count != 0)
            { // Are there any Profiles in there?
                if (Vdata.TryGetValue(key, out valueOut) && Vdata[key].LastTime() <= time) //see if our key is in there
                    return valueOut.DataAtTime(time);
            }
            return PreviousState.GetValueAtTime(key, time); //either no profiles or none that match our keys, try finding it in the previous one
        }
        /// <summary>
        ///  Gets the integer value of the state at a certain time.
        ///  If the exact time is not found, the data is assumed to be on a zero-order hold, and the last value set is found.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public (double Time, Quaternion Value) GetValueAtTime(StateVariableKey<Quaternion> key, double time)
        {
            HSFProfile<Quaternion> valueOut;
            if (Qdata.Count != 0)
            { // Are there any Profiles in there?
                if (Qdata.TryGetValue(key, out valueOut) && Qdata[key].LastTime() <= time) //see if our key is in there
                    return valueOut.DataAtTime(time);
            }
            return PreviousState.GetValueAtTime(key, time); //either no profiles or none that match our keys, try finding it in the previous one
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
                    if (PreviousState != null) // Check whether we are at the first state
                        return HSFProfile<Matrix<double>>.MergeProfiles(valueOut, PreviousState.GetFullProfile(key));
                    return valueOut;
                }
            }
            if (PreviousState != null)
                return PreviousState.GetFullProfile(key); // If no data, return profile from previous states
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
        public void AddValue(StateVariableKey<Quaternion> key, HSFProfile<Quaternion> profIn)
        {
            HSFProfile<Quaternion> valueOut;
            if (!Qdata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Qdata.Add(key, profIn);
            else // Otherwise, add this data point to the existing Profile.
                valueOut.Add(profIn);
        }

        public void SetProfile(StateVariableKey<Vector> key, HSFProfile<Vector> profIn)
        {
            HSFProfile<Vector> valueOut;
            if (!Vdata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Vdata.Add(key, profIn);
            else
            { // Otherwise, erase whatever is there, and insert a new one.
                Vdata.Remove(key);
                Vdata.Add(key, profIn);
            }
        }

        /// <summary>
        /// Adds a Matrix Profile value pair to the state with the given key. If no Profile exists, a new Profile is created
        /// with the corresponding key. If a Profile exists with that key, the pair is appended onto the end of the Profile. </summary>
        /// Ensure that the Profile is still time ordered if this is the case.<param name="key"></param>
        /// <param name="pairIn"></param>
        public void AddValue(StateVariableKey<Vector> key, HSFProfile<Vector> profIn)
        {
            HSFProfile<Vector> valueOut;
            if (!Vdata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
                Vdata.Add(key, profIn);
            else // Otherwise, add this data point to the existing Profile.
                valueOut.Add(profIn);
        }

        public void SetInitialSystemState(XmlNode ICNode, Asset asset)
        {
            string type = ICNode.Attributes["type"].Value;
            string stateVariableName = asset.Name + "." + ICNode.Attributes["key"].Value.ToLower(); // This may be changing to not use asset.name
            double time = SimParameters.SimStartSeconds;

            if (type.ToLower().Equals("int") || type.ToLower().Equals("integer"))
            {
                Int32.TryParse(ICNode.Attributes["value"].Value, out int stateValue);
                AddValue(new StateVariableKey<int>(stateVariableName), time, stateValue);
            }
            else if (type.ToLower().Equals("double"))
            {
                Double.TryParse(ICNode.Attributes["value"].Value, out double stateValue);
                AddValue(new StateVariableKey<double>(stateVariableName), time, stateValue);
            }
            else if (type.ToLower().Equals("bool"))
            {
                string val = ICNode.Attributes["value"].Value;
                bool stateValue = false;
                if (val.ToLower().Equals("true") || val.Equals("1"))
                    stateValue = true;
                AddValue(new StateVariableKey<bool>(stateVariableName), time, stateValue);
            }
            else if (type.ToLower().Equals("matrix"))
            {
                Matrix<double> stateValue = new Matrix<double>(ICNode.Attributes["value"].Value);
                AddValue(new StateVariableKey<Matrix<double>>(stateVariableName), time, stateValue);
            }
            else if (type.ToLower().Equals("quat") || type.ToLower().Equals("quaternion"))
            {
                Quaternion stateValue = new Quaternion(ICNode.Attributes["value"].Value);
                AddValue(new StateVariableKey<Quaternion>(stateVariableName), time, stateValue);
            }
            else if (type.ToLower().Equals("vector"))
            {
                Vector stateValue = new Vector(ICNode.Attributes["value"].Value);
                AddValue(new StateVariableKey<Vector>(stateVariableName), time, stateValue);
            }
            else
            {
                Console.WriteLine($"State variable {stateVariableName} of type {type} is not supported by HSF.");
            }
        }
        
    }
}
