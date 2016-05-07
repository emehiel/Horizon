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
        public static StateVarKey<Matrix<double>> POINTVEC_KEY = new StateVarKey<Matrix<double>>("ECI_Pointing_Vector(XYZ)");
        #endregion Attributes

        #region Constructors
        public ADCS(XmlNode ADCSNode) //TODO: (Morgan) Change this to actually parse the XmlNode
        {
            DefaultSubName = "Adcs";
            getSubNameFromXmlNode(ADCSNode);
            addKey(POINTVEC_KEY);
            SubsystemDependencyFunctions = new Dictionary<string, Delegate>();
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

        #endregion Methods
    }
}