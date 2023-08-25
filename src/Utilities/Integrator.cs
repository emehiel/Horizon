// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public static class Integrator
    {
        /*
        public IntegratorOptions Options { get; set; }

        public Integrator()
        {
            this.Options = new IntegratorOptions();
        }
        */
        public static Matrix<double> RK45(EOMS dynamics, Matrix<double> timeSpan, Vector initialState, IntegratorOptions options, IntegratorParameters param)
        {
            return Integrator.RK45Helper(dynamics, timeSpan, initialState, options, param);
        }

        public static Matrix<double> RK45(EOMS dynamics, Matrix<double> timeSpan, Matrix<double> initialState)
        {
            IntegratorOptions options = new IntegratorOptions();
            IntegratorParameters param = new IntegratorParameters();
            return Integrator.RK45Helper(dynamics, timeSpan, initialState, options, param);
        }

        private static Matrix<double> RK45Helper(EOMS dynamics, Matrix<double> timeSpanData, Matrix<double> initialState, IntegratorOptions options, IntegratorParameters param)
        {

            // Make timeSpanData and initialState Column vectors
            if (timeSpanData.NumRows < timeSpanData.NumCols)
                timeSpanData = Matrix<double>.Transpose(timeSpanData);

            if (initialState.NumRows < initialState.NumCols)
                initialState = Matrix<double>.Transpose(initialState);

            int nsteps = 0;
            int nfailed = 0;
            int nfevals = 0;
            int refine = 4;
            int nout = 0;
            int nout_old = 0;
            int nout_new = 0;
            int neq = initialState.Length;

            Matrix<double> S = new Matrix<double>(new double[,] { { 1.0, 2.0, 3.0 } }) / refine;

            double t0 = timeSpanData[1, 1];
            double tFinal = timeSpanData[2, 1];
            double tSpan = tFinal - t0;
            Matrix<double> tref = new Matrix<double>(1, 1);
            Matrix<double> tout_new = new Matrix<double>(1, refine);

            double t = t0;
            Matrix<double> y = initialState;
            Matrix<double> yout_new = new Matrix<double>(neq, 1);

            MatrixIndex idx = new MatrixIndex();

            int chunk = (int)System.Math.Min(System.Math.Max(100, 50 * refine), refine + System.Math.Floor(System.Math.Pow(2, 13) / neq));
            Matrix<double> tout = new Matrix<double>(1, chunk);
            Matrix<double> yout = new Matrix<double>(neq, chunk);

            nout = 1;
            tout[nout] = t;
            yout[":", nout] = y;

            double expon = 1.0 / 5.0;
            double temph = 0.0;
            Matrix<double> A = new Matrix<double>(new double[,] { { 1.0 / 5.0, 3.0 / 10.0, 4.0 / 5.0, 8.0 / 9.0, 1.0, 1.0 } }); // 1x6
            Matrix<double> B = new Matrix<double>(new double[,] { {1.0/5.0, 3.0/40.0,  44.0/45.0,  19372.0/6561.0,  9017.0/3168.0,   35.0/384.0}, //7x6
					                                  {0.0,     9.0/40.0, -56.0/15.0, -25360.0/2187.0, -355.0/33.0,      0.0},
					                                  {0.0,     0.0,       32.0/9.0,   64448.0/6561.0,  46732.0/5247.0,  500.0/1113.0},
					                                  {0.0,     0.0,       0.0,       -212.0/729.0,     49.0/176.0,      125.0/192.0},
					                                  {0.0,     0.0,       0.0,        0.0,            -5103.0/18656.0, -2187.0/6784.0},
					                                  {0.0,     0.0,       0.0,        0.0,             0.0,             11.0/84.0},
					                                  {0.0,     0.0,       0.0,        0.0,             0.0,             0.0}});

            Matrix<double> hA = new Matrix<double>(A.Size[1], A.Size[2]);
            Matrix<double> hB = new Matrix<double>(B.Size[1], B.Size[2]);
            Matrix<double> E = new Matrix<double>(new double[,] { { 71.0 / 57600.0 }, { 0.0 }, { -71.0 / 16695.0 }, { 71.0 / 1920.0 }, { -17253.0 / 339200.0 }, { 22.0 / 525.0 }, { -1.0 / 40.0 } }); // 7x1

            Matrix<double> f = new Matrix<double>(neq, 7);

            double htSpan = System.Math.Abs(tSpan);
            double hmax = System.Math.Min(htSpan, 0.1 * htSpan);
            double hmin = 16*options.eps;
            double absh = System.Math.Min(hmax, htSpan);

            double rtol = options.rtol;
            double atol = options.atol;
            int tdir = 1;

            double threshold = atol / rtol;
            int next = 2;

            if ((tFinal - t0) > 0)
                tdir = 1;
            else
                tdir = -1;

            // Compute an initial step size h using y'(t).
            Matrix<double> f0 = dynamics[t0, initialState, param];
            
            Matrix<double> temp = Matrix<double>.Max(Matrix<double>.Abs(initialState), threshold);

            // TODO: Assert and throw expection if fo and temp are not the same size (or force f0 and temp to be the same size)
            double tempSingle = (double)Matrix<double>.Max(Matrix<double>.Abs(f0 / temp));
            double rh = tempSingle / (0.8 * System.Math.Pow(rtol, expon));

            if (absh * rh > 1)
                absh = 1 / rh;

            absh = System.Math.Max(absh, hmin);

            //f.setColumn(1,f0);
            f[":", 1] = f0;

            // THE MAIN RK45 LOOP

            Matrix<double> ynew = y;
            double tnew = t;
            bool done = false;
            bool nofailed = true;
            int nStep = 1;
            double err = 0.0;
            double h;

            while (!done)
            {
                // By default, hmin is a small number such that t+hmin is only slightly
                // different than t.  It might be 0 if t is 0.
                hmin = 16*options.eps;
                //hmin = args.getParam("h");
                absh = System.Math.Min(hmax, System.Math.Max(hmin, absh));
                //absh = min(hmax, max(hmin, absh));
                h = tdir * absh;

                // Stretch the step if within 10% of tfinal-t.
                //if (1.1*absh >= fabs(tFinal - t)){
                if (1.1 * absh >= System.Math.Abs(tFinal - t))
                {
                    h = tFinal - t;
                    absh = System.Math.Abs(h);
                    //absh = fabs(h);
                    done = true;
                }

                // LOOP FOR ADVANCING ONE STEP.
                nofailed = true;    // no failed attempts
                while (true)
                {
                    hA = h * A;
                    hB = h * B;

                    for (int i = 1; i < 6; i++)
                        f[":", i + 1] = dynamics[t + hA[1, i], y + f * hB[":", i], param];

                    tnew = t + hA[1, 6];
                    if (done)
                        tnew = tFinal;   // Hit end point exactly.

                    h = tnew - t;      // Purify h

                    ynew = y + f * hB[":", 6];

                    f[":", 7] = dynamics[tnew, ynew, param];

                    nfevals = nfevals + 6;

                    //err = absh * norm(      (f * E) ./ max (max ( abs(y), abs(ynew)), threshold),inf);
                    err =(double) Matrix<double>.Max(Matrix<double>.Abs((f * E) / Matrix<double>.Max(Matrix<double>.Max(Matrix<double>.Abs(y), Matrix<double>.Abs(ynew)), threshold)));
                    err = absh * err;

                    //    % Accept the solution only if the weighted error is no more than the
                    //    % tolerance rtol.  Estimate an h that will yield an error of rtol on
                    //    % the next step or the next try at taking this step, as the case may be,
                    //    % and use 0.8 of this value to avoid failures.

                    if (err > rtol)
                    {		// Failed Step
                        nfailed++;

                        //			if (absh <= hmin)
                        //				 return;
                        //			  end

                        if (nofailed)
                        {
                            nofailed = false;
                            absh = System.Math.Max(hmin, absh * System.Math.Max(0.1, 0.8 * System.Math.Pow(rtol / err, expon)));
                        }
                        else
                            absh = System.Math.Max(hmin, 0.5 * absh);

                        h = tdir * absh;
                        done = false;
                    }
                    else  //  Successful step
                    {
                        break;
                    }
                }
                nsteps++;

                tref = t + (tnew - t) * S;
                nout_new = refine;
                tout_new = Matrix<double>.Horzcat(tref, tnew);
                yout_new = Matrix<double>.Horzcat(ntrp45(tref, t, y, h, f), ynew);

                if (nout_new > 0)
                {
                    nout_old = nout;
                    nout += nout_new;
                    if (nout > tout.Length)
                    {
                        tout = Matrix<double>.Horzcat(tout, new Matrix<double>(1, chunk));
                        yout = Matrix<double>.Horzcat(yout, new Matrix<double>(neq, chunk));
                    }
                    idx = new MatrixIndex(nout_old + 1, nout);
                    tout[1, idx] = tout_new;
                    yout[":", idx] = yout_new;
                }

                if (nofailed)
                {
                    // Note that absh may shrink by 0.8, and that err may be 0.
                    temph = 1.25 * System.Math.Pow(err / rtol, expon);
                    if (temph > 0.2)
                        absh = absh / temph;
                    else
                        absh = 5.0 * absh;
                }

                t = tnew;
                y = ynew;

                f[":", 1] = f[":", 7];
            }

            Matrix<double> SolverReturnMatrix = new Matrix<double>(neq + 1, nout);

            SolverReturnMatrix[1, ":"] = tout[1, new MatrixIndex(1, nout)];
            SolverReturnMatrix[new MatrixIndex(2, neq + 1), ":"] = yout[":", new MatrixIndex(1, nout)];
            return SolverReturnMatrix;
        }

        private static Matrix<double> ntrp45(Matrix<double> tinterp, double t, Matrix<double> y, double h, Matrix<double> f)
        {
	        Matrix<double> BI = new Matrix<double>(new double[,] {{1.0,	-183.0/64.0,     37.0/12.0,    -145.0/128.0},
					                              {0.0,  0.0,            0.0,           0.0},
                                                  {0.0,  1500.0/371.0,  -1000.0/159.0,  1000.0/371.0},
                                                  {0.0, -125.0/32.0,     125.0/12.0,   -375.0/64.0}, 
                                                  {0.0,  9477.0/3392.0, -729.0/106.0,   25515.0/6784.0},
                                                  {0.0, -11.0/7.0,       11.0/3.0,     -55.0/28.0},
                                                  {0.0,  3.0/2.0,       -4.0,           5.0/2.0}});


            Matrix<double> s = (tinterp - t)/h;
            Matrix<double> S = new Matrix<double>(4, s.NumCols);

            for (int i = 1; i <= 4; i++)
                S.SetRow(i, s);

            Matrix<double> Y = new Matrix<double>(y.NumRows, tinterp.NumCols);

            for (int i = 1; i <= Y.NumCols; i++)
                Y.SetColumn(i, y);

            Matrix<double> yinterp = new Matrix<double>(y.NumRows, tinterp.NumCols);

            yinterp = Y + f*(h*BI)*Matrix<double>.Cumprod(S);

            return yinterp;
        }
    }
}
