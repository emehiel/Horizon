using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSFUniverse
{
    public class StandardAtmosphere : Atmosphere
    {
        #region Attributes
        SortedList<double, double[]> lookUpTable = new SortedList<double, double[]>();
        protected new const double IDEAL_GAS = 287.053;
        protected const double EARTH_RADIUS = 6369000.0;
        #endregion

        #region Constructors
        /// <summary>
        /// Implementation of the 1976 standard atmosphere for geometric altitudes below 86 km
        /// </summary>
        /// <remarks>
        /// Standard atmosphere is defined in terms of geopotential altitude.
        /// The geometric altitude is converted to geopotential altitude before evalutation.
        /// 1976 standard atmosphere based on PDAS model <see cref="http://www.pdas.com/atmos.html"/>
        /// validation data found in reference above.
        /// </remarks>
        public StandardAtmosphere()
        {

        }
        #endregion

        #region Methods
        public override void CreateAtmosphere()
        {
            lookUpTable.Add(0, (new double[] { 101325, 288.15, -0.0065 }));
            lookUpTable.Add(11000, (new double[] { 22632.1, 216.65, 0.0 }));
            lookUpTable.Add(20000, (new double[] { 5474.89, 216.65, 0.001 }));
            lookUpTable.Add(32000, (new double[] { 868.019, 228.65, 0.0028 }));
            lookUpTable.Add(47000, (new double[] { 110.906, 270.65, 0.0 }));
            lookUpTable.Add(51000, (new double[] { 66.9389, 270.65, -0.0028 }));
            lookUpTable.Add(71000, (new double[] { 3.95642, 214.65, -0.002 }));
            lookUpTable.Add(84852, (new double[] { 0.37338, 186.946, 0.0 }));
        }

        public override double density(double height)
        {
            double dens = pressure(height) / temperature(height) / IDEAL_GAS;
            return dens;
        }
        public override double pressure(double height)
        {
            double h = height * EARTH_RADIUS / (height + EARTH_RADIUS);

            double key = lookUpTable.TakeWhile(x => x.Key <= h).Last().Key;
            if (lookUpTable[key].ElementAt(2) != 0.0)
            {
                return lookUpTable[key].ElementAt(0) * Math.Pow(temperature(height) /
                    lookUpTable[key].ElementAt(1), -GRAVITY / IDEAL_GAS / lookUpTable[key].ElementAt(2));
            }
            else
            {
                return lookUpTable[key].ElementAt(0) * Math.Exp(-GRAVITY * (h - key) /
                    IDEAL_GAS / lookUpTable[key].ElementAt(1));
            }
        }
        public override double temperature(double height)
        {
            double h = height * EARTH_RADIUS / (height + EARTH_RADIUS);

            double key = lookUpTable.TakeWhile(x => x.Key <= h).Last().Key;
            return lookUpTable[key].ElementAt(1) + lookUpTable[key].ElementAt(2) * (h - key);
        }
        public override double uVelocity(double height)
        {
            return 0.0;
        }
        public override double vVelocity(double height)
        {
            return 0.0;
        }
        #endregion
    }
}
