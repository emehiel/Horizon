using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using System.Xml;

namespace HSFUniverse
{
    public class OrbitalPertEOMS : DynamicEOMS
    {
        protected double _mu;
        protected Matrix<double> _A;
        protected string J2SwitchIn = "";
        protected string J3SwitchIn = "";
        protected string J4SwitchIn = "";
        protected string J5SwitchIn = "";
        protected string J6SwitchIn = "";
        protected string SRPSwitchIn = "";
        protected string DragSwitchIn = "";
        protected string NSunSwitchIn = "";
        protected string ReflectIn = "";
        protected string AvgAreaIn = "";
        protected string MassIn = "";
        protected string CDIn = "";
        Sun mySun = new Sun(false);
        Moon myMoon = new Moon();

        public OrbitalPertEOMS()
        {
            _mu = 398600.4418;
            _A = new Matrix<double>(6);
            _A[1, 4] = 1.0;
            _A[2, 5] = 1.0;
            _A[3, 6] = 1.0;
        }

        public OrbitalPertEOMS(string J2, string J3, string J4, string J5, string J6, string SRP, string Drag, string NSun, string Reflectivity, string AvgArea, string Mass, string CD)
        {
            _mu = 398600.4418;
            _A = new Matrix<double>(6);
            _A[1, 4] = 1.0;
            _A[2, 5] = 1.0;
            _A[3, 6] = 1.0;

            J2SwitchIn = J2;
            J3SwitchIn = J3;
            J4SwitchIn = J4;
            J5SwitchIn = J5;
            J6SwitchIn = J6;
            SRPSwitchIn = SRP;
            DragSwitchIn = Drag;
            NSunSwitchIn = NSun;

            ReflectIn = Reflectivity;
            AvgAreaIn = AvgArea;
            MassIn = Mass;
            CDIn = CD;
        }

