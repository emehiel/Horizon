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
    public class IMU:Subsystem
    {
        #region Attributes
        protected double _accNoiseDensity = 0;
        protected double _accNaturalFrequency = 150;
        protected double _accDampingratio = 0.707;
        protected double _accMax = Double.MaxValue;
        protected double _accMin = Double.MinValue;
        protected double _accOutputRate = 100;
        protected double _accNonLinearity = 0;
        protected double _accCrossAxis = 0;
        protected double _accScaleFactor = 1;
        protected double _gyrNaturalFrequency = 150;
        protected double _gyrDampingRatio = 0.707;
        protected double _gyrMax = Double.MaxValue;
        protected double _gyrMin = Double.MinValue;
        protected double _gyrRateNoiseDensity = 0;
        protected double _gyrBias = 0;
        protected double _gyrOutputRate = 100;
        protected double _gyrScaleFactor = 1;
        protected double _gyrAccelSensitivity = 0;
        protected double _gyrCrossAxis = 0;
        protected double _gyrNonLinearity = 0;
        static private Random rand = new Random(); 
        #endregion
        public IMU(XmlNode SubNode, Asset asset)
        {
            DefaultSubName = "IMU";
            Asset = asset;
            GetSubNameFromXmlNode(SubNode["ASSET"].ChildNodes[1]);
            int gyr = 0;
            int acc = 1;
            XmlNode IMUNode = SubNode["ASSET"].ChildNodes[1];
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
            if (IMUNode.ChildNodes[gyr].Attributes["gyroRateNoiseDensity"] != null)
                _gyrRateNoiseDensity = (double)Convert.ChangeType(IMUNode.ChildNodes[gyr].Attributes["gyroRateNoiseDensity"].Value.ToString(), typeof(double));
            if (IMUNode.ChildNodes[gyr].Attributes["gyroBias"] != null)
                _gyrBias = (double)Convert.ChangeType(IMUNode.ChildNodes[gyr].Attributes["gyroBias"].Value.ToString(), typeof(double));
            if (IMUNode.ChildNodes[gyr].Attributes["gyroOutputRate"] != null)
                _gyrOutputRate = (double)Convert.ChangeType(IMUNode.ChildNodes[gyr].Attributes["gyroOutputRate"].Value.ToString(), typeof(double));
            if (IMUNode.ChildNodes[gyr].Attributes["gyroOutputRate"] != null)
                _gyrScaleFactor = (double)Convert.ChangeType(IMUNode.ChildNodes[gyr].Attributes["gyroScaleFactor"].Value.ToString(), typeof(double));
            if (IMUNode.ChildNodes[gyr].Attributes["gyroOutputRate"] != null)
                _gyrNonLinearity = (double)Convert.ChangeType(IMUNode.ChildNodes[gyr].Attributes["gyroNonLinearity"].Value.ToString(), typeof(double));
            if (IMUNode.ChildNodes[gyr].Attributes["gyroOutputRate"] != null)
                _gyrAccelSensitivity = (double)Convert.ChangeType(IMUNode.ChildNodes[gyr].Attributes["gyroAccelSensitivity"].Value.ToString(), typeof(double));
            if (IMUNode.ChildNodes[gyr].Attributes["gyroOutputRate"] != null)
                _gyrCrossAxis = (double)Convert.ChangeType(IMUNode.ChildNodes[gyr].Attributes["gyroCrossAxis"].Value.ToString(), typeof(double));
            if (IMUNode.ChildNodes[acc].Attributes["accNoiseDensity"] != null)
                _accNoiseDensity = (double)Convert.ChangeType(IMUNode.ChildNodes[acc].Attributes["accNoiseDensity"].Value.ToString(), typeof(double));
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
            if (IMUNode.ChildNodes[acc].Attributes["accNonLinearity"] != null)
                _accNonLinearity = (double)Convert.ChangeType(IMUNode.ChildNodes[acc].Attributes["accNonLinearity"].Value.ToString(), typeof(double));
            if (IMUNode.ChildNodes[acc].Attributes["accCrossAxis"] != null)
                _accCrossAxis = (double)Convert.ChangeType(IMUNode.ChildNodes[acc].Attributes["accCrossAxis"].Value.ToString(), typeof(double));
            if (IMUNode.ChildNodes[acc].Attributes["accScaleFactor"] != null)
                _accScaleFactor = (double)Convert.ChangeType(IMUNode.ChildNodes[acc].Attributes["accScaleFactor"].Value.ToString(), typeof(double));

        }
        /*public IMU(XmlNode IMUNode, Asset asset) : this(IMUNode)
        {
            Asset = asset;
        }
        */
        public List<double> Gyroscope(List<double> truth)
        {
            double noiseX = GaussianWhiteNoise(0, _gyrRateNoiseDensity); // *_gyrOutputRate);
            double noiseY = GaussianWhiteNoise(0, _gyrRateNoiseDensity); // * _gyrOutputRate);
            double noiseZ = GaussianWhiteNoise(0, _gyrRateNoiseDensity); // * _gyrOutputRate);

            List<double> reading = new List<double>(3);

            reading.Add(truth[0] + noiseX);
            reading.Add(truth[1] + noiseY);
            reading.Add(truth[2] + noiseZ);
            /* Check for Saturation of sensor */
            int i = 0;
            foreach( double val in reading.ToList())
            {
                if (val > _gyrMax) { reading[i] = _gyrMax; }
                if (val < _gyrMin) { reading[i] = _gyrMin; }
                i++;
            }
            return reading;
        }
        public List<double> Accelerometer(List<double> truth)
        {
            double noiseX = GaussianWhiteNoise(0, _accNoiseDensity*_accOutputRate);
            double noiseY = GaussianWhiteNoise(0, _accNoiseDensity * _accOutputRate);
            double noiseZ = GaussianWhiteNoise(0, _accNoiseDensity * _accOutputRate);

            List<double> reading = new List<double>();

            reading[0] = truth[0] + noiseX;
            reading[1] = truth[1] + noiseY;
            reading[2] = truth[2] + noiseZ;

            /* Check for Saturation of sensor */
            int i = 0;
            foreach (double val in reading)
            {
                if (val > _accMax) { reading[i] = _accMax; }
                if (val < _accMin) { reading[i] = _accMin; }
                i++;
            }
            return reading;
        }

        public double GaussianWhiteNoise(double mean, double stdDev)
        // http://stackoverflow.com/a/218600
        {
            double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal =
                         mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
            return randNormal;
        }
    }
}
