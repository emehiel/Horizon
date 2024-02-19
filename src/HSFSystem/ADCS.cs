// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using Utilities;
using System.Xml;
using MissionElements;
using HSFUniverse;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;
//using Logging;

namespace HSFSystem
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
        public ADCS() { }
        public ADCS(JObject adcsJson)
        {
            StringComparison stringCompare = StringComparison.CurrentCultureIgnoreCase;
            if (adcsJson.TryGetValue("slewRate", stringCompare, out JToken slewRateJson))
                this._slewRate = slewRateJson.Value<double>();
        }
        /// <summary>
        /// Constructor for built in subsystems
        /// Defaults: Slew time: 10s
        /// </summary>
        /// <param name="ADCSNode"></param>
        /// <param name="asset"></param>
        public ADCS(XmlNode ADCSNode)
        {
            //DefaultSubName = "Adcs";

            double slewRate;
            if (ADCSNode.Attributes["slewRate"].Value != null)
            {
                Double.TryParse(ADCSNode.Attributes["slewRate"].Value, out slewRate);
                _slewRate = slewRate;
            }
        }

        /// <summary>
        /// Constructor for scripted subsystems
        /// </summary>
        /// <param name="ADCSNode"></param>
        /// <param name="asset"></param>

       /* public ADCS(XmlNode ADCSNode, Asset asset) : base(ADCSNode, asset)
        {
            
        }
       */

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
            //  Not a fan of this, but will do for now.  Switching to Python....
            var POINTVEC_KEY = Mkeys.Find(k => k.VariableName == Asset.Name + ".eci_pointing_vector(xyz)");
            
            double es = proposedEvent.GetEventStart(Asset);
            double ts = proposedEvent.GetTaskStart(Asset);
            double te = proposedEvent.GetTaskEnd(Asset);            
            
            // from Brown, Pp. 99
            DynamicState position = Asset.AssetDynamicState;
            Matrix<double> m_SC_pos_at_ts_ECI = position.PositionECI(ts);
            Matrix<double> m_target_pos_at_ts_ECI = _task.Target.DynamicState.PositionECI(ts);
            Matrix<double> m_pv = m_target_pos_at_ts_ECI - m_SC_pos_at_ts_ECI;

            Matrix<double> sc_n = m_SC_pos_at_ts_ECI / Matrix<double>.Norm(m_SC_pos_at_ts_ECI);
            Matrix<double> pv_n = m_pv / Matrix<double>.Norm(m_pv);


            double slewAngle = Math.Acos(Matrix<double>.Dot(pv_n, -sc_n)) * 180 / Math.PI;
                        
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
            //var POINTVEC_KEY = this.Mkeys[0];
            //_newState.SetProfile(POINTVEC_KEY, new HSFProfile<Matrix<double>>(ts, m_pv));
            _newState.AddValue(POINTVEC_KEY, ts, m_pv);
            proposedEvent.SetTaskStart(Asset, ts);
            return true;
        }

        /// <summary>
        /// Dependecy function for the power subsystem
        /// </summary>
        /// <param name="currentEvent"></param>
        /// <returns></returns>
        public HSFProfile<double> Power_asset1_from_ADCS_asset1(Event currentEvent)
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