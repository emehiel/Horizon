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
        public static Matrix<double> RK45(EOMS dynamics, Matrix<double> timeSpan, Matrix<double> initialState, IntegratorOptions options)
        {
            return Integrator.RK45Helper(dynamics, timeSpan, initialState, options);
        }

        public static Matrix<double> RK45(EOMS dynamics, Matrix<double> timeSpan, Matrix<double> initialState)
        {
            IntegratorOptions options = new IntegratorOptions();
            return Integrator.RK45Helper(dynamics, timeSpan, initialState, options);
        }

        private static Matrix<double> RK45Helper(EOMS dynamics, Matrix<double> timeSpanData, Matrix<double> initialState, IntegratorOptions options)
        {
            Double expon = 1.0 / 5.0;
            Double temph = 0.0;
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
            Matrix<double> E = new Matrix<double>(new double[,] { { 71.0 / 57600.0, 0.0, -71.0 / 16695.0, 71.0 / 1920.0, -17253.0 / 339200.0, 22.0 / 525.0, -1.0 / 40.0 } }); // 7x1

            //Complex neq_temp = (Matrix<T>.Max(initialState.Size))[1].Re;
            int neq = Matrix<int>.Max(initialState.Size)[1];
            //short neq = (Double)(Matrix<T>.Max(initialState.Size)[1]);
            Matrix<double> f = new Matrix<double>(neq, 7);

            double t0;
            double tFinal;
            int ntstep;

            Matrix<double> timeSpan;

            // Make timeSpanData and initialState Column vectors
            if (timeSpanData.NumRows > timeSpanData.NumCols)
                timeSpanData = Matrix<double>.Transpose(timeSpanData);

            if (initialState.NumRows > initialState.NumCols)
                initialState = Matrix<double>.Transpose(initialState);

            int nr = timeSpanData.Size[1, 1];
            int nc = timeSpanData.Size[1, 2];

            if (System.Math.Max(nr, nc) > 2)
            {
                t0 = timeSpanData[1, 1];
                tFinal = timeSpanData[1, nc];
                ntstep = nc;
                timeSpan = timeSpanData;
            }
            else
            {
                t0 = timeSpanData[1, 1];
                tFinal = timeSpanData[1, 2];

                ntstep = options.nSteps; ;
                Matrix<double> tvals = new Matrix<double>(1, ntstep + 1);
                double tstep = (tFinal - t0) / (ntstep);

                for (int i = 1; i <= ntstep + 1; i++)
                    tvals[1, i] = tstep * (i - 1) + t0;

                timeSpan = tvals;
            }

            Matrix<double> SolverReturnMatrix = new Matrix<double>(timeSpan.Size[1], initialState.Size[2] + 1);

            Double hmin = options.eps;
            Double hmax = 0.1 * (tFinal - t0);
            Double rtol = options.rtol;
            Double atol = options.atol;
            int tdir = 1;

            uint nfailed = 0;
            uint nfevals = 0;
            Double threshold = atol / rtol;
            int next = 2;

            if ((tFinal - t0) > 0)
                tdir = 1;
            else
                tdir = -1;

            // Compute an initial step size h using y'(t).
            Matrix<double> f0 = dynamics[t0, initialState];

            Double absh = System.Math.Abs(hmax);

            Matrix<double> temp = Matrix<double>.Max(Matrix<double>.Abs(initialState), threshold);
            double tempSingle = (double)Matrix<double>.Max(Matrix<double>.Abs(f0 / temp));
            double rh = tempSingle / (0.8 * System.Math.Pow(rtol, expon));

            if (absh * rh > 1)
                absh = 1 / rh;

            absh = System.Math.Max(absh, hmin);

            //f.setColumn(1,f0);
            f[":", 1] = f0;

            // THE MAIN RK45 LOOP

            Matrix<double> y = initialState;
            Matrix<double> ynew = y;
            double t = t0;
            double tnew = t;
            bool done = false;
            bool nofailed = true;
            int nStep = 1;
            double err = 0.0;
            double h;

            SolverReturnMatrix[nStep, 1] = t0;
            SolverReturnMatrix[nStep, new MatrixIndex(2, (int)y.Size[2])] = y;

            while (!done)
            {

                // By default, hmin is a small number such that t+hmin is only slightly
                // different than t.  It might be 0 if t is 0.
                hmin = options.h;
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



                    //f.setColumn(2, inFunction->operator()(t+hA(1,1),y+f*hB(colon(1,7),1)));
                    //f.setColumn(3, inFunction->operator()(t+hA(1,2),y+f*hB(colon(1,7),2)));
                    //f.setColumn(4, inFunction->operator()(t+hA(1,3),y+f*hB(colon(1,7),3)));
                    //f.setColumn(5, inFunction->operator()(t+hA(1,4),y+f*hB(colon(1,7),4)));
                    //f.setColumn(6, inFunction->operator()(t+hA(1,5),y+f*hB(colon(1,7),5)));

                    tnew = t + (Double)hA[1, 6];
                    if (done)
                        tnew = tFinal;   // Hit end point exactly.

                    h = tnew - t;      // Purify h

                    //ynew = y + f*hB[colon(1,7),6];
                    ynew = y + f * hB[":", 6];
                    //f.setColumn(7, inFunction->operator()(tnew, ynew));

                    nfevals = nfevals + 6;

                    //			err = absh * norm(      (f * E) ./ max (max ( abs(y), abs(ynew)), threshold),inf);
                    err = absh * Matrix<double>.Max(Matrix<double>.Abs(f * E) / Matrix<double>.Max(Matrix<double>.Max(Matrix<double>.Abs(y), Matrix<double>.Abs(ynew))))[1];
                    //err = (absh * mmax(mabs( (f * E)  / mmax(mmax( mabs(y),mabs(ynew) ), threshold) )))(1);


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
                        nStep++;
                        break;
                    }
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

                //newMap.ODE_RESULT[t] = y;
                SolverReturnMatrix[nStep, 1] = t;
                SolverReturnMatrix[nStep, new MatrixIndex(2, neq + 1)] = y;
                Console.WriteLine("Integrator State:  {0} done\r", 100 * t / (tFinal - t0));

                //f.setColumn(1,f(colon(1,neq),7));       // Already evaluated feval(odeFcn,tnew,ynew,integratorArgs)
                f[":", 1] = f[":", 7];
            }

            Console.WriteLine();
            return SolverReturnMatrix;
        }

        private static Matrix<double> ntrp45(double tinterp, double t, Matrix<double> y, double h, Matrix<double> f)
        {
	        Matrix<double> BI = new Matrix<double>(new double[,] {{1.0,	-183.0/64.0,     37.0/12.0,    -145.0/128.0},
					                              {0.0,  0.0,            0.0,           0.0},
                                                  {0.0,  1500.0/371.0,  -1000.0/159.0,  1000.0/371.0},
                                                  {0.0, -125.0/32.0,     125.0/12.0,   -375.0/64.0}, 
                                                  {0.0,  9477.0/3392.0, -729.0/106.0,   25515.0/6784.0},
                                                  {0.0, -11.0/7.0,       11.0/3.0,     -55.0/28.0},
                                                  {0.0,  3.0/2.0,       -4.0,           5.0/2.0}});


            double s = (tinterp - t)/h;  
	        Matrix<double> S = new Matrix<double>(4,1,s);

	        return y + f*(h*BI)*Matrix<double>.Cumprod(S);
        }
    }
}
