// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Utilities;
using UserModel;
using log4net;


namespace HSFUniverse
{

    /// <summary>
    /// A class that specifies the dynamic state of a given rigid body object in the system in a given coordinate frame.
    /// Dynamic State data includes position, velocity, (future: Euler Angels, Quaternions, body angular rates)
    /// This class replaces the Position Class in prior versions of HSF.      
    /// The two coordinate frames used are ECI and LLA. 
    /// ECI refers to an unchanging coordinate frame which is relatively fixed with respect to the Solar
    /// System. The z axis runs along the Earth's rotational axis pointing North, the x axis points in 
    /// the direction of the vernal equinox, and the y axis completes the right-handed orthogonal system.
    /// Units are (Kilometers, Kilometers, Kilometers).
    /// LLA refers to the geodetic latitude, longitude, and altitude above the planetary ellipsoid.
    /// Units are(Radians, Radians, Kilometers)
    /// </summary>
    [Serializable]
    public class DynamicState
    {
        /// <summary>
        /// SortedList with time and the state data at that time stored in a double Matrix
        /// </summary>
        public DynamicStateType Type { get; private set; }
        public EOMS Eoms { get; private set; }
        public string Name { get; private set; }
        public IntegratorParameters IntegratorParameters = new IntegratorParameters();
        private PropagationType _propagatorType;
        private IntegratorOptions _integratorOptions;
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private SortedList<double, Vector> _stateData;

        #region Constructors
        public DynamicState(XmlNode dynamicStateXMLNode)
        {
            if (dynamicStateXMLNode.ParentNode.Attributes["assetName"] != null)
                Name = dynamicStateXMLNode.ParentNode.Attributes["assetName"].Value.ToString() + "." + "DynamicState";
            else
                Name = "Generic.DynamicState";

            // TODO add a line to pre-propagate to simEndTime if the type is predetermined
            // TODO catch exception if _type or initial conditions are not set from teh XML file
            if (dynamicStateXMLNode.Attributes["DynamicStateType"] != null)
            {
                string typeString = dynamicStateXMLNode.Attributes["DynamicStateType"].Value.ToString();
                Type = (DynamicStateType)Enum.Parse(typeof(DynamicStateType), typeString);
            }
            Vector ics = new Vector(dynamicStateXMLNode.Attributes["ICs"].Value.ToString());
            _stateData = new SortedList<double, Vector>((int)(SimParameters.SimEndSeconds/SchedParameters.SimStepSeconds));
            _stateData.Add(0.0, ics);

            if (!(Type == DynamicStateType.STATIC_LLA || Type == DynamicStateType.STATIC_ECI))
            {
                Eoms = EOMFactory.GetEomClass(dynamicStateXMLNode);
                
                // Returns a null reference if INTEGRATOR is not set in XML
                XmlNode integratorNode = dynamicStateXMLNode["INTEGRATOR"];

                _integratorOptions = new IntegratorOptions();

                if (integratorNode != null)
                {
                    if (integratorNode.Attributes["h"] != null)
                        _integratorOptions.h = Convert.ToDouble(integratorNode.Attributes["h"].Value);
                    if (integratorNode.Attributes["rtol"] != null)
                        _integratorOptions.rtol = Convert.ToDouble(integratorNode.Attributes["rtol"].Value);
                    if (integratorNode.Attributes["atol"] != null)
                        _integratorOptions.atol = Convert.ToDouble(integratorNode.Attributes["atol"].Value);
                    if (integratorNode.Attributes["eps"] != null)
                        _integratorOptions.eps = Convert.ToDouble(integratorNode.Attributes["eps"].Value);
                }
            }
            else
            {
                Eoms = null;
                //_stateDataTimeStep = 30.0;
            }

        }

        public DynamicState(DynamicStateType type, EOMS eoms, Vector initialConditions)
        {
            _stateData.Add(0.0, initialConditions);
            Type = type;
            Eoms = eoms;
            //_stateDataTimeStep = stateDataTimeStep;
        }
        #endregion


