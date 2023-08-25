// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;


namespace HSFUniverse
{
    [Serializable]
    public class OrbitalEOMS: DynamicEOMS
    {
        protected double _mu;
        protected Matrix<double> _A;

        public OrbitalEOMS()
        {
            _mu = 398600.4418;
            _A = new Matrix<double>(6);
            _A[1, 4] = 1.0;
            _A[2, 5] = 1.0;
            _A[3, 6] = 1.0;

        }

        public override Matrix<double> this[double t, Matrix<double> y, IntegratorParameters param, Domain environment]
        {
            get
            {
                double r3 = System.Math.Pow(Matrix<double>.Norm(y[new MatrixIndex(1, 3), 1]), 3); // new MatrixIndex akin to colon in matlab
                double mur3 = -_mu / r3;

                // Zonal Harmonics
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

                double r2 = System.Math.Pow(Matrix<double>.Norm(y[new MatrixIndex(1, 3), 1]), 2); // norm position ^2
                double r4 = System.Math.Pow(Matrix<double>.Norm(y[new MatrixIndex(1, 3), 1]), 4); // norm position ^4
                double r5 = System.Math.Pow(Matrix<double>.Norm(y[new MatrixIndex(1, 3), 1]), 5); // norm position ^5
                double r6 = System.Math.Pow(Matrix<double>.Norm(y[new MatrixIndex(1, 3), 1]), 6); // norm position ^6
                double r7 = System.Math.Pow(Matrix<double>.Norm(y[new MatrixIndex(1, 3), 1]), 7); // norm position ^7
                double r9 = System.Math.Pow(Matrix<double>.Norm(y[new MatrixIndex(1, 3), 1]), 9); // norm position ^9

                double ri = y[1]; // x comp of position (ECI)
                double rj = y[2]; // y comp of position (ECI)
                double rk = y[3]; // z comp of position (ECI)

                double rk2 = System.Math.Pow(rk, 2); // z comp of position ^2
                double rk3 = System.Math.Pow(rk, 3); // z comp of position ^3
                double rk4 = System.Math.Pow(rk, 4); // z comp of position ^4
                double rk6 = System.Math.Pow(rk, 6); // z comp of position ^6

                // J2
                double J2ai = ((-3 * J2 * _mu * re2 * ri) / (2 * r5)) * (1 - 5 * rk2 / r2);
                double J2aj = ((-3 * J2 * _mu * re2 * rj) / (2 * r5)) * (1 - 5 * rk2 / r2);
                double J2ak = ((-3 * J2 * _mu * re2 * rk) / (2 * r5)) * (1 - 5 * rk2 / r2);
                J2a = new Matrix<double>[6, 1];
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
                J3a = new Matrix<double>[6, 1];
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
                J4a = new Matrix<double>[6, 1];
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
                J5a = new Matrix<double>[6, 1];
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
                J6a = new Matrix<double>[6, 1];
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
                double cr = 1; // -> Assumed Black body (all momentum absorbed, none reflected) (move to nodes)
                double AvgA = .36 * .36; //  Average Area -> Assumed side of 27U cubesat (move to nodes)
                double m = 54; // Mass -> Assumed mass of 27U cubesat (move to nodes)
                double SRPcoeff = -Psrp * cr * AvgA / m;
                double aSRP = SRPcoeff * Rsat2sun / rsat2sun * Shadow; // need to find/build Rsat2sun & shadow check (How complex? umbra vs panumbra)

                // Drag
                // for density model if needed 
                double r = System.Math.Pow(r2, 0.5); // norm of position vect
                double alt = 6371 - r; // altitude 

                double density = 1; // need density
                ang_vel_E = new Matrix<double>[3, 1]; // rad/sec angular velocity of Earth
                ng_vel_E[1] = 0;
                ng_vel_E[2] = 0;
                ng_vel_E[3] = 72.9211e-6;

                vel = y[new MatrixIndex(4, 6), 1];
                pos = y[new MatrixIndex(1, 3), 1];
                vrel = vel - cross(ang_vel_E, pos);
                vrel_mag = Matrix<double>.Norm(vrel);
                double[,] aDrag = new double[3, 1];
                aDrag = -0.5 * 1 / BC * density * System.Math.Pow(vrel_mag, 2) * (vrel / vrel_mag);

                // NBody
                // need Rsat2sun
                double[,] aNSun = new double[3, 1];
                double muS = 1.32712440018e11;
                aNSun = muS * (Rsat2sun / System.Math.Pow(rsat2sun, 3)) - (vrel / vrel_mag);

                // Move to XML
                double J2switch = 1;
                double J3switch = 1;
                double J4switch = 1;
                double J5switch = 1;
                double J6switch = 1;
                double SRPswitch = 1;
                double Dragswitch = 1;
                double NSunswitch = 1;

                // Build A
                _A[4, 1] = mur3;
                _A[5, 2] = mur3;
                _A[6, 3] = mur3;

                // Calculate output
                Matrix<double> dy = _A * y + J2a * J2switch + J3a * J3switch + J4a * J4switch + J5a * J5switch + J6a * J6switch + aSRP * SRPswitch + aDrag * Dragswitch + aNSun * NSunswitch; // 6x1 vel 1:3 accel 4:6 => add [0;0;0;accelx;y;z]
                return dy;
            }
        }
    }
}
