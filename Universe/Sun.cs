using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using IO;
/**
* Creates a Sun model able to compute the Sun Vector to the Earth as well as 
* determining whether the spacecraft is in the shadow of the Earth.
* @author Brian Butler
* @author Seth Silva
*/

namespace HSFUniverse
{
    enum shadow_state
    {
        NO_SHADOW,
        UMBRA,
        PENUMBRA
    };
    public class Sun
    {
        public static readonly int solar_Constant = 1366;
        private bool _isSunVecConstant;
        private Matrix<double> esVec;

        /**
        * The constructor for the Sun class.
        */
        public Sun()
        {
            _isSunVecConstant = false;
            esVec = getEarSunVec(0.0);
        }

        /**
        * The constructor for the Sun class.
        * @param isSunVecConstant A flag that determines whether to hold
        * the Sun vector constant. If set to true, future calls to getEarSunVec(const double simTime)
        * will always return the value at a <CODE>simTime</CODE> of 0 seconds.
        */
        public Sun(bool isSunVecConstant)
        {
            _isSunVecConstant = isSunVecConstant;
        }


        /**
        * Calculates the Earth-Sun vector in [km] according to the simualtion time.
        * If the Sun is created with isEarthSunVecConstant set to true, the value for the
        * vector at a simulation time of 0 seconds will be alwyas be used.
        * Code from "Fundamentals of Astrodynamics and Applications."
        * @param simTime the simulation time at which the Earth-Sun vector is required
        * @return a Matrix containing the Earth-Sun vector in ECI.
        */
        public Matrix<double> getEarSunVec(double simTime){
            if (_isSunVecConstant && esVec.NumCols != 0 && esVec.NumRows != 0) // != Matrix()
                return esVec;

            Matrix<double> RSun = new Matrix<double>(3, 1, 0.0);
            double eclLong, meanLongSun, MASun, obl, rSun, TUt1, TTdb;
            double JDUt1 = (simTime / 86400) + SimParams.simStart_JD;

            const double aU = 149597870.0;
            const double rad = Math.PI / 180;


            // Computing the number of Julian centuries from the epoch:  
            TUt1 = (JDUt1 - 2451545.0) / 36525;

            // Computing the Mean longitude of the Sun:  
            meanLongSun = 280.460 + 36000.77 * TUt1;

            // Put into range of 0 to 360 degrees
            if (meanLongSun < 0)
            {
                double meanLongSunDiv = Math.Floor(-1 * meanLongSun / 360);
                meanLongSun = meanLongSun + (meanLongSunDiv + 1) * 360;
            }
            else if (meanLongSun > 360)
            {
                double meanLongSunDiv = Math.Floor(meanLongSun / 360);
                meanLongSun = meanLongSun - meanLongSunDiv * 360;
            }
            //end if //

            // Juliamn centuries of Barycentric dynamical time are assumed to be equal
            // to the number of Julian centuries from the epoch:  
            TTdb = TUt1;

            // Computing the Mean Anomaly of the sun: 
            MASun = 357.5277233 + 35999.05034 * TTdb;

            // Put into range of 0 to 360 degrees
            if (MASun < 0)
            {
                double MASunDiv = Math.Floor(-1 * MASun / 360);
                MASun = MASun + (MASunDiv + 1) * 360;
            }
            else if (MASun > 360)
            {
                double MASunDiv = Math.Floor(MASun / 360);
                MASun = MASun - MASunDiv * 360;
            }

            // Computing the ecliptic longitude:  
            eclLong = meanLongSun + 1.914666471 * Math.Sin(MASun * rad) + 0.019994643 * Math.Sin(2 * MASun * rad);

            // Computing the sun-centered position vector from the Sun to Earth:  
            rSun = 1.000140612 - 0.016708617 * Math.Cos(MASun * rad) - 0.000139589 * Math.Cos(2 * MASun * rad);

            // Computing the obliquity of the ecliptic:  
            obl = 23.439291 - 0.0130042 * TTdb;

            // Transforming sun_centered Earth position vector to a geocentric
            // equatorial position vector:
            RSun.SetValue(1, 1, rSun * Math.Cos(eclLong * rad) * aU);
            RSun.SetValue(2, 1, rSun * Math.Cos(obl * rad) * Math.Sin(eclLong * rad) * aU);
            RSun.SetValue(3, 1, rSun * Math.Sin(obl * rad) * Math.Sin(eclLong * rad) * aU);

            //if(isSunVecConstant)
            //	esVec = RSun;

            return (RSun);
        }//End getEarthSunVec method


        /**
        * Casts a shadow on the specified Position.
        * Computes whether the position matrix given is located in the shadow of 
        * the Earth, and it determines which shadow: the Penumbra or Umbra. This
        * function calls the getEarSunVec function to retrieve the Earth-Sun vector. 	
        * Code from "Fundamentals of Astrodynamics and Applications"
        * @param time the simulation time that the shadow determination is requested
        * @returns UMBRA, PENUMBRA, or NO_SHADOW depending on the case.
        */
        shadow_state castShadowOnPos(DynamicState pos, double simTime)
        {
            double penVert;
            double satHoriz;
            double satVert;
            double sigma;
            double umbVert;
            shadow_state shadow;

            const double alphaPen = 0.26900424;
            const double alphaUmb = 0.26411888;
            const double rad = Math.PI / 180;
            const double rEar = 6378.137;

            // Get earth-sun vector
            Matrix<double> rSun = getEarSunVec(simTime);
            // Get the vector from the earth to the object
            Matrix<double> assetPosAtTime = pos.PositionECI(simTime); //TODO: this method is not yet implemented
            double dot_p = Matrix<double>.Dot((-rSun), assetPosAtTime);
            // Calculate the cosine of the angle between the position vector
            // and the axis the earth-sun vector lies on
            double arg = (dot_p) / (Matrix<double>.Norm(-rSun) * Matrix<double>.Norm(assetPosAtTime));

	        //fix argument, must be between -1 and 1
	        if(Math.Abs(arg) > 1)
		        arg = arg/Math.Abs(arg)*Math.Floor(Math.Abs(arg));
	        sigma = Math.Acos(arg);
            // Calculate the distance from the 
            satHoriz = Matrix<double>.Norm(assetPosAtTime)*Math.Cos(sigma);
            satVert  = Matrix<double>.Norm(assetPosAtTime)*Math.Sin(sigma);

            // Calculate distance away from earth-sun axis where penumbra ends
            penVert = rEar + Math.Tan(alphaPen* rad)*satHoriz;

            // determine the shadow state of the position
            if (dot_p > 0 && satVert <= penVert)
            {
                shadow = shadow_state.PENUMBRA;
                //Calculate distance away from earth-sun axis where umbra ends
                umbVert = rEar - Math.Tan(alphaUmb * rad) * satHoriz;

                if (satVert <= umbVert)
                    shadow = shadow_state.UMBRA;
            }
            else
                shadow = shadow_state.NO_SHADOW;

	        return(shadow);
        }//End castShadowOnPos method


    } //end Sun Class


}//End Universe Namespace
