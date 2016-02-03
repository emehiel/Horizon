using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System_H;

namespace Subsystem{
    
/**
 * A class for interpreting data between Subsystems. State data between Subsystems can only be exchanged 
 * through a dependency. Dependencies have two parts. The dependency collector, and the dependency
 * functions. 
 
 * A dependency collector is called within the canPerform in a subsystem that depends on
 * other Subsystems. It collects the data from all its associated dependency functions

 * A dependency function is named in a specific convention following the order: sub1 type2 sub3
 * For example, for the above naming convention the function name would be: sub1type2sub3()

 * The appropriate way to read a dependency function is as follows: The dependency function sub1type2sub3
 * interprets data from what sub3 set to the state and changes it into data format type2, which sub1
 * is dependent on for its operation and understands what to do with.
 *
 * <I>For a complete example let's pretend we have 2 Subsystems, mixingbowl and cookiesheet. When
 * mixingbowl runs, it goes first, and it only cares about the volume of dough contained within.
 * It sets to the state doughvolume. Now cookiesheet has to run, but to calculate it's size it needs the
 * area the cookies are going to take up. The only way it can tell how much area it needs is to
 * do some operation on the mixingbowl's doughvolume state variable. But it can't ask mixingbowl directly,
 * and it <B>CERTAINLY</B> can't just ask the state what mixingbowl set directly. It's none of
 * cookiesheet's business after all... So, it needs to ask a neutral third party (read collector
 * function) that mediates between different Subsystems. That neutral party might be called 
 * COOKIESHEET_COLLECT_AREAS(). COOKIESHEET_COLLECT_AREAS() knows a dependency function COOKIESHEETareaMIXINGBOWL()
 * which understands how to convert directly from the states that mixingbowl sets to the state in terms of
 * volume and output it as a suggested area.
 
 * In this way, everyone is kept completely modular so that if a new subsystem (chocolatechipsbag) decided 
 * to come in at a later time or on a later batch of cookies, all that is needed is to add another function
 * COOKIESHEETareaCHOCOLATECHIPSBAG, and to call it inside COOKIESHEET_COLLECT_AREAS(), and then the result 
 * will be interpreted to cookiesheet in a way it already knows how to deal with! No need to disturb cookiesheet 
 * because someone else joined the party!!!</I>
 *
 * @author Brian Butler
 * @author Cory O'Connor
 * @author Travis Lockyer
 */
 
    class Dependencies{
        private List<State> endStates;
        private Dictionary<List<State>, int> stateMap;
        protected Dependencies(){}
        protected Dependencies(const Dependencies){}
        //------------------------------------------------------------------------------------------------
	    //--------------------------------- DECLARE DEPENDENCY COLLECTORS --------------------------------
	    //------------------------------------------------------------------------------------------------

	    public Profile<double> Asset1_SSDRSUB_getNewDataProfile(){
            State state = endStates[0];
            return SSDRSUB_NewDataProfile_EOSENSORSUB(state);
        }
        public Profile<double> Asset1_COMMSUB_getDataRateProfile(){
            State state = endStates[0];
            return COMMSUB_DataRateProfile_SSDRSUB(state);
        }
        public Profile<double> Asset1_POWERSUB_getPowerProfile(){
            State state = endStates[0];
            Profile<double> prof1 = POWERSUB_PowerProfile_ADCSSUB(state);
	        Profile<double> prof2 = POWERSUB_PowerProfile_EOSENSORSUB(state);
        	Profile<double> prof3 = POWERSUB_PowerProfile_SSDRSUB(state);
        	Profile<double> prof4 = POWERSUB_PowerProfile_COMMSUB(state);
	        return (prof1 + prof2 + prof3 + prof4);
        }
        public Profile<double> Asset2_SSDRSUB_getNewDataProfile(){
            State state = endStates[1];
            return SSDRSUB_NewDataProfile_EOSENSORSUB(state);
        }
        public Profile<double> Asset2_COMMSUB_getDataRateProfile(){
            State state = endStates[1];
            return COMMSUB_DataRateProfile_SSDRSUB(state);
        }
        public Profile<double> Asset2_POWERSUB_getPowerProfile(){
            State state = endStates[1];
            Profile<double> prof1 = POWERSUB_PowerProfile_ADCSSUB(state);
	        Profile<double> prof2 = POWERSUB_PowerProfile_EOSENSORSUB(state);
        	Profile<double> prof3 = POWERSUB_PowerProfile_SSDRSUB(state);
        	Profile<double> prof4 = POWERSUB_PowerProfile_COMMSUB(state);
        	return (prof1 + prof2 + prof3 + prof4);
        }

	    //------------------------------------------------------------------------------------------------
	    //--------------------------------- DECLARE DEPENDENCY FUNCTIONS ---------------------------------
	    //------------------------------------------------------------------------------------------------
        public Profile<double> SSDRSUB_NewDataProfile_EOSENSORSUB(State* state){
            StateVarKey<double> EOSENSORDATA(STATEVARNAME_PIXELS);
            return state.getProfile(EOSENSORDATA)/500;
        }
        public Profile<double> COMMSUB_DataRateProfile_SSDRSUB(State* state){
            StateVarKey<double> dataratio(STATEVARNAME_DATABUFFERRATIO);
	        double datarate = 5000*(state.getValueAtTime(dataratio, state._taskStart()).second - state.getValueAtTime(dataratio, state._taskEnd()).second)/(state._taskEnd() - state._taskStart());
        	Profile<double> prof1;
        	if(datarate != 0) {
        		prof1[state->getTaskStart()] = datarate;
        		prof1[state->getTaskEnd()] = 0;
        	}
        	return prof1;
        }
        public Profile<double> POWERSUB_PowerProfile_ADCSSUB(State* state){
            Profile<double> prof1;
	        prof1[state._eventStart()] = 40;
        	prof1[state._taskStart()] = 60;
        	prof1[state._taskEnd()] = 40;
        	return prof1;
        }
        public Profile<double> POWERSUB_PowerProfile_EOSENSORSUB(State* state){
            	Profile<double> prof1;
            	prof1[state._eventStart()] = 10;
            	StateVarKey<bool> eo3(STATEVARNAME_EOON);
            	if(state->getValueAtTime(eo3, state._taskStart()).second) {
            		prof1[state._taskStart()] = 60;
            		prof1[state._taskEnd()] = 10;
            	}
            	return prof1;
        }
        public Profile<double> POWERSUB_PowerProfile_SSDRSUB(State* state){
            	Profile<double> prof1;
	            prof1[state._eventStart()] = 15;
            	return prof1;
        }
        public Profile<double> POWERSUB_PowerProfile_COMMSUB(State* state){
            StateVarKey<double> c1(STATEVARNAME_DATARATE);
        	return state.getProfile(c1)*20;
        }
        
        // Do not edit code below this line
        public static Dependencies Instance(){}
        public static Dependencies Instance(int iNum){}
        public State getState(int assetnum){}
        public State getAssetState(int assetnum){}
        public void updateState(const List<State> newStates){}
        public void updateStates(int thread, const List<State> newStates){}
        
        private static Dependencies pinstance{}
        private Dictionary<Dependencies, int> pinstances;

    }
}
