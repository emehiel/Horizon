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
        private StateVarKey<double> DOD_KEY;
        private StateVarKey<double> POWIN_KEY; 
        #endregion Attributes

        #region Constructors
        /// <summary>
        /// Create a new power node. Defaults: batterySize = 1000000, fullSolarPanelPower =150, penumbraSolarPanelPower = 75
        /// </summary>
        /// <param name="PowerNode"></param>
        /// <param name="dependencies"></param>
        public Power(XmlNode PowerNode, Dependency dependencies, Asset asset) 
        {
            DefaultSubName = "Power";
            Asset = asset;
            getSubNameFromXmlNode(PowerNode);
            DOD_KEY = new StateVarKey<double>(Asset.Name + "." + "depthofdischarge");
            POWIN_KEY = new StateVarKey<double>(Asset.Name + "." + "solarpanelpowerin");
            addKey(DOD_KEY);
            addKey(POWIN_KEY);
            SubsystemDependencyFunctions = new Dictionary<string, Delegate>();
            DependentSubsystems = new List<Subsystem>();
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
        private HSFProfile<double> calcSolarPanelPowerProfile(double start, double end, SystemState state, DynamicState position, Universe universe)
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
        public override bool canPerform(Event proposedEvent, Universe universe)
        {
            //Make sure all dependent subsystems have been evaluated
            if (!base.canPerform(proposedEvent, universe)) 
                return false;
            double es = proposedEvent.GetEventStart(Asset);
            double te = proposedEvent.GetTaskEnd(Asset);
            double ee = proposedEvent.GetEventEnd(Asset);
            double powerSubPowerOut = 10;

            if (ee > SimParameters.SimEndSeconds)
            {
                Logger.Report("Simulation ended");
                return false;
            }

            // get the old DOD
            double olddod = _oldState.getLastValue(Dkeys.First()).Value; 

            // collect power profile out
            HSFProfile<double> powerOut = DependencyCollector(proposedEvent); // deps->callDoubleDependency("POWERSUB_getPowerProfile");
            powerOut = powerOut + powerSubPowerOut;
            // collect power profile in
            DynamicState position = Asset.AssetDynamicState;
            HSFProfile<double> powerIn = calcSolarPanelPowerProfile(es, te, _newState, position, universe);
            // calculate dod rate
            HSFProfile<double> dodrateofchange = ((powerOut - powerIn) / _batterySize);

            bool exceeded= false ;
            double freq = 5.0;
            HSFProfile<double> dodProf = dodrateofchange.lowerLimitIntegrateToProf(es, te, freq, 0.0, ref exceeded, 0, olddod);

            _newState.addValue(DOD_KEY, dodProf);
            return true;
        }

        /// <summary>
        /// Override of the canExtend method for the power subsystem
        /// </summary>
        /// <param name="newState"></param>
        /// <param name="universe"></param>
        /// <param name="evalToTime"></param>
        /// <returns></returns>
        public override bool canExtend(Event proposedEvent, Universe universe, double evalToTime) {
            double powOut = 65;
            double ee = proposedEvent.GetEventEnd(Asset);
            if (ee > SimParameters.SimEndSeconds)
                return false;

            Sun sun = universe.Sun;
            double te = proposedEvent.State.getLastValue(DOD_KEY).Key;
            if (proposedEvent.GetEventEnd(Asset) < evalToTime)
                proposedEvent.SetEventEnd(Asset, evalToTime);

            // get the dod initial conditions
            double olddod = proposedEvent.State.getValueAtTime(DOD_KEY, te).Value;

            // collect power profile out
            HSFProfile<double> powerOut = new HSFProfile<double>(te, powOut);
            // collect power profile in
            DynamicState position = Asset.AssetDynamicState;
            HSFProfile<double> powerIn = calcSolarPanelPowerProfile(te, ee, proposedEvent.State, position, universe);
            // calculate dod rate
            HSFProfile<double> dodrateofchange = ((powerOut - powerIn) / _batterySize);

            bool exceeded_lower = false, exceeded_upper = false;
            double freq = 5.0;
            HSFProfile<double> dodProf = dodrateofchange.limitIntegrateToProf(te, ee, freq, 0.0, 1.0, ref exceeded_lower, ref exceeded_upper, 0, olddod);
            if (exceeded_upper)
                return false;
            proposedEvent.State.addValue(DOD_KEY, dodProf);
            return true;
        }
        #endregion Methods
    }

}
