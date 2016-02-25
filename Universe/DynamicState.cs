using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Utilities;

namespace Universe
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
    public class DynamicState
    {
        private SortedList<Double, Matrix> _stateData;

        private DynamicStateType _type { get; }

        private EOMS _eoms { get; }

        private double _stateDataTimeStep { get;  set; }

        private XmlNode _intergratorNode;

        private PropagationType _propagatorType { get; set; }

        public DynamicState(XmlNode dynamicStateXMLNode)
        {
            string typeString = dynamicStateXMLNode.Attributes["DynamicStateType"].ToString();
            _type = (DynamicStateType)Enum.Parse(typeof(DynamicStateType), typeString);

            Matrix ics = new Matrix(dynamicStateXMLNode.Attributes["ICs"].ToString());
            _stateData.Add(0.0, ics);

            if (!(_type == DynamicStateType.STATIC_LLA || _type == DynamicStateType.STATIC_ECI))
            {
                // I think this should be a constructor...
                //_eoms = createEOMSObject(dynamicStateXMLNode["EOMS"]);

                // Returns a null reference if INTEGRATOR is not set in XML
                _intergratorNode = dynamicStateXMLNode["INTEGRATOR"];

                if (dynamicStateXMLNode.Attributes["PosDataStep"] != null)
                    _stateDataTimeStep = Convert.ToDouble(dynamicStateXMLNode.Attributes["PosDataStep"]);
                else
                    _stateDataTimeStep = 30.0;
            }
            else
            {
                _eoms = null;
                _intergratorNode = null;
                _stateDataTimeStep = 30.0;
            }

        }

        public DynamicState(SortedList<double, Matrix> stateData, DynamicStateType type, EOMS eoms, double stateDataTimeStep, XmlNode integratorNode)
        {
            _stateData = stateData;
            _type = type;
            _eoms = eoms;
            _stateDataTimeStep = stateDataTimeStep;
            _intergratorNode = integratorNode;
        }

        public Matrix IC()
        {
            return _stateData[0.0];
        }

        public void Add(Double simTime, Matrix dynamicState)
        {
            this._stateData.Add(simTime, dynamicState);
        }

        /// <summary>
        /// Returns the current position of the system at the time simTime.
        /// </summary>
        /// <param name="simTime"></param>
        /// <returns></returns>
        public Matrix PositionECI(double simTime)
        {
            Matrix initState = _stateData[0];
            double JD = simTime / 86400.0 + SimParameters._simStartJD;

            bool hasrun = !(_stateData.Count == 1);

            if (_type == DynamicStateType.STATIC_LLA)
                return GeometryUtilities.LLA2ECI(initState, JD);
            else if (_type == DynamicStateType.STATIC_ECI)
                return initState;
            else if (_type == DynamicStateType.PREDETERMINED_LLA)
            {
                if (hasrun)
                {
                    //return LLA2ECI((posData.lower_bound(simTime))->second, JD);
                    return GeometryUtilities.LLA2ECI(this[simTime], JD);
                }
                else
                {
                    Console.WriteLine("Integrating and resampling position data... ");
                    double[] vals = new double[2] { 0, SimParameters._simEndSeconds };
                    Integrator solver;
                    bool rk45;
                    // Update the integrator parameters using the information in the XML Node
                    setIntegratorParams(solver);

                    if (rk45)
                    {
                        solver.setParam("nsteps", (vals[1] - vals[0]) / schedParams::SIMSTEP_SECONDS());
                        _stateData = solver.rk45(eomsObject, new Matrix(vals), initState).ODE_RESULT;
                    }
                    else
                    {
                        Console.WriteLine("Using rk4 integrator...");
                        _stateData = solver.rk4(eomsObject, new Matrix(2, 1, vals), initState).ODE_RESULT;
                    }
                    Console.WriteLine("DONE!");
                    //return LLA2ECI((posData.lower_bound(simTime))->second, JD);			
                    return GeometryUtilities.LLA2ECI(this[simTime], JD);
                }
            }
            else if (_type == DynamicStateType.PREDETERMINED_ECI)
            {
                if (hasrun)
                {
                    //return (posData.lower_bound(simTime))->second;
                    return this[simTime];
                }
                else
                {
                    Console.WriteLine("Integrating and resampling position data... ");
                    this->setPropagationModel();

                    double[] vals = new double[2] { 0, SimParameters._simEndSeconds };
                    if (this->propagatorType == RK45)
                    {
                        
                        Integrator solver;

                        // Update the integrator parameters using the information in the XML Node
                        setIntegratorParams(solver);

                        solver.setParam("nsteps", (vals[1] - vals[0]) / schedParams::SIMSTEP_SECONDS());

                        Console.WriteLine("Using rk45 integrator...");
                        _stateData = solver.rk45(eomsObject, new Matrix(2, 1, vals), initState).ODE_RESULT;
                    }
                    else if (this->propagatorType == RK4)
                    {
                          Integrator solver;

                        // Update the integrator parameters using the information in the XML Node
                        setIntegratorParams(solver);

                        Console.WriteLine("Using rk4 integrator...");
                        _stateData = solver.rk4(eomsObject, new Matrix(2, 1, vals), initState).ODE_RESULT;
                    }
                    else if (this->propagatorType == SGP4)
                    {
                        _stateData"Using sgp4 integrator... which is not implemented...");
                        // creat an sgp4 object and propagate
                    }
                    Console.WriteLine("DONE!");

                    return this[simTime];
                }
            }
            else
                return null;
        }

 
        /// <summary>
        /// Gets and Sets the dynamic state of an asset in inertial coordinates at the given simulation time.
        /// This method overwrites any existing state data at simTime.
        /// If the dynamic state data does not exist at simTime, the value returned is a linear interpolation
        /// of existing data.
        /// </summary>
        /// <param name="simTime">The simulation time key</param>
        /// <returns>The inertial dynamic state date of the asset</returns>
        public Matrix this[Double simTime]
        {
            get
            {
                // TODO: if the stateData does not exist at 'simTime' interprolate...
                return _stateData[simTime];
            }
            set
            {
                _stateData[simTime] = value;
            }
        }
    }

    public enum DynamicStateType { STATIC_LLA, STATIC_ECI, PREDETERMINED_LLA, PREDETERMINED_ECI, DYNAMIC_LLA, DYNAMIC_ECI };
    public enum PropagationType { TRAPZ, RK4, RK45, SPG4 };
}
