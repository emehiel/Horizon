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
        #region Properties
        /** The previous state, upon which this state is based */
        public SystemState PreviousState { get; set; } = null;

        /** The Dictionary of integer Profiles. */
        public Dictionary<StateVariableKey<int>, HSFProfile<int>> Idata { get; private set; } = new Dictionary<StateVariableKey<int>, HSFProfile<int>>();

        /** The Dictionary of double precision Profiles. */
        public Dictionary<StateVariableKey<double>, HSFProfile<double>> Ddata { get; private set; } = new Dictionary<StateVariableKey<double>, HSFProfile<double>>();

        /** The Dictionary of boolean Profiles. */
        public Dictionary<StateVariableKey<bool>, HSFProfile<bool>> Bdata { get; private set; } = new Dictionary<StateVariableKey<bool>, HSFProfile<bool>>();

        /** The Dictionary of Matrix<double> Profiles. */
        public Dictionary<StateVariableKey<Matrix<double>>, HSFProfile<Matrix<double>>> Mdata { get; private set; } = new Dictionary<StateVariableKey<Matrix<double>>, HSFProfile<Matrix<double>>>();

        /** The Dictionary of Quaternion Profiles. */
        public Dictionary<StateVariableKey<Quaternion>, HSFProfile<Quaternion>> Qdata { get; private set; } = new Dictionary<StateVariableKey<Quaternion>, HSFProfile<Quaternion>>();

        /** The Dictionary of Vector Profiles. */
        public Dictionary<StateVariableKey<Vector>, HSFProfile<Vector>> Vdata { get; private set; } = new Dictionary<StateVariableKey<Vector>, HSFProfile<Vector>>();

        #endregion

        #region Constructors
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
            Vdata = new Dictionary<StateVariableKey<Vector>, HSFProfile<Vector>>(state.Vdata);
        }

        #endregion

        //TODO:  ONly used in unit testing
        /// <summary>
        /// combine two system states by adding the states from one into the other
        /// </summary>
        /// <param name="moreState"></param>
        public void Add(SystemState moreState)
        {
            foreach (var data in moreState.Idata)
                AddValues(data.Key, data.Value);
            foreach (var data in moreState.Ddata)
                AddValues(data.Key, data.Value);
            foreach (var data in moreState.Bdata)
                AddValues(data.Key, data.Value);
            foreach (var data in moreState.Mdata)
                AddValues(data.Key, data.Value);
            foreach (var data in moreState.Qdata)
                AddValues(data.Key, data.Value);
            foreach (var data in moreState.Vdata)
                AddValues(data.Key, data.Value);
        }

        #region Overrides
        public override string ToString()
        {
            string stateData = "";
            foreach(var data in Idata)
            {
                stateData += data.Key.VariableName + "," + data.Value.ToString();
            }
            return stateData;
        }
        #endregion

        #region GetLastValue
        /// <summary>
        ///  Gets the last integer value set for the given state variable key in the SystemState. If no value is found
        ///  it checks the previous SystemState, continuing all the way to the initial SystemState.
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
        ///  Gets the last double value set for the given state variable key in the SystemState. If no value is found
        ///  it checks the previous SystemState, continuing all the way to the initial SystemState.
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
        ///  Gets the last boolean value set for the given state variable key in the SystemState. If no value is found
        ///  it checks the previous SystemState, continuing all the way to the initial SystemState.
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
        ///  Gets the last Matrix<double> value set for the given state variable key in the SystemState. If no value is found
        ///  it checks the previous SystemState, continuing all the way to the initial SystemState.
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
        ///  Gets the last Vector value set for the given state variable key in the SystemState. If no value is found
        ///  it checks the previous SystemState, continuing all the way to the initial SystemState.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public (double Time, Vector Value) GetLastValue(StateVariableKey<Vector> key)
        {
            HSFProfile<Vector> valueOut;
            if (Vdata.Count != 0)
            { // Are there any Profiles in there?
                if (Vdata.TryGetValue(key, out valueOut)) //see if our key is in there
                    return valueOut.Last(); //found it, return it TODO: return last value or pair?
            }
            return PreviousState.GetLastValue(key); //either no profiles or none that match our keys, try finding it in the previous one
        }

        /// <summary>
        ///  Gets the last Matrix<double> value set for the given state variable key in the SystemState. If no value is found
        ///  it checks the previous SystemState, continuing all the way to the initial SystemState.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public (double Time, Quaternion Value) GetLastValue(StateVariableKey<Quaternion> key)
        {
            HSFProfile<Quaternion> valueOut;
            if (Qdata.Count != 0)
            { // Are there any Profiles in there?
                if (Qdata.TryGetValue(key, out valueOut)) //see if our key is in there
                    return valueOut.Last(); //found it, return it TODO: return last value or pair?
            }
            return PreviousState.GetLastValue(key); //either no profiles or none that match our keys, try finding it in the previous one
        }
        #endregion

        #region GetValueAtTime

        /// <summary>
        ///  Gets the integer value of the SystemState at a certain time.
        ///  If the exact time is not found, the data is assumed to be on a zero-order hold.
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
        ///  Gets the double value of the SystemState at a certain time.
        ///  If the exact time is not found, the data is assumed to be on a zero-order hold.        /// </summary>
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
        ///  Gets the boolean value of the SystemState at a certain time.
        ///  If the exact time is not found, the data is assumed to be on a zero-order hold.
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
        ///  Gets the Matrix(double) value of the SystemState at a certain time.
        ///  If the exact time is not found, the data is assumed to be on a zero-order hold.
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
        ///  Gets the vector value of the SystemState at a certain time.
        ///  If the exact time is not found, the data is assumed to be on a zero-order hold.
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
        ///  Gets the quaternion value of the SystemState at a certain time.
        ///  If the exact time is not found, the data is assumed to be on a zero-order hold.
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
        #endregion

        #region GetProfile
        /// <summary>
        /// Returns the integer Profile matching the key given. If no Profile is found, it goes back one Event
        /// and checks again until it gets to the initial state.
        /// </summary>
        /// <typeparam name="T">The type of the profile we are getting</typeparam>
        /// <param name="_key">The state variable key that is being looked up.</param>
        /// <returns>Profile saved in the state.</returns>
        public HSFProfile<int> GetProfile(StateVariableKey<int> _key)
        {
            if (Idata.Count != 0)
            { // Are there any Profiles in there?
                if (Idata.TryGetValue(_key, out HSFProfile<int> valueOut)) //see if our key is in there
                    return valueOut;
            }
            return PreviousState.GetProfile(_key); // This isn't the right profile, go back one and try it out!
        }
        public HSFProfile<double> GetProfile(StateVariableKey<double> _key)
        {
            if (Ddata.Count != 0)
            { // Are there any Profiles in there?
                if (Ddata.TryGetValue(_key, out HSFProfile<double> valueOut)) //see if our key is in there
                    return valueOut;
            }
            return PreviousState.GetProfile(_key);  // This isn't the right profile, go back one and try it out!
        }

        public HSFProfile<bool> GetProfile(StateVariableKey<bool> _key)
        {
            if (Bdata.Count != 0)
            { // Are there any Profiles in there?
                if (Bdata.TryGetValue(_key, out HSFProfile<bool> valueOut)) //see if our key is in there
                    return valueOut;
            }
            return PreviousState.GetProfile(_key);  // This isn't the right profile, go back one and try it out!
        }

        public HSFProfile<Matrix<double>> GetProfile(StateVariableKey<Matrix<double>> _key)
        {
            if (Mdata.Count != 0)
            { // Are there any Profiles in there?
                if (Mdata.TryGetValue(_key, out HSFProfile<Matrix<double>> valueOut)) //see if our key is in there
                    return valueOut;
            }
            return PreviousState.GetProfile(_key);  // This isn't the right profile, go back one and try it out!
        }

        public HSFProfile<Quaternion> GetProfile(StateVariableKey<Quaternion> _key)
        {
            if (Qdata.Count != 0)
            { // Are there any Profiles in there?
                if (Qdata.TryGetValue(_key, out HSFProfile<Quaternion> valueOut)) //see if our key is in there
                    return valueOut;
            }
            return PreviousState.GetProfile(_key);  // This isn't the right profile, go back one and try it out!
        }

        public HSFProfile<Vector> GetProfile(StateVariableKey<Vector> _key)
        {
            if (Vdata.Count != 0)
            { // Are there any Profiles in there?
                if (Vdata.TryGetValue(_key, out HSFProfile<Vector> valueOut)) //see if our key is in there
                    return valueOut;
            }
            return PreviousState.GetProfile(_key);  // This isn't the right profile, go back one and try it out!
        }

        #endregion

        #region GetFullProfile

        /////////////  NOT REFERENCED OUTSIDE METHOD - REMOVE?  /////////////////////
        ///// <summary>
        ///// Returns the integer Profile for this state and all previous states merged into one Profile
        ///// </summary>
        ///// <param name="key"></param>
        ///// <returns></returns>
        //public HSFProfile<int> GetFullProfile(StateVariableKey<int> key) {
        //    HSFProfile<int> valueOut = new HSFProfile<int>();
        //    if (Idata.Count != 0) { // Are there any Profiles in there?
        //        if (Idata.TryGetValue(key, out valueOut)) { //see if our key is in there
        //            if (PreviousState != null) // Check whether we are at the first state
        //                return HSFProfile<int>.MergeProfiles(valueOut, PreviousState.GetFullProfile(key));
        //            return valueOut;
        //        }
        //    }
        //    if (PreviousState != null)
        //        return PreviousState.GetFullProfile(key); // If no data, return profile from previous states
        //    return valueOut; //return empty profile
        //}

        ///// <summary>
        ///// Returns the integer Profile for this state and all previous states merged into one Profile
        ///// </summary>
        ///// <param name="key"></param>
        ///// <returns></returns>
        //public HSFProfile<double> GetFullProfile(StateVariableKey<double> key) {
        //    HSFProfile<double> valueOut = new HSFProfile<double>();
        //    if (Ddata.Count != 0) { // Are there any Profiles in there?
        //        if (Ddata.TryGetValue(key, out valueOut)) { //see if our key is in there
        //            if (PreviousState != null) // Check whether we are at the first state
        //                return HSFProfile<double>.MergeProfiles(valueOut, PreviousState.GetFullProfile(key));
        //            return valueOut;
        //        }
        //    }
        //    if (PreviousState != null)
        //        return PreviousState.GetFullProfile(key); // If no data, return profile from previous states
        //    return valueOut; //return empty profile
        //}
        ///// <summary>
        ///// Returns the integer Profile for this state and all previous states merged into one Profile
        ///// </summary>
        ///// <param name="key"></param>
        ///// <returns></returns>
        //public HSFProfile<bool> GetFullProfile(StateVariableKey<bool> key)
        //{
        //    HSFProfile<bool> valueOut = new HSFProfile<bool>();
        //    if (Bdata.Count != 0)
        //    { // Are there any Profiles in there?
        //        if (Bdata.TryGetValue(key, out valueOut))
        //        { //see if our key is in there
        //            if (PreviousState != null) // Check whether we are at the first state
        //                return HSFProfile<bool>.MergeProfiles(valueOut, PreviousState.GetFullProfile(key));
        //            return valueOut;
        //        }
        //    }
        //    if (PreviousState != null)
        //        return PreviousState.GetFullProfile(key); // If no data, return profile from previous states
        //    return valueOut; //return empty profile
        //}
        
        ///// <summary>
        ///// Returns the integer Profile for this state and all previous states merged into one Profile
        ///// </summary>
        ///// <param name="key"></param>
        ///// <returns></returns>
        //public HSFProfile<Matrix<double>> GetFullProfile(StateVariableKey<Matrix<double>> key)
        //{
        //    HSFProfile<Matrix<double>> valueOut = new HSFProfile<Matrix<double>>();
        //    if (Mdata.Count != 0)
        //    { // Are there any Profiles in there?
        //        if (Mdata.TryGetValue(key, out valueOut))
        //        { //see if our key is in there
        //            if (PreviousState != null) // Check whether we are at the first state
        //                return HSFProfile<Matrix<double>>.MergeProfiles(valueOut, PreviousState.GetFullProfile(key));
        //            return valueOut;
        //        }
        //    }
        //    if (PreviousState != null)
        //        return PreviousState.GetFullProfile(key); // If no data, return profile from previous states
        //    return valueOut; //return empty profile
        //}

        ///// <summary>
        ///// Returns the integer Profile for this state and all previous states merged into one Profile
        ///// </summary>
        ///// <param name="key"></param>
        ///// <returns></returns>
        //public HSFProfile<Vector> GetFullProfile(StateVariableKey<Vector> key)
        //{
        //    HSFProfile<Vector> valueOut = new HSFProfile<Vector>();
        //    if (Vdata.Count != 0)
        //    { // Are there any Profiles in there?
        //        if (Vdata.TryGetValue(key, out valueOut))
        //        { //see if our key is in there
        //            if (PreviousState != null) // Check whether we are at the first state
        //                return HSFProfile<Vector>.MergeProfiles(valueOut, PreviousState.GetFullProfile(key));
        //            return valueOut;
        //        }
        //    }
        //    if (PreviousState != null)
        //        return PreviousState.GetFullProfile(key); // If no data, return profile from previous states
        //    return valueOut; //return empty profile
        //}

        ///// <summary>
        ///// Returns the qu aternionProfile for this state and all previous states merged into one Profile
        ///// </summary>
        ///// <param name="key"></param>
        ///// <returns></returns>
        //public HSFProfile<Quaternion> GetFullProfile(StateVariableKey<Quaternion> key)
        //{
        //    HSFProfile<Quaternion> valueOut = new HSFProfile<Quaternion>();
        //    if (Qdata.Count != 0)
        //    { // Are there any Profiles in there?
        //        if (Qdata.TryGetValue(key, out valueOut))
        //        { //see if our key is in there
        //            if (PreviousState != null) // Check whether we are at the first state
        //                return HSFProfile<Quaternion>.MergeProfiles(valueOut, PreviousState.GetFullProfile(key));
        //            return valueOut;
        //        }
        //    }
        //    if (PreviousState != null)
        //        return PreviousState.GetFullProfile(key); // If no data, return profile from previous states
        //    return valueOut; //return empty profile
        //}
        #endregion

        #region SetProfile
        // Eric Removed these because we do not want to overrite profile data (8/16/22).  If a user overwrites profiles, they are doing something wrong...
        // TODO:  On that note, do we want to let the user overwrite a SINGLE VALUE at a SINGLE TIME?

        ///// <summary>
        ///// Sets the integer Profile in the state with its matching key. If a Profile is found already under
        ///// that key, this will overwrite the old Profile.
        ///// </summary>
        ///// <param name="key"></param>
        ///// <param name="profIn"></param>
        //public void SetProfile(StateVariableKey<int> key, HSFProfile<int> profIn) {
        //    HSFProfile<int> valueOut;
        //    if (!Idata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
        //        Idata.Add(key, profIn);
        //    else { // Otherwise, erase whatever is there, and insert a new one.
        //        Idata.Remove(key);
        //        Idata.Add(key, profIn);
        //    }
        //}

        ///// <summary>
        ///// Sets the integer Profile in the state with its matching key. If a Profile is found already under
        ///// that key, this will overwrite the old Profile.
        ///// </summary>
        ///// <param name="key"></param>
        ///// <param name="profIn"></param>
        //public void SetProfile(StateVariableKey<double> key, HSFProfile<double> profIn)
        //{
        //    HSFProfile<double> valueOut;
        //    if (!Ddata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
        //        Ddata.Add(key, profIn);
        //    else
        //    { // Otherwise, erase whatever is there, and insert a new one.
        //        Ddata.Remove(key);
        //        Ddata.Add(key, profIn);
        //    }
        //}

        ///// <summary>
        ///// Sets the integer Profile in the state with its matching key. If a Profile is found already under
        ///// that key, this will overwrite the old Profile.
        ///// </summary>
        ///// <param name="key"></param>
        ///// <param name="profIn"></param>
        //public void SetProfile(StateVariableKey<bool> key, HSFProfile<bool> profIn)
        //{
        //    HSFProfile<bool> valueOut;
        //    if (!Bdata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
        //        Bdata.Add(key, profIn);
        //    else { // Otherwise, erase whatever is there, and insert a new one.
        //        Bdata.Remove(key);
        //        Bdata.Add(key, profIn);
        //    }
        //}

        ///// <summary>
        ///// Sets the integer Profile in the state with its matching key. If a Profile is found already under
        ///// that key, this will overwrite the old Profile.
        ///// </summary>
        ///// <param name="key"></param>
        ///// <param name="profIn"></param>
        //public void SetProfile(StateVariableKey<Matrix<double>> key, HSFProfile<Matrix<double>> profIn)
        //{
        //    HSFProfile<Matrix<double>> valueOut;
        //    if (!Mdata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
        //        Mdata.Add(key, profIn);
        //    else { // Otherwise, erase whatever is there, and insert a new one.
        //        Mdata.Remove(key);
        //        Mdata.Add(key, profIn);
        //    }
        //}

        //public void SetProfile(StateVariableKey<Quaternion> key, HSFProfile<Quaternion> profIn)
        //{
        //    HSFProfile<Quaternion> valueOut;
        //    if (!Qdata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
        //        Qdata.Add(key, profIn);
        //    else
        //    { // Otherwise, erase whatever is there, and insert a new one.
        //        Qdata.Remove(key);
        //        Qdata.Add(key, profIn);
        //    }
        //}

        //public void SetProfile(StateVariableKey<Vector> key, HSFProfile<Vector> profIn)
        //{
        //    HSFProfile<Vector> valueOut;
        //    if (!Vdata.TryGetValue(key, out valueOut)) // If there's no Profile matching that key, insert a new one.
        //        Vdata.Add(key, profIn);
        //    else
        //    { // Otherwise, erase whatever is there, and insert a new one.
        //        Vdata.Remove(key);
        //        Vdata.Add(key, profIn);
        //    }
        //}
        #endregion

        #region AddValue (Key, Time, Value)
        /// <summary>
        /// Adds a integer (time, value) pair to the SystemState with the given key. If no Profile exists, a new Profile is created
        /// with the corresponding key. If a Profile exists with that key, the pair is added into the of the Profile in time order.
        /// <param name="key"></param>
        /// <param name="pairIn"></param>
        public void AddValue(StateVariableKey<int> stateVariableKey, double time, int stateValue)
        {
            if (Idata.TryGetValue(stateVariableKey, out HSFProfile<int> valueOut)) // If there's a Profile matching that key,add this data point to the existing Profile.
            {
                if (valueOut.LastTime() < time)  // Only let the user add data points that are after the last data point
                    valueOut[time] = stateValue;
                else if (valueOut.Data.ContainsKey(time))
                {
                    string msg = $"Cannot overwrite SystemState Values '{stateVariableKey.ToString()}', at {time}, with value {stateValue}";
                    throw new ArgumentOutOfRangeException(msg);
                }
                else
                {
                    string msg = $"Cannot insert SystemState Values before last value (causality) '{stateVariableKey.ToString()}', at {time}, with value {stateValue}";
                    throw new ArgumentOutOfRangeException(msg);
                }
            }
            else // Otherwise, insert a new one.
                Idata[stateVariableKey] = new HSFProfile<int>(time, stateValue);
        }
        /// <summary>
        /// Adds a double (time, value) pair to the SystemState with the given key. If no Profile exists, a new Profile is created
        /// with the corresponding key. If a Profile exists with that key, the pair is added into the of the Profile in time order.
        /// </summary>
        /// <param name="stateVariableKey"></param>
        /// <param name="time"></param>
        /// <param name="stateValue"></param>
        public void AddValue(StateVariableKey<double> stateVariableKey, double time, double stateValue)
        {
            if (Ddata.TryGetValue(stateVariableKey, out HSFProfile<double> valueOut)) // If there's a Profile matching that key,add this data point to the existing Profile.
            {
                if (valueOut.LastTime() < time)  // Only let the user add data points that are after the last data point
                    valueOut[time] = stateValue;
                else if (valueOut.LastTime() == time)
                {
                    string msg = $"Cannot overwrite SystemState Values {stateVariableKey}, at {time}, with value {stateValue}";
                    throw new ArgumentOutOfRangeException(msg);
                }
                else
                {
                    string msg = $"Cannot insert SystemState Values before last value (causality) {stateVariableKey}, at {time}, with value {stateValue}";
                    throw new ArgumentOutOfRangeException(msg);
                }
            }
            else // Otherwise, insert a new one.
                Ddata[stateVariableKey] = new HSFProfile<double>(time, stateValue);
        }
        /// <summary>
        /// Adds a boolean (time, value) pair to the SystemState with the given key. If no Profile exists, a new Profile is created
        /// with the corresponding key. If a Profile exists with that key, the pair is added into the of the Profile in time order.
        /// </summary>
        /// <param name="stateVariableKey"></param>
        /// <param name="time"></param>
        /// <param name="stateValue"></param>
        public void AddValue(StateVariableKey<bool> stateVariableKey, double time, bool stateValue)
        {
            if (Bdata.TryGetValue(stateVariableKey, out HSFProfile<bool> valueOut)) // If there's a Profile matching that key,add this data point to the existing Profile.
            {
                if (valueOut.LastTime() < time)  // Only let the user add data points that are after the last data point
                    valueOut[time] = stateValue;
                else if (valueOut.LastTime() == time)
                {
                    string msg = $"Cannot overwrite SystemState Values {stateVariableKey}, at {time}, with value {stateValue}";
                    throw new ArgumentOutOfRangeException(msg);
                }
                else
                {
                    string msg = $"Cannot insert SystemState Values before last value (causality) {stateVariableKey}, at {time}, with value {stateValue}";
                    throw new ArgumentOutOfRangeException(msg);
                }
            }
            else // Otherwise, insert a new one.
                Bdata[stateVariableKey] = new HSFProfile<bool>(time, stateValue);
        }

        /// <summary>
        /// Adds a Matrix(double) (time, value) pair to the SystemState with the given key. If no Profile exists, a new Profile is created
        /// with the corresponding key. If a Profile exists with that key, the pair is added into the of the Profile in time order.
        /// </summary>
        /// <param name="stateVariableKey"></param>
        /// <param name="time"></param>
        /// <param name="stateValue"></param>
        public void AddValue(StateVariableKey<Matrix<double>> stateVariableKey, double time, Matrix<double> stateValue)
        {
            if (Mdata.TryGetValue(stateVariableKey, out HSFProfile<Matrix<double>> valueOut)) // If there's a Profile matching that key,add this data point to the existing Profile.
            {
                if (valueOut.LastTime() < time)  // Only let the user add data points that are after the last data point
                    valueOut[time] = stateValue;
                else if (valueOut.LastTime() == time)
                {
                    string msg = $"Cannot overwrite SystemState Values {stateVariableKey}, at {time}, with value {stateValue}";
                    throw new ArgumentOutOfRangeException(msg);
                }
                else
                {
                    string msg = $"Cannot insert SystemState Values before last value (causality) {stateVariableKey}, at {time}, with value {stateValue}";
                    throw new ArgumentOutOfRangeException(msg);
                }
            }
            else // Otherwise, insert a new one.
                Mdata[stateVariableKey] = new HSFProfile<Matrix<double>>(time, stateValue);
        }

        /// <summary>
        /// Adds a Quaternion (time, value) pair to the SystemState with the given key. If no Profile exists, a new Profile is created
        /// with the corresponding key. If a Profile exists with that key, the pair is added into the of the Profile in time order.
        /// </summary>
        /// <param name="stateVariableKey"></param>
        /// <param name="time"></param>
        /// <param name="stateValue"></param>
        public void AddValue(StateVariableKey<Quaternion> stateVariableKey, double time, Quaternion stateValue)
        {
            if (Qdata.TryGetValue(stateVariableKey, out HSFProfile<Quaternion> valueOut)) // If there's a Profile matching that key,add this data point to the existing Profile.
            {
                if (valueOut.LastTime() < time)  // Only let the user add data points that are after the last data point
                    valueOut[time] = stateValue;
                else if (valueOut.LastTime() == time)
                {
                    string msg = $"Cannot overwrite SystemState Values {stateVariableKey}, at {time}, with value {stateValue}";
                    throw new ArgumentOutOfRangeException(msg);
                }
                else
                {
                    string msg = $"Cannot insert SystemState Values before last value (causality) {stateVariableKey}, at {time}, with value {stateValue}";
                    throw new ArgumentOutOfRangeException(msg);
                }
            }
            else // Otherwise, insert a new one.
                Qdata[stateVariableKey] = new HSFProfile<Quaternion>(time, stateValue);
        }

        /// <summary>
        /// Adds a Vector (time, value) pair to the SystemState with the given key. If no Profile exists, a new Profile is created
        /// with the corresponding key. If a Profile exists with that key, the pair is added into the of the Profile in time order.
        /// </summary>
        /// <param name="stateVariableKey"></param>
        /// <param name="time"></param>
        /// <param name="stateValue"></param>
        public void AddValue(StateVariableKey<Vector> stateVariableKey, double time, Vector stateValue)
        {
            if (Vdata.TryGetValue(stateVariableKey, out HSFProfile<Vector> valueOut)) // If there's a Profile matching that key,add this data point to the existing Profile.
            {
                if (valueOut.LastTime() < time)  // Only let the user add data points that are after the last data point
                    valueOut[time] = stateValue;
                else if (valueOut.LastTime() == time)
                {
                    string msg = $"Cannot overwrite SystemState Values {stateVariableKey}, at {time}, with value {stateValue}";
                    throw new ArgumentOutOfRangeException(msg);
                }
                else
                {
                    string msg = $"Cannot insert SystemState Values before last value (causality) {stateVariableKey}, at {time}, with value {stateValue}";
                    throw new ArgumentOutOfRangeException(msg);
                }
            }
            else // Otherwise, insert a new one.
                Vdata[stateVariableKey] = new HSFProfile<Vector>(time, stateValue);
        }

        #endregion

        #region AddValues (key, HSFProfile)

        // TODO:  May want to get ride of these so users can work with single (time, value) pairs, and list of (time, value) pairs
        /// <summary>
        /// Adds integer (time, value) pair to an existing SystemState variable from the input HSFProfile.  If the state variable key does not exist
        /// a new HSFProfile is added with the values in stateValues.
        /// </summary>
        /// <param name="stateVariableKey">The StateVariableKey for the Idata</param>
        /// <param name="stateValues">The integer values to add to the state stored in an HSFProfile<int></param>
        public void AddValues(StateVariableKey<int> stateVariableKey, HSFProfile<int> stateValues)
        {
            foreach (var stateValue in stateValues.Data)
                AddValue(stateVariableKey, stateValue.Key, stateValue.Value);
        }

        /// <summary>
        /// Adds double (time, value) pair to an existing SystemState variable from the input HSFProfile.  If the state variable key does not exist
        /// a new HSFProfile is added with the values in stateValues.
        /// </summary>
        /// <param name="stateVariableKey">The StateVariableKey for the Idata</param>
        /// <param name="stateValues">The integer values to add to the state stored in an HSFProfile<int></param>
        public void AddValues(StateVariableKey<double> stateVariableKey, HSFProfile<double> stateValues) {
            foreach (var stateValue in stateValues.Data)
                AddValue(stateVariableKey, stateValue.Key, stateValue.Value);
        }

        /// <summary>
        /// Adds boolean (time, value) pair to an existing SystemState variable from the input HSFProfile.  If the state variable key does not exist
        /// a new HSFProfile is added with the values in stateValues.
        /// </summary>
        /// <param name="stateVariableKey">The StateVariableKey for the Idata</param>
        /// <param name="stateValues">The integer values to add to the state stored in an HSFProfile<int></param>
        public void AddValues(StateVariableKey<bool> stateVariableKey, HSFProfile<bool> stateValues)
        {
            foreach (var stateValue in stateValues.Data)
                AddValue(stateVariableKey, stateValue.Key, stateValue.Value);
        }

        /// <summary>
        /// Adds Matrix<double> (time, value) pair to an existing SystemState variable from the input HSFProfile.  If the state variable key does not exist
        /// a new HSFProfile is added with the values in stateValues.
        /// </summary>
        /// <param name="stateVariableKey">The StateVariableKey for the Idata</param>
        /// <param name="stateValues">The integer values to add to the state stored in an HSFProfile<int></param>
        public void AddValues(StateVariableKey<Matrix<double>> stateVariableKey, HSFProfile<Matrix<double>> stateValues)
        {
            foreach (var stateValue in stateValues.Data)
                AddValue(stateVariableKey, stateValue.Key, stateValue.Value);
        }

        /// <summary>
        /// Adds Quaterion (time, value) pair to an existing SystemState variable from the input HSFProfile.  If the state variable key does not exist
        /// a new HSFProfile is added with the values in stateValues.
        /// </summary>
        /// <param name="stateVariableKey">The StateVariableKey for the Idata</param>
        /// <param name="stateValues">The integer values to add to the state stored in an HSFProfile<int></param>
        public void AddValues(StateVariableKey<Quaternion> stateVariableKey, HSFProfile<Quaternion> stateValues)
        {
            foreach (var stateValue in stateValues.Data)
                AddValue(stateVariableKey, stateValue.Key, stateValue.Value);
        }

        /// <summary>
        /// Adds Vector (time, value) pair to an existing SystemState variable from the input HSFProfile.  If the state variable key does not exist
        /// a new HSFProfile is added with the values in stateValues.
        /// </summary>
        /// <param name="stateVariableKey">The StateVariableKey for the Idata</param>
        /// <param name="stateValues">The integer values to add to the state stored in an HSFProfile<int></param>
        public void AddValues(StateVariableKey<Vector> stateVariableKey, HSFProfile<Vector> stateValues)
        {
            foreach (var stateValue in stateValues.Data)
                AddValue(stateVariableKey, stateValue.Key, stateValue.Value);
        }
        #endregion

        #region AddValues (Key, List(Time, Value))
        // TODO:  Which AddValues() should we use, both?
        /// <summary>
        /// Adds integer (time, value) pairs to an existing SystemState variable from the input list(time, value) pairs.  If the state variable key does not exist
        /// a new HSFProfile is added with the values in stateValues.
        /// </summary>
        /// <param name="stateVariableKey"></param>
        /// <param name="stateValues"></param>
        public void AddValues(StateVariableKey<int> stateVariableKey, List<(double Time, int Value)> stateValues)
        {
            foreach (var (time, value) in stateValues)
                AddValue(stateVariableKey, time, value);
        }

        /// <summary>
        /// Adds double (time, value) pairs to an existing SystemState variable from the input list(time, value) pairs.  If the state variable key does not exist
        /// a new HSFProfile is added with the values in stateValues.
        /// </summary>
        /// <param name="stateVariableKey"></param>
        /// <param name="stateValues"></param>
        public void AddValues(StateVariableKey<double> stateVariableKey, List<(double Time, double Value)> stateValues)
        {
            foreach (var (time, value) in stateValues)
                AddValue(stateVariableKey, time, value);
        }

        /// <summary>
        /// Adds boolean (time, value) pairs to an existing SystemState variable from the input list(time, value) pairs.  If the state variable key does not exist
        /// a new HSFProfile is added with the values in stateValues.
        /// </summary>
        /// <param name="stateVariableKey"></param>
        /// <param name="stateValues"></param>
        public void AddValues(StateVariableKey<bool> stateVariableKey, List<(double Time, bool Value)> stateValues)
        {
            foreach (var (time, value) in stateValues)
                AddValue(stateVariableKey, time, value);
        }

        /// <summary>
        /// Adds Matrix<double> (time, value) pairs to an existing SystemState variable from the input list(time, value) pairs.  If the state variable key does not exist
        /// a new HSFProfile is added with the values in stateValues.
        /// </summary>
        /// <param name="stateVariableKey"></param>
        /// <param name="stateValues"></param>
        public void AddValues(StateVariableKey<Matrix<double>> stateVariableKey, List<(double Time, Matrix<double> Value)> stateValues)
        {
            foreach (var (time, value) in stateValues)
                AddValue(stateVariableKey, time, value);
        }

        /// <summary>
        /// Adds Quaternion (time, value) pairs to an existing SystemState variable from the input list(time, value) pairs.  If the state variable key does not exist
        /// a new HSFProfile is added with the values in stateValues.
        /// </summary>
        /// <param name="stateVariableKey"></param>
        /// <param name="stateValues"></param>
        public void AddValues(StateVariableKey<Quaternion> stateVariableKey, List<(double Time, Quaternion Value)> stateValues)
        {
            foreach (var (time, value) in stateValues)
                AddValue(stateVariableKey, time, value);
        }

        /// <summary>
        /// Adds Vector (time, value) pairs to an existing SystemState variable from the input list(time, value) pairs.  If the state variable key does not exist
        /// a new HSFProfile is added with the values in stateValues.
        /// </summary>
        /// <param name="stateVariableKey"></param>
        /// <param name="stateValues"></param>
        public void AddValues(StateVariableKey<Vector> stateVariableKey, List<(double Time, Vector Value)> stateValues)
        {
            foreach (var (time, value) in stateValues)
                AddValue(stateVariableKey, time, value);
        }

        #endregion



        public void SetInitialSystemState(XmlNode StateNode, string keyName)
        {
            string type = StateNode.Attributes["type"].Value.ToLower();
            //string stateVariableName = asset.Name + "." + ICNode.Attributes["key"].Value.ToLower(); // This may be changing to not use asset.name
            double time = SimParameters.SimStartSeconds;

            if (type.ToLower().Equals("int") || type.ToLower().Equals("integer"))
            {
                Int32.TryParse(StateNode.Attributes["value"].Value, out int stateValue);
                AddValue(new StateVariableKey<int>(keyName), time, stateValue);
            }
            else if (type.ToLower().Equals("double"))
            {
                Double.TryParse(StateNode.Attributes["value"].Value, out double stateValue);
                AddValue(new StateVariableKey<double>(keyName), time, stateValue);
            }
            else if (type.ToLower().Equals("bool"))
            {
                string val = StateNode.Attributes["value"].Value;
                bool stateValue = false;
                if (val.ToLower().Equals("true") || val.Equals("1"))
                    stateValue = true;
                AddValue(new StateVariableKey<bool>(keyName), time, stateValue);
            }
            else if (type.ToLower().Equals("matrix"))
            {
                Matrix<double> stateValue = new Matrix<double>(StateNode.Attributes["value"].Value);
                AddValue(new StateVariableKey<Matrix<double>>(keyName), time, stateValue);
            }
            else if (type.ToLower().Equals("quat") || type.ToLower().Equals("quaternion"))
            {
                Quaternion stateValue = new Quaternion(StateNode.Attributes["value"].Value);
                AddValue(new StateVariableKey<Quaternion>(keyName), time, stateValue);
            }
            else if (type.ToLower().Equals("vector"))
            {
                Vector stateValue = new Vector(StateNode.Attributes["value"].Value);
                AddValue(new StateVariableKey<Vector>(keyName), time, stateValue);
            }
            else
            {
                Console.WriteLine($"State variable {keyName} of type {type} is not supported by HSF.");
            }
        }
    }
}
