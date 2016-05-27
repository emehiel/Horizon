using System.Collections.Generic;
using Utilities;
using System;
using MissionElements;

namespace HSFSystem
{

    /// <summary>
    /// A singleton class to hold dependency functions for interpretting data between subsystems
    /// </summary>
    public class Dependency
    {
        //TODO(MORGAN): Make sure dependency can be singleton if it has these endstates and stateMap fields
        static Dependency _instance = null;
        //public List<SystemState> endStates {get; private set;}
        //private Dictionary<List<State>, int> stateMap;
        private Dictionary<string, Delegate> DependencyFunctions;

        private Dependency()
        {
            DependencyFunctions = new Dictionary<string, Delegate>();
           // endStates = new List<SystemState>();
        }
        /// <summary>
        /// Create a singleton instance of the Dependency dictionary
        /// </summary>
        public static Dependency Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new Dependency();
                }
                return _instance;
            }
        }
        /// <summary>
        /// Add a new dependency function and its call key to the dictionary. A new dependency 
        /// added with the same callkey will be overwritten
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="func"></param>
        public void Add(string callKey, Delegate func)
        {
            if (DependencyFunctions.ContainsKey(callKey))
                DependencyFunctions.Remove(callKey);
            DependencyFunctions.Add(callKey, func);
        }
        /// <summary>
        /// Retrieve a specific Delegate dependency function from the dictionary.
        /// </summary>
        /// <param name="callKey"></param>
        /// <returns></returns>
        public Delegate GetDependencyFunc(string callKey)
        {
            Delegate ret;
            if(DependencyFunctions.TryGetValue(callKey, out ret))
                return ret;
            throw new KeyNotFoundException();
        }

        /// <summary>
        /// Append a Dictionary of dependency functions to the already existing dictionary
        /// </summary>
        /// <param name="newDeps"></param>
        public void Append(Dictionary<string, Delegate> newDeps)
        {
            foreach (var dep in newDeps)
            {
                Add(dep.Key, dep.Value);
            }
        }
        /// <summary>
        /// Update the endStates Attribute of dependencies so that the dependency functions can have access
        /// to all states of the system.
        /// </summary>
        /// <param name="assetStates"></param>
        //public void UpdateStates(List<SystemState> assetStates)
        //{
        //    endStates.Clear();
        //    foreach (SystemState newState  in assetStates)
        //    {
        //        endStates.Add(newState);
        //    }
        //}
        //------------------------------------------------------------------------------------------------
        //--------------------------------- DECLARE DEPENDENCY COLLECTORS --------------------------------
        //------------------------------------------------------------------------------------------------
        /*Morgan Doesn't like this. This needs to be rethought out.
        Dependency collectors just need to cast to right type and accumulate if necessary

            public HSFProfile<double> Asset1_SSDRSUB_getNewDataProfile()
            {
                State state = endStates[0];
                return SSDRSUB_NewDataProfile_EOSENSORSUB(state);
            }
            public HSFProfile<double> Asset1_COMMSUB_getDataRateProfile()
            {
                State state = endStates[0];
                return COMMSUB_DataRateProfile_SSDRSUB(state);
            }
            public HSFProfile<double> Asset1_POWERSUB_getPowerProfile()
            {
                State state = endStates[0];
                HSFProfile<double> prof1 = POWERSUB_PowerProfile_ADCSSUB(state);
                HSFProfile<double> prof2 = POWERSUB_PowerProfile_EOSENSORSUB(state);
                HSFProfile<double> prof3 = POWERSUB_PowerProfile_SSDRSUB(state);
                HSFProfile<double> prof4 = POWERSUB_PowerProfile_COMMSUB(state);
                return (prof1 + prof2 + prof3 + prof4);
            }
            public HSFProfile<double> Asset2_SSDRSUB_getNewDataProfile()
            {
                State state = endStates[1];
                return SSDRSUB_NewDataProfile_EOSENSORSUB(state);
            }
            public HSFProfile<double> Asset2_COMMSUB_getDataRateProfile()
            {
                State state = endStates[1];
                return COMMSUB_DataRateProfile_SSDRSUB(state);
            }
            public HSFProfile<double> Asset2_POWERSUB_getPowerProfile()
            {
                State state = endStates[1];
                HSFProfile<double> prof1 = POWERSUB_PowerProfile_ADCSSUB(state);
                HSFProfile<double> prof2 = POWERSUB_PowerProfile_EOSENSORSUB(state);
                HSFProfile<double> prof3 = POWERSUB_PowerProfile_SSDRSUB(state);
                HSFProfile<double> prof4 = POWERSUB_PowerProfile_COMMSUB(state);
                return (prof1 + prof2 + prof3 + prof4);
            }
            */
            //------------------------------------------------------------------------------------------------
            //--------------------------------- DECLARE DEPENDENCY FUNCTIONS ---------------------------------
            //------------------------------------------------------------------------------------------------
            /* Morgan Doesn't like this. This should be rethought
            define dependency functions in their respective classes and subsystems add them so subsystemNode, subsystemNode adds to Dependency dictionary in this class
            public HSFProfile<double> SSDRSUB_NewDataProfile_EOSENSORSUB(State state)
            {
                StateVarKey<double> EOSENSORDATA(STATEVARNAME_PIXELS);
                return state.getProfile(EOSENSORDATA) / 500;
            }
            public HSFProfile<double> COMMSUB_DataRateProfile_SSDRSUB(State state)
            {
                StateVarKey<double> dataratio(STATEVARNAME_DATABUFFERRATIO);
                double datarate = 5000 * (state.getValueAtTime(dataratio, state._taskStart()).second - state.getValueAtTime(dataratio, state._taskEnd()).second) / (state._taskEnd() - state._taskStart());
                HSFProfile<double> prof1;
                if (datarate != 0)
                {
                    prof1[state->getTaskStart()] = datarate;
                    prof1[state->getTaskEnd()] = 0;
                }
                return prof1;
            }
            public HSFProfile<double> POWERSUB_PowerProfile_ADCSSUB(State state)
            {
                HSFProfile<double> prof1;
                prof1[state._eventStart()] = 40;
                prof1[state._taskStart()] = 60;
                prof1[state._taskEnd()] = 40;
                return prof1;
            }
            public HSFProfile<double> POWERSUB_PowerProfile_EOSENSORSUB(State state)
            {
                HSFProfile<double> prof1 = new HSFProfile<double>();
                prof1[state._eventStart()] = 10;
                StateVarKey<bool> eo3(STATEVARNAME_EOON);
                if (state->getValueAtTime(eo3, state._taskStart()).second)
                {
                    prof1[state._taskStart()] = 60;
                    prof1[state._taskEnd()] = 10;
                }
                return prof1;
            }
            public HSFProfile<double> POWERSUB_PowerProfile_SSDRSUB(State state)
            {
                HSFProfile<double> prof1;
                prof1[state._eventStart()] = 15;
                return prof1;
            }
            public HSFProfile<double> POWERSUB_PowerProfile_COMMSUB(State state)
            {
                StateVarKey<double> c1(STATEVARNAME_DATARATE);
                return state.getProfile(c1) * 20;
            }

            // Do not edit code below this line
            public static Dependencies Instance() { }
            public static Dependencies Instance(int iNum) { }
            public State getState(int assetnum) { }
            public State getAssetState(int assetnum) { }
            //TODO: Need to figure out what these functions are supposed to do
            //    public void updateState(const List<State> newStates) { }
            //   public void updateStates(int thread, const List<State> newStates) { }

            private static Dependencies pinstance;
            private Dictionary<Dependencies, int> pinstances
            */
    }
}

