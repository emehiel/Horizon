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
        public static StateVarKey<Matrix<double>> POINTVEC_KEY; 
        #endregion Attributes

        #region Constructors
        public ADCS(XmlNode ADCSNode, Dependencies dependencies, Asset asset) 
        {
            DefaultSubName = "Adcs";
            Asset = asset;
            getSubNameFromXmlNode(ADCSNode);
            POINTVEC_KEY = new StateVarKey<Matrix<double>>(Asset.Name + "." +"ECI_Pointing_Vector(XYZ)");
            addKey(POINTVEC_KEY);
            DependentSubsystems = new List<ISubsystem>();
            SubsystemDependencyFunctions = new Dictionary<string, Delegate>();
            dependencies.Add("PowerfromADCS", new Func<SystemState, HSFProfile<double>>(POWERSUB_PowerProfile_ADCSSUB));
        }
        #endregion Constructors
        
        #region Methods
        public override bool canPerform(SystemState oldState, SystemState newState, Dictionary<Asset, Task> tasks,
                                        Universe environment)
        {
            if (base.canPerform(oldState, newState, tasks, environment) == false)
                return false;
            //double timetoslew = (rand()%5)+8;
            double timetoslew = 10;

            double es = newState.EventStart;
            double ts = newState.TaskStart;

            if (es + timetoslew > ts)
            {
                Logger.Report("ADCS");
                return false;
            }

            // from Brown, Pp. 99
            DynamicState position = Asset.AssetDynamicState;
            Matrix<double> m_SC_pos_at_ts_ECI = position.PositionECI(ts);
            Matrix<double> m_target_pos_at_ts_ECI = _task.Target.DynamicState.PositionECI(ts);
            Matrix<double> m_pv = m_target_pos_at_ts_ECI - m_SC_pos_at_ts_ECI;

            // set state data
            newState.setProfile(POINTVEC_KEY, new HSFProfile<Matrix<double>>(ts, m_pv));

            return true;
        }
        /// <summary>
        /// Dependecy function for the power subsystem
        /// </summary>
        /// <param name="currentState"></param>
        /// <returns></returns>
        HSFProfile<double> POWERSUB_PowerProfile_ADCSSUB(SystemState currentState)
        {
            HSFProfile<double> prof1 = new HSFProfile<double>();
            prof1[currentState.EventStart] = 40;
            prof1[currentState.TaskStart] = 60;
            prof1[currentState.TaskEnd] = 40;
            return prof1;
        }
        #endregion Methods
    }
}