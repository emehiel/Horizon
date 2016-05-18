using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.Xml;
using MissionElements;
using HSFUniverse;
using HSFSystem;

namespace HSFSubsystem
{
    public class ADCS : Subsystem
    {
        #region Attributes
        private StateVarKey<Matrix<double>> POINTVEC_KEY; 
        #endregion Attributes

        #region Constructors
        public ADCS(XmlNode ADCSNode, Dependencies dependencies, Asset asset) 
        {
            DefaultSubName = "Adcs";
            Asset = asset;
            getSubNameFromXmlNode(ADCSNode);
            POINTVEC_KEY = new StateVarKey<Matrix<double>>(Asset.Name + "." +"ECI_Pointing_Vector(XYZ)");
            addKey(POINTVEC_KEY);
            DependentSubsystems = new List<Subsystem>();
            SubsystemDependencyFunctions = new Dictionary<string, Delegate>();
            dependencies.Add("PowerfromADCS", new Func<Event, HSFProfile<double>>(POWERSUB_PowerProfile_ADCSSUB));
        }
        #endregion Constructors
        
        #region Methods
        public override bool canPerform(Event proposedEvent, Universe environment)
        {
            if (base.canPerform(proposedEvent, environment) == false)
                return false;
            //double timetoslew = (rand()%5)+8;
            double timetoslew = 10;

            double es = proposedEvent.GetEventStart(Asset);
            double ts = proposedEvent.GetTaskStart(Asset);
            double te = proposedEvent.GetTaskEnd(Asset);

            if (es + timetoslew > ts) //fix event task end!
            {
                if (es + timetoslew > te)
                {
                    Logger.Report("ADCS: Not enough time to slew event start: "+ es + "task end" + te);
                    return false;
                }
                else
                    ts = es + timetoslew;
            }


            // from Brown, Pp. 99
            DynamicState position = Asset.AssetDynamicState;
            Matrix<double> m_SC_pos_at_ts_ECI = position.PositionECI(ts);
            Matrix<double> m_target_pos_at_ts_ECI = _task.Target.DynamicState.PositionECI(ts);
            Matrix<double> m_pv = m_target_pos_at_ts_ECI - m_SC_pos_at_ts_ECI;

            // set state data
            _newState.setProfile(POINTVEC_KEY, new HSFProfile<Matrix<double>>(ts, m_pv));
            proposedEvent.SetTaskStart(Asset, ts);
            return true;
        }
        /// <summary>
        /// Dependecy function for the power subsystem
        /// </summary>
        /// <param name="currentEvent"></param>
        /// <returns></returns>
        HSFProfile<double> POWERSUB_PowerProfile_ADCSSUB(Event currentEvent)
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