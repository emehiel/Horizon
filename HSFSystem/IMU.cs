using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSFSystem;
using HSFSubsystem;
using System.Xml;
using MissionElements;
using Utilities;
using HSFUniverse;

namespace HSFSystem
{
    public class IMU:Subsystem
    {
        #region Attributes
        // TODO update keys to accept vector
        protected StateVarKey<Matrix<double>> MEASURE_KEY;
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
        #region Constructors
        public IMU(XmlNode SubNode, Asset asset)
        {
            DefaultSubName = "IMU";
            Asset = asset;
            GetSubNameFromXmlNode(SubNode);
            int gyr = 0;
            int acc = 1;
            XmlNode IMUNode = SubNode;
            if (SubNode.ChildNodes[1].Name.Equals("Gyro"))
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
        public IMU(XmlNode SubNode, Dependency dependencies ,Asset asset) : this(SubNode, asset)
        {
            DependentSubsystems = new List<Subsystem>();
            SubsystemDependencyFunctions = new Dictionary<string, Delegate>();
            dependencies.Add("ACCxFromIMU" + "." + Asset.Name, new Func<Event, HSFProfile<double>>(STATESUB_ACCxMeasurementsFrom_IMUSUB));
            dependencies.Add("ACCyFromIMU" + "." + Asset.Name, new Func<Event, HSFProfile<double>>(STATESUB_ACCyMeasurementsFrom_IMUSUB));
            dependencies.Add("ACCzFromIMU" + "." + Asset.Name, new Func<Event, HSFProfile<double>>(STATESUB_ACCzMeasurementsFrom_IMUSUB));
            dependencies.Add("GYRxFromIMU" + "." + Asset.Name, new Func<Event, HSFProfile<double>>(STATESUB_GYRxMeasurementsFrom_IMUSUB));
            dependencies.Add("GYRyFromIMU" + "." + Asset.Name, new Func<Event, HSFProfile<double>>(STATESUB_GYRyMeasurementsFrom_IMUSUB));
            dependencies.Add("GYRzFromIMU" + "." + Asset.Name, new Func<Event, HSFProfile<double>>(STATESUB_GYRzMeasurementsFrom_IMUSUB));
            dependencies.Add("BaroFromIMU" + "." + Asset.Name, new Func<Event, HSFProfile<double>>(STATESUB_BaroMeasurementsFrom_IMUSUB));
            MEASURE_KEY = new StateVarKey<Matrix<double>>(Asset.Name + "." + "measurements");
            addKey(MEASURE_KEY);
        }
        #endregion
        #region Overrides
        /// <summary>
        /// An override of the Subsystem CanPerform method
        /// </summary>
        /// <param name="proposedEvent"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        public override bool CanPerform(Event proposedEvent, Universe environment)
        {
            double ts = proposedEvent.GetTaskStart(Asset);
            IsEvaluated = true;
            if (!base.CanPerform(proposedEvent, environment))
                return false;
            if(_task == null)
            {
                return true; // TODO Is this what we want to do?

            }
            if (_task.Type == TaskType.FLYALONG)
            {
                HSFProfile<double> newProf = DependencyCollector(proposedEvent);
                if (!newProf.Empty())
                    proposedEvent.State.SetProfile(MEASURE_KEY, newProf);
            }
            var state = Asset.AssetDynamicState.DynamicStateECI(proposedEvent.GetEventStart(Asset));
            Vector gyro = new Vector(3);
            Vector accel = new Vector(3);
            try
            {
                gyro = Asset.AssetDynamicState.IntegratorParameters.GetValue(new StateVarKey<Vector>("asset1.gyro"));
                accel = Asset.AssetDynamicState.IntegratorParameters.GetValue(new StateVarKey<Vector>("asset1.accel"));
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Key Not Found x");
            }
            Matrix<double> gyr = Gyroscope(gyro);
            Matrix<double> acc = Accelerometer(new Vector(3));
            Matrix<double> measure = new Matrix<double>(6, 1);
            measure = Matrix<double>.Horzcat(acc, gyr);
            _newState.AddValue(MEASURE_KEY, new HSFProfile<Matrix<double>>(ts, measure));
            return true;
        }
        #endregion
        #region Methods
        public Vector Gyroscope(Vector truth)
        {
            double noiseX = GaussianWhiteNoise(0, _gyrRateNoiseDensity); // *_gyrOutputRate);
            double noiseY = GaussianWhiteNoise(0, _gyrRateNoiseDensity); // * _gyrOutputRate);
            double noiseZ = GaussianWhiteNoise(0, _gyrRateNoiseDensity); // * _gyrOutputRate);

            Vector reading = new Vector(3);

            reading[1] = truth[1];// + noiseX;
            reading[2] = truth[2];// + noiseY;
            reading[3] = truth[3];// + noiseZ;
            /* Check for Saturation of sensor */
            int i = 1;
            foreach( double val in reading)
            {
                if (val > _gyrMax) { reading[i] = _gyrMax; }
                if (val < _gyrMin) { reading[i] = _gyrMin; }
                i++;
            }
            return reading;
        }
        public Vector Accelerometer(Vector truth)
        {
            double noiseX = GaussianWhiteNoise(0, _accNoiseDensity); // * _accOutputRate);
            double noiseY = GaussianWhiteNoise(0, _accNoiseDensity); // * _accOutputRate);
            double noiseZ = GaussianWhiteNoise(0, _accNoiseDensity); // * _accOutputRate);

            Vector reading = new Vector(3);

            reading[1] = truth[1] + noiseX;
            reading[2] = truth[2] + noiseY;
            reading[3] = truth[3] + noiseZ;

            /* Check for Saturation of sensor */
            int i = 1;
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
        #endregion
        #region Dependencies
        public HSFProfile<double> STATESUB_ACCxMeasurementsFrom_IMUSUB(Event currentEvent)
        {
            HSFProfile<double> prof1 = new HSFProfile<double>();
            Vector accel = new Vector(3);
            try
            {
                accel = Asset.AssetDynamicState.IntegratorParameters.GetValue(new StateVarKey<Vector>("asset1.accel"));
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Key Not Found X");
            }
            Vector acc = Accelerometer(accel);
            prof1[currentEvent.GetEventStart(Asset)] = acc[1];
            return prof1;
        }
        public HSFProfile<double> STATESUB_ACCyMeasurementsFrom_IMUSUB(Event currentEvent)
        {
            HSFProfile<double> prof1 = new HSFProfile<double>();
            Vector accel = new Vector(3);
            try
            {
                accel = Asset.AssetDynamicState.IntegratorParameters.GetValue(new StateVarKey<Vector>("asset1.accel"));
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Key Not Found Y");
            }
            prof1[currentEvent.GetEventStart(Asset)] = accel[2];
            return prof1;
        }
        public HSFProfile<double> STATESUB_ACCzMeasurementsFrom_IMUSUB(Event currentEvent)
        {
            HSFProfile<double> prof1 = new HSFProfile<double>();
            Vector accel = new Vector(3);
            try
            {
                accel = Asset.AssetDynamicState.IntegratorParameters.GetValue(new StateVarKey<Vector>("asset1.accel"));
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Key Not Found Z");
            }
            prof1[currentEvent.GetEventStart(Asset)] = accel[3];
            return prof1;
        }
        public HSFProfile<double> STATESUB_GYRxMeasurementsFrom_IMUSUB(Event currentEvent)
        {
            HSFProfile<double> prof1 = new HSFProfile<double>();
            Vector gyro = new Vector(3);
            try
            {
                gyro = Asset.AssetDynamicState.IntegratorParameters.GetValue(new StateVarKey<Vector>("asset1.gyro"));
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Key Not Found x");
            }
            Vector gyr = Gyroscope(gyro);
            prof1[currentEvent.GetEventStart(Asset)] = gyr[1];
            return prof1;
        }
        public HSFProfile<double> STATESUB_GYRyMeasurementsFrom_IMUSUB(Event currentEvent)
        {
            HSFProfile<double> prof1 = new HSFProfile<double>();
            Vector gyro = new Vector(3);
            try
            {
                gyro = Asset.AssetDynamicState.IntegratorParameters.GetValue(new StateVarKey<Vector>("asset1.gyro"));
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Key Not Found x");
            }
            Vector gyr = Gyroscope(gyro);
            prof1[currentEvent.GetEventEnd(Asset)] = gyr[2];
            return prof1;
        }
        public HSFProfile<double> STATESUB_GYRzMeasurementsFrom_IMUSUB(Event currentEvent)
        {
            HSFProfile<double> prof1 = new HSFProfile<double>();
            Vector gyro = new Vector(3);
            try
            {
                gyro = Asset.AssetDynamicState.IntegratorParameters.GetValue(new StateVarKey<Vector>("asset1.gyro"));
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Key Not Found z");
            }
            Vector gyr = Gyroscope(gyro);
            prof1[currentEvent.GetEventStart(Asset)] = gyr[3];
            return prof1;
        }
        public HSFProfile<double> STATESUB_BaroMeasurementsFrom_IMUSUB(Event currentEvent)
        {
            HSFProfile<double> prof1 = new HSFProfile<double>();
            double ts = currentEvent.GetEventStart(Asset);
            Vector pos = Asset.AssetDynamicState.PositionECI(ts);
            double noise = GaussianWhiteNoise(0, 12); // FIXME: Sensor absolute accuracy +-1.5 mbar, ~8m = 1mbar at sea level
            prof1[ts] = pos[1]- 6378633;
            return prof1;
        }
        #endregion

    }
}
