using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSFSystem;
using HSFSubsystem;
using System.Xml;
using MissionElements;

namespace HSFSystem
{
    class IMU:Subsystem
    {
        #region Attributes
        protected double _accNoisePower = 0;
        protected double _accNaturalFrequency = 150;
        protected double _accDampingratio = 0.707;
        protected double _accMax = Double.MaxValue;
        protected double _accMin = Double.MinValue;
        protected double _accOutputRate = 100;
        protected double _gyrNaturalFrequency = 150;
        protected double _gyrDampingRatio = 0.707;
        protected double _gyrMax = Double.MaxValue;
        protected double _gyrMin = Double.MinValue;
        protected double _gyrRateWalk = 0;
        protected double _gyrBias = 0;
        protected double _gyrOutputRate = 100;
        #endregion
        public IMU(XmlNode IMUNode, Dependency dependencies, Asset asset)
        {
            DefaultSubName = "IMU";
            Asset = asset;
            GetSubNameFromXmlNode(IMUNode);
            int gyr = 0;
            int acc = 1;
            if (IMUNode.ChildNodes[1].Name.Equals("Gyro"))
            {
                gyr = 1;
                acc = 0;
            }

            if (IMUNode.ChildNodes[gyr].Attributes["gyroNaturalFrequency"] != null)
                _gyrNaturalFrequency = (double)Convert.ChangeType(IMUNode.ChildNodes[gyr].Attributes["gyroNaturalFrequency"].Value.ToString(), typeof(double));
            if (IMUNode.ChildNodes[gyr].Attributes["gyroDampingRatio"] != null)
                _gyrDampingRatio = (double)Convert.ChangeType(IMUNode.ChildNodes[gyr].Attributes["gyroDampingRatio"].Value.ToString(), typeof(double));
            if (IMUNode.ChildNodes[gyr].Attributes["gyroMax"] != null)
                _gyrMax = (double)Convert.ChangeType(IMUNode.ChildNodes[gyr].Attributes["gyroMax"].Value.ToString(), typeof(double));
            if (IMUNode.ChildNodes[gyr].Attributes["gyroMin"] != null)
                _gyrMin = (double)Convert.ChangeType(IMUNode.ChildNodes[gyr].Attributes["gyroMin"].Value.ToString(), typeof(double));
            if (IMUNode.ChildNodes[gyr].Attributes["gyroRateWalk"] != null)
                _gyrRateWalk = (double)Convert.ChangeType(IMUNode.ChildNodes[gyr].Attributes["gyroRateWalk"].Value.ToString(), typeof(double));
            if (IMUNode.ChildNodes[gyr].Attributes["gyroBias"] != null)
                _gyrBias = (double)Convert.ChangeType(IMUNode.ChildNodes[gyr].Attributes["gyroBias"].Value.ToString(), typeof(double));
            if (IMUNode.ChildNodes[gyr].Attributes["gyroOutputRate"] != null)
                _gyrOutputRate = (double)Convert.ChangeType(IMUNode.ChildNodes[gyr].Attributes["gyroOutputRate"].Value.ToString(), typeof(double));
            if (IMUNode.ChildNodes[acc].Attributes["accNoisePower"] != null)
                _accNoisePower = (double)Convert.ChangeType(IMUNode.ChildNodes[acc].Attributes["accNoisePower"].Value.ToString(), typeof(double));
            if (IMUNode.ChildNodes[acc].Attributes["accNaturalFrequency"] != null)
                _accNaturalFrequency = (double)Convert.ChangeType(IMUNode.ChildNodes[acc].Attributes["accNaturalFrequency"].Value.ToString(), typeof(double));
            if (IMUNode.ChildNodes[acc].Attributes["accDampingRatio"] != null)
                _accDampingratio = (double)Convert.ChangeType(IMUNode.ChildNodes[acc].Attributes["accDampingRatio"].Value.ToString(), typeof(double));
            if (IMUNode.ChildNodes[acc].Attributes["accMax"] != null)
                _accMax = (double)Convert.ChangeType(IMUNode.ChildNodes[acc].Attributes["accMax"].Value.ToString(), typeof(double));
            if (IMUNode.ChildNodes[acc].Attributes["accMin"] != null)
                _accMin = (double)Convert.ChangeType(IMUNode.ChildNodes[acc].Attributes["accMin"].Value.ToString(), typeof(double));
            if (IMUNode.ChildNodes[acc].Attributes["accOutputRate"] != null)
                _accOutputRate = (double)Convert.ChangeType(IMUNode.ChildNodes[acc].Attributes["accOutputRate"].Value.ToString(), typeof(double));

        }
        public double Gyroscope()
        {
            throw new NotImplementedException();
        }
        public double Accelerometer()
        {
            throw new NotImplementedException();
        }
    }
}