        public Vector InitialConditions()
        {
            return _stateData.Values.Last();
        }

        public void Add(double simTime, Vector dynamicState)
        {
            _stateData.Add(simTime, dynamicState);
        }

        public Vector DynamicStateECI(double simTime) //Should we be interpolating?
        {
            return this[simTime]; 
        }

        #region Accessors
        /// <summary>
        /// Returns the current position of the system at the time simTime.
        /// </summary>
        /// <param name="simTime"></param>
        /// <returns></returns>
        public Vector PositionECI(double simTime)
        {
            return this[simTime][new MatrixIndex(1, 3)];
        }

        /// <summary>
        /// Returns velocity at time in ECI
        /// </summary>
        /// <param name="simTime"></param>
        /// <returns></returns>
        public Vector VelocityECI(double simTime)
        {
            return this[simTime][new MatrixIndex(4, 6)];
        }

        /// <summary>
        /// Returns euler angles at time in radians
        /// </summary>
        /// <param name="simTime"></param>
        /// <returns></returns>
        public Vector EulerAngles(double simTime)
        {
            Vector eulerAngles = GeometryUtilities.quat2euler(Quaternions(simTime));
            return eulerAngles;
        }

        /// <summary>
        /// Returns quaternions at time
        /// </summary>
        /// <param name="simTime"></param>
        /// <returns></returns>
        public Matrix<double> Quaternions(double simTime)
        {
            return _stateData[simTime][new MatrixIndex(7, 10)];
        }

        /// <summary>
        /// Returns euler rates at time in rad/s
        /// </summary>
        /// <param name="simTime"></param>
        /// <returns></returns>
        public Matrix<double> EulerRates(double simTime)
        {
            return _stateData[simTime][new MatrixIndex(11, 13)];
        }
        #endregion

