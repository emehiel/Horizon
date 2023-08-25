using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace HSFUniverse
{
    public class Moon
    {
        #region Attributes
        public static readonly double Mass = 0.07346e24; // kg
        #endregion
        public Moon()
        {

        }

        public double deg2rad(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        // implemented a better method for this 
        public Vector rMoonold(double jd)
        {

            double RE = 6378;

            // ...Time in centuries since J2000:
            double T = (jd - 2451545) / 36525;
            // ...Ecliptic longitude(deg):
            double e_long = 218.32 + 481267.881 * T + 6.29 * Math.Sin(deg2rad(135.0 + 477198.87 * T)) - 1.27 * Math.Sin(deg2rad(259.3 - 413335.36 * T)) +
                    +0.66 * Math.Sin(deg2rad(235.7 + 890534.22 * T)) + 0.21 * Math.Sin(deg2rad(269.9 + 954397.74 * T))
                    - 0.19 * Math.Sin(deg2rad(357.5 + 35999.05 * T)) - 0.11 * Math.Sin(deg2rad(186.5 + 966404.03 * T));
            e_long = (e_long % 360);

            //...Ecliptic latitude(deg):   
            double e_lat = 5.13 * Math.Sin(deg2rad(93.3 + 483202.02 * T)) + 0.28 * Math.Sin(deg2rad(228.2 + 960400.89 * T))
                    - 0.28 * Math.Sin(deg2rad(318.3 + 6003.15 * T)) - 0.17 * Math.Sin(deg2rad(217.6 - 407332.21 * T));
            e_lat = e_lat % 360;


            // ...Horizontal parallax(deg):    
            double h_par = 0.9508
                    + 0.0518 * Math.Cos(deg2rad(135.0 + 477198.87 * T)) + 0.0095 * Math.Cos(deg2rad(259.3 - 413335.36 * T))
                    + 0.0078 * Math.Cos(deg2rad(235.7 + 890534.22 * T)) + 0.0028 * Math.Cos(deg2rad(269.9 + 954397.74 * T));
            h_par = h_par % 360;

            // ...Angle between earth's orbit and its equator (deg):
            double obliquity = 23.439291 - 0.0130042 * T;

            // ...Direction cosines of the moon's geocentric equatorial position vector:
            double l = Math.Cos(deg2rad(e_lat)) * Math.Cos(deg2rad(e_long));
            double m = Math.Cos(deg2rad(obliquity)) * Math.Cos(deg2rad(e_lat)) * Math.Sin(deg2rad(e_long)) - Math.Sin(deg2rad(obliquity)) * Math.Sin(deg2rad(e_lat));
            double n = Math.Sin(deg2rad(obliquity)) * Math.Cos(deg2rad(e_lat)) * Math.Sin(deg2rad(e_long)) + Math.Cos(deg2rad(obliquity)) * Math.Sin(deg2rad(e_lat));

            // ...Earth - moon distance(km):
            double dist = RE / Math.Sin(deg2rad(h_par));
            var rmoonl = new List<double>()
                    {
                        l,
                       m,
                        n
                    };

            //...Moon's geocentric equatorial position vector (km):
            Vector r_moon = dist * new Vector(rmoonl);


            return r_moon;
        }



    }
}
