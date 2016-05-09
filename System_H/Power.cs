using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Utilities;
using HSFUniverse;
using MissionElements;
using UserModel;
using HSFSystem;

namespace HSFSubsystem
{
    public class Power : Subsystem
    {
        #region Attributes
        //Some Defaults
        private double _batterySize = 1000000;
        private double _fullSolarPanelPower = 150;
        private double _penumbraSolarPanelPower = 75;

        //put these in constructor and get from xml
        public static StateVarKey<double> DOD_KEY;
        public static StateVarKey<double> POWIN_KEY; 
        #endregion Attributes

        #region Constructors
        /// <summary>
        /// Create a new power node. Defaults: batterySize = 1000000, fullSolarPanelPower =150, penumbraSolarPanelPower = 75
        /// </summary>
        /// <param name="PowerNode"></param>
        /// <param name="dependencies"></param>
        public Power(XmlNode PowerNode, Dependencies dependencies, Asset asset) 
        {
            DefaultSubName = "Power";
            Asset = asset;
            getSubNameFromXmlNode(PowerNode);
            DOD_KEY = new StateVarKey<double>(Asset.Name + "." + "DepthOfDischarge");
            POWIN_KEY = new StateVarKey<double>(Asset.Name + "." + "SolarPanelPowerIn");
            addKey(DOD_KEY);
            addKey(POWIN_KEY);
            SubsystemDependencyFunctions = new Dictionary<string, Delegate>();
            DependentSubsystems = new List<ISubsystem>();
            if (PowerNode.Attributes["batterySize"] != null)
                _batterySize = (double)Convert.ChangeType(PowerNode.Attributes["batterySize"].Value, typeof(double));
            if (PowerNode.Attributes["fullSolarPower"] != null)
                _fullSolarPanelPower = (double)Convert.ChangeType(PowerNode.Attributes["fullSolarPower"].Value, typeof(double));
            if(PowerNode.Attributes["penumbraSolarPower"] != null)
                _penumbraSolarPanelPower = (double)Convert.ChangeType(PowerNode.Attributes["penumbraSolarPower"].Value, typeof(double));
            

        }
        #endregion Constructors

        #region Methods
        private double getSolarPanelPower(ShadowState shadow)
        {
            switch (shadow)
            {
                case ShadowState.UMBRA:
                    return 0;
                case ShadowState.PENUMBRA:
                    return _penumbraSolarPanelPower;
                default:
                    return _fullSolarPanelPower;
            }
        }
        private HSFProfile<double> calcSolarPanelPowerProfile(double start, double end, ref SystemState state, DynamicState position, Universe universe)
        {
            // create solar panel profile for this event
            double freq = 5;
            ShadowState lastShadow = universe.Sun.castShadowOnPos(position, start);
            HSFProfile<double> solarPanelPowerProfile = new HSFProfile<double>(start, getSolarPanelPower(lastShadow));

            for (double time = start + freq; time <= end; time += freq)
            {
                ShadowState shadow = universe.Sun.castShadowOnPos(position, time);
                // if the shadow state changes during this step, save the power data
                if (shadow != lastShadow)
                {
                    solarPanelPowerProfile[time] = getSolarPanelPower(shadow);
                    lastShadow = shadow;
                }
            }
            state.addValue(POWIN_KEY, solarPanelPowerProfile);
            return solarPanelPowerProfile;
        }

        /// <summary>
        /// Override of the canPerform method for the power subsystem
        /// </summary>
        /// <param name="oldState"></param>
        /// <param name="newState"></param>
        /// <param name="tasks"></param>
        /// <param name="universe"></param>
        /// <returns></returns>
        public override bool canPerform(SystemState oldState, SystemState newState, Dictionary<Asset, Task> tasks,
                            Universe universe)
        {
            //Make sure all dependent subsystems have been evaluated
            if (!base.canPerform(oldState, newState, tasks, universe)) 
                return false;
            double es = newState.EventStart;
            double te = newState.TaskEnd;
            double ee = newState.EventEnd;
            double powerSubPowerOut = 10;

            if (ee > SimParameters.SimEndSeconds)
                return false;

            // get the old DOD
            double olddod = oldState.getLastValue(Dkeys.First()).Value; //TODO: (Morgan check front is same as first

            // collect power profile out
            HSFProfile<double> powerOut = DependencyCollector(newState); // deps->callDoubleDependency("POWERSUB_getPowerProfile");
            powerOut = powerOut + powerSubPowerOut;
            // collect power profile in
            DynamicState position = Asset.AssetDynamicState;
            HSFProfile<double> powerIn = calcSolarPanelPowerProfile(es, te, ref newState, position, universe);
            // calculate dod rate
            HSFProfile<double> dodrateofchange = ((powerOut - powerIn) / _batterySize);

            bool exceeded= false ;
            double freq = 5.0;
            HSFProfile<double> dodProf = dodrateofchange.lowerLimitIntegrateToProf(es, te, freq, 0.0, ref exceeded, 0, olddod);

            newState.addValue(DOD_KEY, dodProf);
            return true;
        }

        /// <summary>
        /// Override of the canExtend method for the power subsystem
        /// </summary>
        /// <param name="newState"></param>
        /// <param name="universe"></param>
        /// <param name="evalToTime"></param>
        /// <returns></returns>
        public override bool canExtend(SystemState newState, Universe universe, double evalToTime) { 
            double ee = newState.EventEnd;
            if (ee > SimParameters.SimEndSeconds)
                return false;

            Sun sun = universe.Sun;
            double te = newState.getLastValue(DOD_KEY).Key;
            if (newState.EventEnd < evalToTime)
                newState.EventEnd = evalToTime;

            // get the dod initial conditions
            double olddod = newState.getValueAtTime(DOD_KEY, te).Value;

            // collect power profile out
            HSFProfile<double> powerOut = new HSFProfile<double>(te, 65);
            // collect power profile in
            DynamicState position = Asset.AssetDynamicState;
            HSFProfile<double> powerIn = calcSolarPanelPowerProfile(te, ee, ref newState, position, universe);
            // calculate dod rate
            HSFProfile<double> dodrateofchange = ((powerOut - powerIn) / _batterySize);

            bool exceeded_lower = false, exceeded_upper = false;
            double freq = 5.0;
            HSFProfile<double> dodProf = dodrateofchange.limitIntegrateToProf(te, ee, freq, 0.0, 1.0, ref exceeded_lower, ref exceeded_upper, 0, olddod);
            if (exceeded_upper)
                return false;
            newState.addValue(DOD_KEY, dodProf);
            return true;
        }
        #endregion Methods
    }

}
