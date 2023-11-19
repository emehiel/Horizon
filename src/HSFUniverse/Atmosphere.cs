using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using Utilities;
using System.Diagnostics.CodeAnalysis;

namespace HSFUniverse
{
    // TODO: Need to rethink public vs private. 
    // TODO: Need to find alternative data source. I thought my current source was 2 months. It is only about a week. 
    // TODO: make it possible to specify a specfic filename/path easily
    [Serializable]
    //[ExcludeFromCodeCoverage]
    public abstract class Atmosphere
    {
        protected SortedList<double, double> uVelocityData;
        protected SortedList<double, double> vVelocityData;
        protected SortedList<double, double> pressureData;
        protected SortedList<double, double> temperatureData;
        public SortedList<double, double> densityData;

        protected const double GRAVITY = 9.80665;
        protected const double IDEAL_GAS = 286.9;

        abstract public double temperature(double height);
        abstract public double pressure(double height);
        abstract public double density(double height);
        abstract public double uVelocity(double height);
        abstract public double vVelocity(double height);

        abstract public void CreateAtmosphere();


    }
    //[ExcludeFromCodeCoverage]
    public class HorizontalWindModel14
    {
        [DllImport(@"C:\Users\steve\Desktop\HWM\hwm14.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern void hwm14([In] int iyd, [In] float sec, [In]  float alt, [In] float glat, [In] float glon, [In] float stl, [In] float f107a, [In] float f107,  [In] float ap, [Out] float w);
        public static Vector hwm14Interface(int iyd, float sec, float alt, float glat, float glon, float stl, float[] ap)
        {
            //inithwm();
            float[] w = new float[2];
            float f107a = 100;
            float f107 = 100;
            hwm14( iyd,  sec, alt, glat, glon, stl, f107a, f107, 0, 0);
            return new Vector(new List<double>(new double[]{ 0, Convert.ToDouble(w[1]) }));
        }
    }
            
}
