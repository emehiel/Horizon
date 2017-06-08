// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserModel;

namespace Utilities
{
    public static class GeometryUtilities
    {
        public static bool hasLOS(Vector posECI1, Vector posECI2)
        {
            /* parameter t is is the parameter that represents where the minimum distance is
            d is a minimum at (posECI1) + t*(posECI2 - posECI1)*/
            double t = -Vector.Dot(posECI1, posECI2 - posECI1) / System.Math.Pow(Vector.Norm(posECI2 - posECI1), 2);
            /* if t > 1 or t < 0, then the minumim distance does not occur along the line segment connecting positions 1 and 2,
            and the two positions are visible from each other*/
            if (t >= 1 || t <= 0)
                return true;
            // calculate the minimum distance to the center of the earth
            double d = Vector.Norm(Vector.Cross(posECI2 - posECI1, posECI1)) / Vector.Norm(posECI2 - posECI1);
            // otherwise compare the minimum distance to the radius of the earth
            return d >= SimParameters.EARTH_RADIUS;
        }
        public static bool hasLOS(Matrix<double> posECI1, Matrix<double> posECI2)
        {
            /* parameter t is is the parameter that represents where the minimum distance is
            d is a minimum at (posECI1) + t*(posECI2 - posECI1)*/
            double t = -Matrix<double>.Dot(posECI1, posECI2 - posECI1) / System.Math.Pow(Matrix<double>.Norm(posECI2 - posECI1), 2);
            /* if t > 1 or t < 0, then the minumim distance does not occur along the line segment connecting positions 1 and 2,
            and the two positions are visible from each other*/
            if (t >= 1 || t <= 0)
                return true;
            // calculate the minimum distance to the center of the earth
            double d = Matrix<double>.Norm(Matrix<double>.Cross(posECI2 - posECI1, posECI1)) / Matrix<double>.Norm(posECI2 - posECI1);
            // otherwise compare the minimum distance to the radius of the earth
            return d >= SimParameters.EARTH_RADIUS;
        }
        public static Vector LLA2ECI(Vector LLA, double JD )
        {
            Vector pos = new Vector(3);

            double lat = LLA[1]; //deg
            double lon = LLA[2]; //deg
            double alt = LLA[3]; //km

            double gst = CT2LST(lon, JD); //deg
            double theta = gst * System.Math.PI / 180.0; //(lon + gst) * M_PI/180.0; //rad

            double r = (SimParameters.EARTH_RADIUS + alt) * System.Math.Cos(lat * System.Math.PI / 180.0); // km

            pos[1] = r * System.Math.Cos(theta);// km
            pos[2] = r * System.Math.Sin(theta);// km
            pos[3] = (alt + SimParameters.EARTH_RADIUS) * System.Math.Sin(lat * System.Math.PI / 180.0);// km

            return pos;
        }

        public static Vector ECI2LLA( Vector ECI, double JD )
        {
            Vector pos = new Vector(3);
            double x = ECI[1];
            double y = ECI[2];
            double z = ECI[3];

            double r = Vector.Norm(ECI);
                double templon = 180 / System.Math.PI * System.Math.Atan2(y, x); //deg
            double diff = templon - CT2LST(templon, JD);
            double lon = templon + diff;

            pos[1] = 180 / System.Math.PI * System.Math.Atan2(z, System.Math.Sqrt(x * x + y * y)); //deg
            pos[2] = lon; //deg
            pos[3] = r - SimParameters.EARTH_RADIUS; //km
            return pos;
        }

        public static double HMS2UT(uint h, uint m, double seconds)
        {
            return h + m / 60.0 + seconds / 3600.0;
        }

        public static double YMDUT2JD( uint y, uint m, uint d, double UT)
        {
            return 367 * y - System.Math.Floor(7.0 * (y + System.Math.Floor((m + 9.0) / 12.0)) / 4.0) + System.Math.Floor(275.0 * m / 9.0) + d + 1721013.5 + UT / 24.0;
        }

        public static double CT2LST( double longitude, double JD)
        {
            double J0 = System.Math.Floor(JD + 0.5) - 0.5;
            double UT = (JD - J0) * 24.0;
            double T0 = (J0 - 2451545.0) / 36525.0;
            double[] c = new double[5] { 100.4606184, 36000.77004, 0.000387933, -2.583e-8, 360.98564724 };
            double g0 = c[0] + c[1] * T0 + c[2] * System.Math.Pow(T0, 2) + c[3] * System.Math.Pow(T0, 3);

            g0 = g0 - 360.0 * System.Math.Floor(g0 / 360.0);

            double gst = g0 + c[4] * UT / 24.0;
            double lst = gst + longitude;

            lst = lst - 360.0 * System.Math.Floor(lst / 360.0);

            return lst;
        }

        public static Vector quat2euler(Matrix<double> q)
        {
            Vector eulerAngles = new Vector(3);

            eulerAngles[1] = System.Math.Atan2(2 * (q[1] * q[2] + q[3] * q[4]), 1 - 2 * (q[1] * q[1] + q[2] * q[2]));
            eulerAngles[2] = System.Math.Asin(2 * (q[1] * q[3] - q[4] * q[1]));
            eulerAngles[3] = System.Math.Atan2(2 * (q[1] * q[4] + q[2] * q[3]), 1 - 2 * (q[3] * q[3] + q[4] * q[4]));

            return eulerAngles;
        }
    }
}
