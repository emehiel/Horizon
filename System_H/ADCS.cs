using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.Xml;
using MissionElements;
using HSFUniverse;

namespace HSFSubsystem
{
    public class ADCS : Subsystem
    {
        #region Attributes
        public static StateVarKey<Matrix<double>> POINTVEC_KEY = new StateVarKey<Matrix<double>>("ECI_Pointing_Vector(XYZ)");
        #endregion Attributes

        #region Constructors
        public ADCS()
        {
            base.Name = "ADCS";
            base.addKey(POINTVEC_KEY);
            base.SubsystemDependencyFunctions = new Dictionary<string, Delegate>();
        }
        public ADCS(string name)
        {
            base.Name = name;
            base.addKey(POINTVEC_KEY);
            base.SubsystemDependencyFunctions = new Dictionary<string, Delegate>();

        }
        public ADCS(XmlNode ADCSNode) //TODO: (Morgan) Change this to actually parse the XmlNode
        {
            //throw new NotImplementedException();
        }
        #endregion Constructors

        #region Methods
        public bool canPerform(SystemState oldState, SystemState newState, Task task, DynamicState position,
                                        Universe environment)
        {
            if (base.canPerform(oldState, newState, task, position, environment) == false)
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
            Matrix<double> m_SC_pos_at_ts_ECI = position.PositionECI(ts);
            Matrix<double> m_target_pos_at_ts_ECI = task.Target.DynamicState.PositionECI(ts);
            Matrix<double> m_pv = m_target_pos_at_ts_ECI - m_SC_pos_at_ts_ECI;

            // set state data
            newState.setProfile(POINTVEC_KEY, new HSFProfile<Matrix<double>>(ts, m_pv));

            return true;
        }

        public override bool canExtend(SystemState State, DynamicState position, Universe environment, double evalToTime)
        {
            if (State.EventEnd < evalToTime)
                State.EventEnd = evalToTime;
            return true;
        }
        #endregion Methods
    }
}