        /// <summary>
        /// Propogate state from last propogated time to simTime
        /// </summary>
        /// <param name="simTime"></param>
        private void PropagateState(double simTime)
        {
            log.Info("Integrating and resampling dynamic state data to "+ simTime + "seconds...");
            Matrix <double> tSpan = new Matrix<double>(new double[1, 2] { { _stateData.Last().Key, simTime } });
            // Update the integrator parameters using the information in the XML Node

            Matrix<double> data = Integrator.RK45(Eoms, tSpan, InitialConditions(), _integratorOptions, IntegratorParameters);

            for (int index = 1; index <= data.Length; index++)
                _stateData[data[1, index]] = data[new MatrixIndex(2, data.NumRows), index];
            log.Info("Done Integrating");
        }
        public void DynamicPropogateState()
        {
            double simTime = _stateData.Last().Key + SchedParameters.SimStepSeconds;
            Matrix<double> tSpan = new Matrix<double>(new double[1, 2] { { _stateData.Last().Key, simTime } });
            // Update the integrator parameters using the information in the XML Node

            Matrix<double> data = Integrator.RK45(Eoms, tSpan, InitialConditions(), _integratorOptions, IntegratorParameters);

            for (int index = 1; index <= data.Length; index++)
                _stateData[data[1, index]] = (Vector)data[new MatrixIndex(2, data.NumRows), index];
        }
        /// <summary>
        /// Gets and Sets the dynamic state of an asset in inertial coordinates at the given simulation time.
        /// This method overwrites any existing state data at simTime.
        /// If the dynamic state data does not exist at simTime, the value returned is a linear interpolation
        /// of existing data.
        /// </summary>
        /// <param name="simTime">The simulation time key</param>
        /// <returns>The inertial dynamic state date of the asset</returns>
        private Vector this[double simTime]
        {
            get
            {
                
                if (Type == DynamicStateType.STATIC_LLA)
                {
                    // Set the JD associated with simTime
                    double JD = simTime / 86400.0 + SimParameters.SimStartJD;
                    return GeometryUtilities.LLA2ECI(InitialConditions(), JD);
                }

                else if (Type == DynamicStateType.STATIC_ECI)
                    return InitialConditions();
                else if (Type == DynamicStateType.PREDETERMINED_ECI || Type == DynamicStateType.PREDETERMINED_LLA)
                {
                    bool hasNotPropagated = _stateData.Count() == 1;
                    if (hasNotPropagated)
                        PropagateState(SimParameters.SimEndSeconds);

                    Vector dynamicStateAtSimTime;

                    if (!_stateData.TryGetValue(simTime, out dynamicStateAtSimTime))
                    {
                        int lowerIndex = _stateData.Keys.LowerBoundIndex(simTime);
                        int slopeInd = 1;
                        if (simTime >= SimParameters.SimEndSeconds)
                            slopeInd = -1;
                        KeyValuePair<double, Vector> lowerData = _stateData.ElementAt(lowerIndex);
                        KeyValuePair<double, Vector> upperData = _stateData.ElementAt(lowerIndex + slopeInd);

                        double lowerTime = lowerData.Key;
                        Vector lowerState = lowerData.Value;

                        double upperTime = upperData.Key;
                        Vector upperState = upperData.Value;

                        Vector slope = (upperState - lowerState) / (upperTime - lowerTime);

                        dynamicStateAtSimTime = slope * (simTime - lowerTime) + lowerState;
                    }

                return dynamicStateAtSimTime;

                }
                else if (Type == DynamicStateType.DYNAMIC_ECI || Type == DynamicStateType.DYNAMIC_LLA)
                {
                    Vector dynamicStateAtSimTime;

                    if (!_stateData.TryGetValue(simTime, out dynamicStateAtSimTime))
                    {
                        int lowerIndex = _stateData.Keys.LowerBoundIndex(simTime);
                        int slopeInd = 1;
                        if (simTime >= SimParameters.SimEndSeconds)
                            slopeInd = -1;
                        KeyValuePair<double, Vector> lowerData = _stateData.ElementAt(lowerIndex);
                        KeyValuePair<double, Vector> upperData = _stateData.ElementAt(lowerIndex + slopeInd);

                        double lowerTime = lowerData.Key;
                        Vector lowerState = lowerData.Value;

                        double upperTime = upperData.Key;
                        Vector upperState = upperData.Value;

                        Vector slope = (upperState - lowerState) / (upperTime - lowerTime);

                        dynamicStateAtSimTime = slope * (simTime - lowerTime) + lowerState;
                    }
                    return dynamicStateAtSimTime;
                }
                else
                    return null; // TODO: Throw exception?

            }
            set
            {
                _stateData[simTime] = value;
            }
        }

        /// <summary>
        /// Determine if there is a line of sight (LOS) to the target at sim time
        /// </summary>
        /// <param name="targetPositionECI"></param>
        /// <param name="simTime"></param>
        /// <returns></returns>
        public bool hasLOSTo(Vector targetPositionECI, double simTime)
        {
            return GeometryUtilities.hasLOS(PositionECI(simTime), targetPositionECI);
        }

        /// <summary>
        /// Override of the Object ToString method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var csv = new StringBuilder();
            string t = Name + "_time,";
            string rx = Name + "_R_x,";
            string ry = Name + "_R_y,";
            string rz = Name + "_R_z,";
            string vx = Name + "_V_x,";
            string vy = Name + "_V_y,";
            string vz = Name + "_V_z,";
            // header
            csv.AppendLine(t + rx + ry + rz + vx + vy + vz );

            // data
            foreach (var d in _stateData)
                csv.AppendLine(d.Key + "," + d.Value[1] + "," + d.Value[2] + "," + d.Value[3] + "," + d.Value[4] + "," + d.Value[5] + "," + d.Value[6]);

            return csv.ToString();
        }
    }
    
    // Dynamic states type supported by HSF
    public enum DynamicStateType { STATIC_LLA, STATIC_ECI, PREDETERMINED_LLA, PREDETERMINED_ECI, DYNAMIC_LLA, DYNAMIC_ECI };

    // Propagator types supported by HSF
    public enum PropagationType { TRAPZ, RK4, RK45, SPG4 };
}