        public override Matrix<double> this[double t, Matrix<double> y, IntegratorParameters param, Domain environment]
        {
            get {   
                // Pert Switches from XML
                int J2switch = Convert.ToInt16(J2SwitchIn);
                int J3switch = Convert.ToInt16(J3SwitchIn);
                int J4switch = Convert.ToInt16(J4SwitchIn);
                int J5switch = Convert.ToInt16(J5SwitchIn);
                int J6switch = Convert.ToInt16(J6SwitchIn);
                int SRPswitch = Convert.ToInt16(SRPSwitchIn);
                int Dragswitch = Convert.ToInt16(DragSwitchIn);
                int NSunswitch = Convert.ToInt16(NSunSwitchIn);

                // Geometry from XML for drag
                double cr = Convert.ToDouble(ReflectIn);
                double AvgA = Convert.ToDouble(AvgAreaIn);
                double m = Convert.ToDouble(MassIn);
                double CD = Convert.ToDouble(CDIn);

                // Set up Parameters
                double J2 = 0.0010826269;
                double J3 = -0.0000025324105;
                double J4 = -0.0000016198976;
                double J5 = -0.00000022775359;
                double J6 = 0.00000054066658;

                double re = 6378.137; // radius of earth (km)
                double re2 = System.Math.Pow(re, 2); // radius of earth ^2
                double re3 = System.Math.Pow(re, 3); // radius of earth ^3
                double re4 = System.Math.Pow(re, 4); // radius of earth ^4
                double re5 = System.Math.Pow(re, 5); // radius of earth ^5
                double re6 = System.Math.Pow(re, 6); // radius of earth ^6

                double r = Matrix<double>.Norm(y[new MatrixIndex(1, 3), 1]);
                double r2 = System.Math.Pow(Matrix<double>.Norm(y[new MatrixIndex(1, 3), 1]), 2); // norm position ^2
                double r3 = System.Math.Pow(Matrix<double>.Norm(y[new MatrixIndex(1, 3), 1]), 3); // new MatrixIndex akin to colon in matlab
                double r4 = System.Math.Pow(Matrix<double>.Norm(y[new MatrixIndex(1, 3), 1]), 4); // norm position ^4
                double r5 = System.Math.Pow(Matrix<double>.Norm(y[new MatrixIndex(1, 3), 1]), 5); // norm position ^5
                double r6 = System.Math.Pow(Matrix<double>.Norm(y[new MatrixIndex(1, 3), 1]), 6); // norm position ^6
                double r7 = System.Math.Pow(Matrix<double>.Norm(y[new MatrixIndex(1, 3), 1]), 7); // norm position ^7
                double r9 = System.Math.Pow(Matrix<double>.Norm(y[new MatrixIndex(1, 3), 1]), 9); // norm position ^9

                double mur3 = -_mu / r3;
                double ri = y[1]; // x comp of position (ECI)
                double rj = y[2]; // y comp of position (ECI)
                double rk = y[3]; // z comp of position (ECI)

                Matrix<double> R = y[new MatrixIndex(1, 3), 1];
                Matrix<double> V = y[new MatrixIndex(4, 6), 1];

                double rk2 = System.Math.Pow(rk, 2); // z comp of position ^2
                double rk3 = System.Math.Pow(rk, 3); // z comp of position ^3
                double rk4 = System.Math.Pow(rk, 4); // z comp of position ^4
                double rk6 = System.Math.Pow(rk, 6); // z comp of position ^6

                // Call Sun.cs for earth sun vect & shadow state
                Matrix<double> Rsat2sun = mySun.getEarSunVec(t);
                double rsat2sun = Matrix<double>.Norm(Rsat2sun);
                ShadowState ShadowValue = mySun.castShadowOnPos2(R, t);
                int Shadow;
                if (ShadowValue == 0)
                {
                    Shadow = 1;
                }
                else
                {
                    Shadow = 0;
                }

                // Zonal Harmonics
                // J2
                double J2ai = ((-3 * J2 * _mu * re2 * ri) / (2 * r5)) * (1 - 5 * rk2 / r2);
                double J2aj = ((-3 * J2 * _mu * re2 * rj) / (2 * r5)) * (1 - 5 * rk2 / r2);
                double J2ak = ((-3 * J2 * _mu * re2 * rk) / (2 * r5)) * (1 - 5 * rk2 / r2);
                Matrix<double> J2a = new Matrix<double>(6, 1);
                J2a[1] = 0;
                J2a[2] = 0;
                J2a[3] = 0;
                J2a[4] = J2ai;
                J2a[5] = J2aj;
                J2a[6] = J2ak;

                // J3
                double J3ai = ((-5 * J3 * _mu * re3 * ri) / (2 * r7)) * (3 * rk - 7 * rk3 / r2);
                double J3aj = ((-5 * J3 * _mu * re3 * rj) / (2 * r7)) * (3 * rk - 7 * rk3 / r2);
                double J3ak = ((-5 * J3 * _mu * re3) / (2 * r7)) * (6 * rk2 - 7 * rk4 / r2 - 3 * r2 / 5);
                Matrix<double> J3a = new Matrix<double>(6, 1);
                J3a[1] = 0;
                J3a[2] = 0;
                J3a[3] = 0;
                J3a[4] = J3ai;
                J3a[5] = J3aj;
                J3a[6] = J3ak;

                // J4
                double J4ai = ((15 * J4 * _mu * re4 * ri) / (8 * r7)) * (1 - 14 * rk2 / r2 + 21 * rk4 / r4);
                double J4aj = ((15 * J4 * _mu * re4 * rj) / (8 * r7)) * (1 - 14 * rk2 / r2 + 21 * rk4 / r4);
                double J4ak = ((15 * J4 * _mu * re4 * rk) / (8 * r7)) * (5 - 70 * rk2 / (3 * r2) + 21 * rk4 / r4);
                Matrix<double> J4a = new Matrix<double>(6, 1);
                J4a[1] = 0;
                J4a[2] = 0;
                J4a[3] = 0;
                J4a[4] = J4ai;
                J4a[5] = J4aj;
                J4a[6] = J4ak;

                // J5
                double J5ai = ((3 * J5 * _mu * re5 * ri * rk) / (8 * r9)) * (35 - 210 * rk2 / r2 + 231 * rk4 / r4);
                double J5aj = ((3 * J5 * _mu * re5 * rj * rk) / (8 * r9)) * (35 - 210 * rk2 / r2 + 231 * rk4 / r4);
                double J5ak = ((3 * J5 * _mu * re5 * rk * rk) / (8 * r9)) * (105 - 315 * rk2 / r2 + 231 * rk4 / r4) - 15 * J5 * _mu * re5 / (8 * r7);
                Matrix<double> J5a = new Matrix<double>(6, 1);
                J5a[1] = 0;
                J5a[2] = 0;
                J5a[3] = 0;
                J5a[4] = J5ai;
                J5a[5] = J5aj;
                J5a[6] = J5ak;

                // J6
                double J6ai = ((-J6 * _mu * re6 * ri) / (16 * r9)) * (35 - 945 * rk2 / r2 + 3465 * rk4 / r4 - 3003 * rk6 / r6);
                double J6aj = ((-J6 * _mu * re6 * rj) / (16 * r9)) * (35 - 945 * rk2 / r2 + 3465 * rk4 / r4 - 3003 * rk6 / r6);
                double J6ak = ((-J6 * _mu * re6 * rk) / (16 * r9)) * (245 - 2205 * rk2 / r2 + 4851 * rk4 / r4 - 3003 * rk6 / r6);
                Matrix<double> J6a = new Matrix<double>(6, 1);
                J6a[1] = 0;
                J6a[2] = 0;
                J6a[3] = 0;
                J6a[4] = J6ai;
                J6a[5] = J6aj;
                J6a[6] = J6ak;

                // SRP
                // need Rsat2sun, rsat2sun, Shadow 
                double SF = 1367; // Solar Rad Const (W/m^2)
                double c = 3e8; // Speed of light (m/s)
                double Psrp = SF / c; // Pressure due to SRP (N/m^2)
                //double cr = 1; // -> Assumed Black body (all momentum absorbed, none reflected) (move to nodes)
                //double AvgA = .36 * .36; //  Average Area -> Assumed side of 27U cubesat (move to nodes)
                //double m = 54; // Mass -> Assumed mass of 27U cubesat (move to nodes)
                double SRPcoeff = -Psrp * cr * AvgA / m;
                Matrix<double> SRPaInter = SRPcoeff * Rsat2sun / rsat2sun * Shadow; // need to find/build Rsat2sun & shadow check (How complex? umbra vs panumbra)
                Matrix<double> SRPa = new Matrix<double>(6, 1);
                SRPa[1] = 0;
                SRPa[2] = 0;
                SRPa[3] = 0;
                SRPa[4] = SRPaInter[1];
                SRPa[5] = SRPaInter[2];
                SRPa[6] = SRPaInter[3];

                // Drag
                // for density model if needed 
                double alt = 6371 - r; // altitude 

                double density = 6.967e-13; // need density
                Matrix<double> ang_vel_E = new Matrix<double>(3, 1); // rad/sec angular velocity of Earth
                ang_vel_E[1] = 0;
                ang_vel_E[2] = 0;
                ang_vel_E[3] = 72.9211e-6;
                Matrix<double> cross = Matrix<double>.Cross(ang_vel_E, R);
                Matrix<double> vrel = V - cross;
                double vrel_mag = Matrix<double>.Norm(vrel);
                double BC = CD * AvgA / m;
                Matrix<double> DragaInter = -0.5 * 1 / BC * density * System.Math.Pow(vrel_mag, 2) * (vrel / vrel_mag);
                Matrix<double> Draga = new Matrix<double>(6, 1);
                Draga[1] = 0;
                Draga[2] = 0;
                Draga[3] = 0;
                Draga[4] = DragaInter[1];
                Draga[5] = DragaInter[2];
                Draga[6] = DragaInter[3];

                // NBody
                // need Rsat2sun
                double muS = 1.32712440018e11;
                Matrix<double> NSunaInter = muS * (Rsat2sun / System.Math.Pow(rsat2sun, 3)) - (vrel / vrel_mag);
                Matrix<double> NSuna = new Matrix<double>(6, 1);
                NSuna[1] = 0;
                NSuna[2] = 0;
                NSuna[3] = 0;
                NSuna[4] = NSunaInter[1];
                NSuna[5] = NSunaInter[2];
                NSuna[6] = NSunaInter[3];

                // Build A
                _A[4, 1] = mur3;
                _A[5, 2] = mur3;
                _A[6, 3] = mur3;

                // Calculate output
                Matrix<double> dy = _A * y + J2a * J2switch + J3a * J3switch + J4a * J4switch + J5a * J5switch + J6a * J6switch + SRPa * SRPswitch + Draga * Dragswitch + NSuna * NSunswitch; // 6x1 vel 1:3 accel 4:6 => add [0;0;0;accelx;y;z]
                return dy;
            }

        }
    }
}
