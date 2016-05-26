using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Utilities;
using UserModel;


namespace HSFUniverse
{
    /*
     * A class that specifies the dynamic state of a given rigid body object in the system in a given coordinate frame.
     * Dynamic State data includes position, velocity, Euler Angels, Quaternions, body angular rates.
     * This class replaces the Position Class in prior versions of HSF.
     * 
    * The two coordinate frames used are ECI and LLA. 

    * ECI refers to an unchanging coordinate frame which is relatively fixed with respect to the Solar 
    * System. The z axis runs along the Earth's rotational axis pointing North, the x axis points in 
    * the direction of the vernal equinox, and the y axis completes the right-handed orthogonal system.
    * Units are (Kilometers, Kilometers, Kilometers).

    * LLA refers to the geodetic latitude, longitude, and altitude above the planetary ellipsoid. Units
    * are (Radians, Radians, Kilometers)
    *
    * @author Cory O'Connor
    * @author Eric Mehiel (conversion to C#)
    */
    [Serializable]
    public class DynamicState
    {
        /// <summary>
        /// SortedList with time and the state data at that time stored in a double Matrix
        /// </summary>
        private SortedList<double, Matrix<double>> _stateData;

        public DynamicStateType Type { get; private set; }

        public EOMS Eoms { get; private set; }

        //private double _stateDataTimeStep { get;  set; }

      //  private XmlNode _integratorNode;

        private PropagationType _propagatorType;

        private IntegratorOptions _integratorOptions;

        public DynamicState(XmlNode dynamicStateXMLNode)
        {
            // TODO add a line to pre-propagate to simEndTime if the type is predetermined
            // TODO catch exception if _type or initial conditions are not set from teh XML file
            if (dynamicStateXMLNode.Attributes["DynamicStateType"] != null)
            {
                string typeString = dynamicStateXMLNode.Attributes["DynamicStateType"].Value.ToString();
                Type = (DynamicStateType)Enum.Parse(typeof(DynamicStateType), typeString);
            }
            Matrix<double> ics = new Matrix<double>(dynamicStateXMLNode.Attributes["ICs"].Value.ToString());
            _stateData = new SortedList<double, Matrix<double>>();
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

        public DynamicState(DynamicStateType type, EOMS eoms, Matrix<double> initialConditions)
        {
            _stateData.Add(0.0, initialConditions);
            Type = type;
            Eoms = eoms;
            //_stateDataTimeStep = stateDataTimeStep;
        }

        public Matrix<double> InitialConditions()
        {
            return _stateData[0.0];
        }

        public void Add(double simTime, Matrix<double> dynamicState)
        {
            _stateData.Add(simTime, dynamicState);
        }

        public Matrix<double> DynamicStateECI(double simTime) //Should we be interpolating?
        {
            return this[simTime]; 
        }

        /// <summary>
        /// Returns the current position of the system at the time simTime.
        /// </summary>
        /// <param name="simTime"></param>
        /// <returns></returns>
        public Matrix<double> PositionECI(double simTime)
        {
            return this[simTime][new MatrixIndex(1, 3), 1];
        }

        public Matrix<double> VelocityECI(double simTime)
        {
            return _stateData[simTime][new MatrixIndex(4, 6), 1];
        }

        public Matrix<double> EulerAngles(double simTime)
        {
            Matrix<double> eulerAngles = GeometryUtilities.quat2euler(Quaternions(simTime));
            return eulerAngles;
        }

        public Matrix<double> Quaternions(double simTime)
        {
            return _stateData[simTime][new MatrixIndex(7, 10), 1];
        }

        public Matrix<double> EulerRates(double simTime)
        {
            return _stateData[simTime][new MatrixIndex(11, 13), 1];
        }

        private void PropagateState(double simTime)
        {

            Console.WriteLine("Integrating and resampling dynamic state data to {0} seconds... ", simTime);
            Matrix<double> tSpan = new Matrix<double>(new double[1, 2] { { _stateData.Last().Key, simTime } });
            // Update the integrator parameters using the information in the XML Node

            Matrix<double> data = Integrator.RK45(Eoms, tSpan, InitialConditions(), _integratorOptions);

            for (int index = 1; index <= data.Length; index++)
                _stateData[data[1, index]] = data[new MatrixIndex(2, data.NumRows), index];

            Console.WriteLine("DONE!");
        }
 
        /// <summary>
        /// Gets and Sets the dynamic state of an asset in inertial coordinates at the given simulation time.
        /// This method overwrites any existing state data at simTime.
        /// If the dynamic state data does not exist at simTime, the value returned is a linear interpolation
        /// of existing data.
        /// </summary>
        /// <param name="simTime">The simulation time key</param>
        /// <returns>The inertial dynamic state date of the asset</returns>
        private Matrix<double> this[double simTime]
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

                    Matrix<double> dynamicStateAtSimTime;

                    if (!_stateData.TryGetValue(simTime, out dynamicStateAtSimTime))
                    {
                        int lowerIndex = _stateData.Keys.LowerBoundIndex(simTime);
                        KeyValuePair<double, Matrix<double>> lowerData = _stateData.ElementAt(lowerIndex);
                        KeyValuePair<double, Matrix<double>> upperData = _stateData.ElementAt(lowerIndex + 1);

                        double lowerTime = lowerData.Key;
                        Matrix<double> lowerState = lowerData.Value;

                        double upperTime = upperData.Key;
                        Matrix<double> upperState = upperData.Value;

                        Matrix<double> slope = (upperState - lowerState) / (upperTime - lowerTime);

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

        public bool hasLOSTo(Matrix<double> targetPositionECI, double simTime)
        {
            return GeometryUtilities.hasLOS(PositionECI(simTime), targetPositionECI);
        }
    }

   
    public enum DynamicStateType { STATIC_LLA, STATIC_ECI, PREDETERMINED_LLA, PREDETERMINED_ECI, DYNAMIC_LLA, DYNAMIC_ECI };
    public enum PropagationType { TRAPZ, RK4, RK45, SPG4 };
}
