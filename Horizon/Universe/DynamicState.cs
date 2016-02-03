using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Horizon_Utilities;

namespace Horizon.Universe
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
    class DynamicState
    {
        SortedList<Double, Matrix> stateData;

        private DynamicStateType type;
        DynamicStateType Type
        {
            get
            {
                return this.type;
            }
        }

        public DynamicState(SortedList<double, Matrix> stateData, DynamicStateType type)
        {
            this.stateData = stateData;
            this.type = type;
        }

        public DynamicState(XmlNode positionXmlNode)
        {
            stateData = new SortedList<double, Matrix>();
            this.type = DynamicStateType.STATIC_ECI;

        }

        public void Add(Double simTime, Matrix dynamicState)
        {
            this.stateData.Add(simTime, dynamicState);
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
                return this.stateData[simTime];
            }
            set
            {
                this.stateData[simTime] = value;
            }
        }
    }

    public enum DynamicStateType { STATIC_LLA, STATIC_ECI, PREDETERMINED_LLA, PREDETERMINED_ECI, DYNAMIC_LLA, DYNAMIC_ECI };
}
