// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using Utilities;
using System.Xml;
using MissionElements;
using HSFUniverse;
using HSFSystem;
using System.Diagnostics.CodeAnalysis;
//using Logging;

namespace HSFSubsystem
{
    //[ExcludeFromCodeCoverage]
    public class ADCS : Subsystem
    {
        #region Attributes
        protected StateVariableKey<Matrix<double>> POINTVEC_KEY;
         double _slewRate = 5;//deg/sec
        //double _slewRate = 5;
        #endregion Attributes

        #region Constructors
        /// <summary>
        /// Constructor for built in subsystems
        /// Defaults: Slew time: 10s
        /// </summary>
        /// <param name="ADCSNode"></param>
        /// <param name="dependencies"></param>
        /// <param name="asset"></param>
        public ADCS(XmlNode ADCSNode, Dependency dependencies, Asset asset)
        {
            //DefaultSubName = "Adcs";
            Asset = asset;
            GetSubNameFromXmlNode(ADCSNode);
            double slewRate;
            if (ADCSNode.Attributes["slewRate"].Value != null)
            {
                Double.TryParse(ADCSNode.Attributes["slewRate"].Value, out slewRate);
                _slewRate = slewRate;
            }

            // Move this to SubsystemFactory
            var states = ADCSNode.SelectNodes("IC");
            //foreach (XmlNode state in states)
            //{
            //    addKey(new StateVarKey<Matrix<double>>(Asset.Name + "." + state.Attributes["key"].Value));
            //}
            //POINTVEC_KEY = new StateVariableKey<Matrix<double>>(Asset.Name + "." + "eci_pointing_vector(xyz)");
            //addKey(POINTVEC_KEY);
            DependentSubsystems = new List<Subsystem>();
            SubsystemDependencyFunctions = new Dictionary<string, Delegate>();
            dependencies.Add("PowerfromADCS"+"."+Asset.Name, new Func<Event, HSFProfile<double>>(POWERSUB_PowerProfile_ADCSSUB));
            //dependencies.Add("testDep", new Func<Event, HSFProfile<double>>((Delegate)"test");
        }
        public HSFProfile<double> test(Event currentEvent)
        {
            return new HSFProfile<double>();
        }

        #endregion Constructors

        #region Methods
        /// <summary>
        /// An override of the Subsystem CanPerform method
        /// </summary>
        /// <param name="proposedEvent"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        public override bool CanPerform(Event proposedEvent, Domain environment)
        {
            //if (base.CanPerform( proposedEvent, environment) == false)
            //    return false;
            var POINTVEC_KEY = this.Mkeys[0];

            double es = proposedEvent.GetEventStart(Asset);
            double ts = proposedEvent.GetTaskStart(Asset);
            double te = proposedEvent.GetTaskEnd(Asset);

            //  Now accounting for previous pointing vector...
            var pc = _newState.GetLastValue(POINTVEC_KEY).Value;

            DynamicState position = Asset.AssetDynamicState;
            Matrix<double> sc = position.PositionECI(ts);
            Matrix<double> t = _task.Target.DynamicState.PositionECI(ts);
            Matrix<double> p = t - sc;

            Matrix<double> sc_n = sc / Matrix<double>.Norm(sc);
            Matrix<double> pc_n = pc / Matrix<double>.Norm(pc);
            Matrix<double> p_n = p / Matrix<double>.Norm(p);

            double slewAngle = Math.Acos(Matrix<double>.Dot(p_n, pc_n)) * 180 / Math.PI;
            //double slewAngle = Math.Acos(Matrix<double>.Dot(p_n, -sc_n)) * 180 / Math.PI;
                        
            //double timetoslew = (rand()%5)+8;
            double timetoslew = slewAngle/_slewRate;
            
            if (es + timetoslew > ts)
            {
                if (es + timetoslew > te)
                {
                    // TODO: Change this to Logger
                    //Console.WriteLine("ADCS: Not enough time to slew event start: "+ es + "task end" + te);
                    return false;
                }
                else
                    ts = es + timetoslew;
            }




            // set state data

            //_newState.SetProfile(POINTVEC_KEY, new HSFProfile<Matrix<double>>(ts, m_pv));
            _newState.AddValue(POINTVEC_KEY, ts, p);
            proposedEvent.SetTaskStart(Asset, ts);
            return true;
        }

        /// <summary>
        /// Dependecy function for the power subsystem
        /// </summary>
        /// <param name="currentEvent"></param>
        /// <returns></returns>
        public HSFProfile<double> POWERSUB_PowerProfile_ADCSSUB(Event currentEvent)
        {
            HSFProfile<double> prof1 = new HSFProfile<double>();
            prof1[currentEvent.GetEventStart(Asset)] = 40;
            prof1[currentEvent.GetTaskStart(Asset)] = 60;
            prof1[currentEvent.GetTaskEnd(Asset)] = 40;
            return prof1;
        }
        #endregion Methods
    }
}