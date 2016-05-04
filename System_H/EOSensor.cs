using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HSFUniverse;
using MissionElements;
using System.Xml;
using HSFSystem;
using Utilities;

namespace HSFSubsystem
{
    public class EOSensor : Subsystem
    {
        #region Attributes
        //Default stuff
        public static string SUBNAME_EOSENSOR = "EOSensor";
        public static StateVarKey<double> PIXELS_KEY  = new StateVarKey<double> ("numPixels");
        public static StateVarKey<double> INCIDENCE_KEY = new StateVarKey<double>("IncidenceAngle");
        public static StateVarKey<bool> EOON_KEY = new StateVarKey<bool>("EOSensorOn");
        private double _lowQualityPixels = 5000;
        private double _lowQualityTime = 3;
        private double _midQualityPixels = 10000;
        private double _midQualityTime = 5;
        private double _highQualityPixels = 15000;
        private double _highQualityTime = 7;
        #endregion

        #region Constructors
        public EOSensor(XmlNode EOSensorXmlNode, Dependencies dependencies, Asset asset)
        {
            DefaultSubName = "EOSensor";
            getSubNameFromXmlNode(EOSensorXmlNode);
            Asset = asset;
            addKey(PIXELS_KEY);
            addKey(INCIDENCE_KEY);
            addKey(EOON_KEY);
            if (EOSensorXmlNode.Attributes["lowQualityPixels"] != null)
                _lowQualityPixels = (double)Convert.ChangeType(EOSensorXmlNode.Attributes["lowQualityPixels"].Value.ToString(), typeof(double));
            if (EOSensorXmlNode.Attributes["lowQualityPixels"] != null)
                _lowQualityTime = (double)Convert.ChangeType(EOSensorXmlNode.Attributes["lowQualityTime"].Value.ToString(), typeof(double));
            if (EOSensorXmlNode.Attributes["midQualityPixels"] != null)
                _midQualityPixels = (double)Convert.ChangeType(EOSensorXmlNode.Attributes["midQualityPixels"].Value.ToString(), typeof(double));
            if (EOSensorXmlNode.Attributes["midQualityTime"] != null)
                _midQualityTime = (double)Convert.ChangeType(EOSensorXmlNode.Attributes["midQualityTime"].Value.ToString(), typeof(double));
            if (EOSensorXmlNode.Attributes["highQualityPixels"] != null)
                _highQualityPixels = (double)Convert.ChangeType(EOSensorXmlNode.Attributes["highQualityPixels"].Value.ToString(), typeof(double));
            if (EOSensorXmlNode.Attributes["highQualityTime"] != null)
                _highQualityTime = (double)Convert.ChangeType(EOSensorXmlNode.Attributes["highQualityTime"].Value.ToString(), typeof(double));
        }
        #endregion

        #region Methods
        public bool canPerform(SystemState oldState, SystemState newState,
                            Task task, DynamicState position,
                            Universe environment, List<SystemState> allStates)
        {
            if (task.Type == TaskType.IMAGING)
            {
                //set pixels and time to caputre based on target value
                int value = task.Target.Value;
                double pixels = _lowQualityPixels;
                double timetocapture = _lowQualityTime;
                if (value <= _highQualityTime  && value >= _midQualityTime) //Morgan took out magic numbers
                {
                    pixels = _midQualityPixels;
                    timetocapture =_midQualityTime;
                }
                if (value > _highQualityTime)
                {
                    pixels = _highQualityPixels;
                    timetocapture = _highQualityTime;
                }

                // get event start and task start times
                double es = newState.EventStart;
                double ts = newState.TaskStart;

                // set task end based upon time to capture
                double te = ts + timetocapture;
                newState.TaskEnd = te;

                // calculate incidence angle
                // from Brown, Pp. 99
                double timage = ts + timetocapture / 2;
                Matrix<double> m_SC_pos_at_tf_ECI = position.DynamicStateECI(timage);
                Matrix<double> m_target_pos_at_tf_ECI = task.Target.DynamicState.DynamicStateECI(timage);
                Matrix<double> m_pv = m_target_pos_at_tf_ECI - m_SC_pos_at_tf_ECI;
                Matrix<double> pos_norm = -m_SC_pos_at_tf_ECI / Matrix<double>.Norm(-m_SC_pos_at_tf_ECI);
                Matrix<double> pv_norm = m_pv / Matrix<double>.Norm(m_pv);

                double incidenceang = 90 - 180 / Math.PI * Math.Acos(Matrix<double>.Dot(pos_norm, pv_norm));

                // set state data
                newState.addValue(INCIDENCE_KEY, new KeyValuePair<double, double>(timage, incidenceang));
                newState.addValue(INCIDENCE_KEY, new KeyValuePair<double, double>(timage + 1, 0.0));

                newState.addValue(PIXELS_KEY, new KeyValuePair<double, double>(timage, pixels));
                newState.addValue(PIXELS_KEY, new KeyValuePair<double, double>(timage + 1, 0.0));
                        
                newState.addValue(EOON_KEY, new KeyValuePair<double, bool>(ts, true));
                newState.addValue(EOON_KEY, new KeyValuePair<double, bool>(te, false));

                return true;
            }
            else
                return true;
        }
        #endregion
    }
}
