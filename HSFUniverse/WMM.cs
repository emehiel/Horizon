using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace HSFUniverse
{
    public class WMM
    {
        private double a, a2, a4, b, b2, b4, c2, c3, c4, parp;
        private double epoch;
        private List<double> sp, cp, fm, fn, pp;
        private double maxord, maxdeg;
        private double re = 6378;
        private Matrix<double> p, dp, k, tc, c, cd;

        public double dec, dip, ti, bh, bx, by, bz, gv;

        public WMM()
        {
            string wmm_filename = AppDomain.CurrentDomain.BaseDirectory + "\\..\\..\\..\\HSFUniverse\\WMM.COF";
            string line;
            List<Dictionary<string, double>> wmm = new List<Dictionary<string, double>>();
            System.IO.StreamReader file =
                    new System.IO.StreamReader(wmm_filename);
            while ((line = file.ReadLine()) != null)
            {
                var linevals = line.Split((string[])null, StringSplitOptions.RemoveEmptyEntries);
                if (linevals.Count() == 3)
                {
                    epoch = Convert.ToDouble(linevals[0]);
                }
                else if (linevals.Count() == 6)
                {
                    Dictionary<string, double> linedict = new Dictionary<string, double>();
                    linedict.Add("n", Convert.ToDouble(linevals[0]));
                    linedict.Add("m", Convert.ToDouble(linevals[1]));
                    linedict.Add("gnm", Convert.ToDouble(linevals[2]));
                    linedict.Add("hnm", Convert.ToDouble(linevals[3]));
                    linedict.Add("dgnm", Convert.ToDouble(linevals[4]));
                    linedict.Add("dhnm", Convert.ToDouble(linevals[5]));
                    wmm.Add(linedict);
                }

            }
            maxord = 12;
            maxdeg = 12;
            tc = new Matrix<double>(14);
            sp = new List<double>(new double[15]);
            cp = new List<double>(new double[15]);
            cp[0] = 1.0;
            pp = new List<double>(new double[14]);
            pp[0] = 1.0;
            p = new Matrix<double>(15);
            p[1, 1] = 1.0;
            dp = new Matrix<double>(14);
            a = 6378.137;
            b = 6356.7523142;

            re = 6371.2;
            a2 = a * a;
            b2 = b * b;
            c2 = a2 - b2;
            a4 = a2 * a2;
            b4 = b2 * b2;
            c4 = a4 - b4;

            c = new Matrix<double>(15);
            cd = new Matrix<double>(15);


            foreach (Dictionary<string, double> wmmnm in wmm)
            {
                int m = (int)wmmnm["m"] + 1;
                int n = (int)wmmnm["n"] + 1;
                double gnm = wmmnm["gnm"];
                double hnm = wmmnm["hnm"];
                double dgnm = wmmnm["dgnm"];
                double dhnm = wmmnm["dhnm"];
                if (m <= n)
                {
                    c[m, n] = gnm;
                    cd[m, n] = dgnm;
                    if (m != 1)
                    {
                        c[n, m - 1] = hnm;
                        cd[n, m - 1] = dhnm;
                    }
                }
            }
            /* CONVERT SCHMIDT NORMALIZED GAUSS COEFFICIENTS TO UNNORMALIZED */
            Matrix<double> snorm = new Matrix<double>(14);
            snorm[1, 1] = 1.0;
            k = new Matrix<double>(14);
            fn = new List<double>(new double[] { 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0, 12.0, 13.0 });
            fm = new List<double>(new double[] { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0, 12.0});
            for (int n = 2; n < maxord + 2; n++)
            {
                snorm[1, n] = snorm[1, n - 1] * (2.0 * (n-1) - 1) / (n-1);
                double j = 2.0;
                int m = 0;
                int D1 = 1;
                int D2 = (n-1 - m + D1) / D1;
                while (D2 > 0)
                {
                    k[m + 1, n] = (((n - 2) * (n - 2)) - (m * m)) / ((2.0 * (n-1) - 1) * (2.0 * (n-1) - 3.0));
                    if (m > 0)
                    {
                        double flnmj = (((n-1) - m + 1.0) * j) / ((n-1) + m);
                        snorm[m + 1, n] = snorm[m, n] * Math.Sqrt(flnmj);
                        j = 1.0;
                        c[n, m] = snorm[m + 1, n] * c[n, m];
                        cd[n, m] = snorm[m + 1, n] * cd[n, m];
                    }
                    c[m + 1, n] = snorm[m + 1, n] * c[m + 1, n];
                    cd[m + 1, n] = snorm[m + 1, n] * cd[m + 1, n];
                    D2 = D2 - 1;
                    m = m + D1;
                }
            }
        }

        public void GeoMag(double dlat, double dlon, double alt, DateTime date)
        {
            double time = date.Year + date.DayOfYear / 365.0;
            double dt = time - epoch;
            double glat = dlat;
            double glon = dlon;
            double rlat = Math.PI / 180 * (glat);
            double rlon = Math.PI/180*(glon);
            double srlon = Math.Sin(rlon);
            double srlat = Math.Sin(rlat);
            double crlon = Math.Cos(rlon);
            double crlat = Math.Cos(rlat);
            double srlat2 = srlat * srlat;
            double crlat2 = crlat * crlat;
            sp[1] = srlon;
            cp[1] = crlon;

            double otime = -1000;
            double oalt = -1000;
            double olat = -1000;
            double olon = -1000;

            /* CONVERT FROM GEODETIC COORDS. TO SPHERICAL COORDS. */
            if (alt == oalt || glat == olat)
            {
                throw new ArgumentOutOfRangeException("Altitude or latitude too low");
            }
            double q = Math.Sqrt(a2 - c2 * srlat2);
            double q1 = alt * q;
            double q2 = ((q1 + a2) / (q1 + b2)) * ((q1 + a2) / (q1 + b2));
            double ct = srlat / Math.Sqrt(q2 * crlat2 + srlat2);
            double st = Math.Sqrt(1.0 - (ct * ct));
            double r2 = (alt * alt) + 2.0 * q1 + (a4 - c4 * srlat2) / (q * q);
            double r = Math.Sqrt(r2);
            double d = Math.Sqrt(a2 * crlat2 + b2 * srlat2);
            double ca = (alt + d) / r;
            double sa = c2 * crlat * srlat / (r * d);
            if (glon != olon)
            {
                for (int m = 2; m < maxord + 1; m++)
                {
                    sp[m] = sp[1] * cp[m - 1] + cp[1] * sp[m - 1];
                    cp[m] = cp[1] * cp[m - 1] - sp[1] * sp[m - 1];
                }
            }

            double aor = re / r;
            double ar = aor * aor;
            double br = 0;
            double bt = 0;
            double bp = 0;
            double bpp = 0;
            for (int n = 2; n < maxord + 2; n++)
            {
                ar = ar * aor;
                int m = 1;
                int D4 = (n + m - 1);
                while (D4 > 0)
                {

                    /*
                        COMPUTE UNNORMALIZED ASSOCIATED LEGENDRE POLYNOMIALS
                        AND DERIVATIVES VIA RECURSION RELATIONS
                    */
                    if (alt != oalt || glat != olat) {
                        if (n == m)
                        {
                            p[m,n] = st * p[m - 1, n - 1];
                            dp[m,n] = st * dp[m - 1, n - 1] + ct * p[m - 1, n - 1];
                        }
                        else if(n == 2 && m == 1)
                        {
                            p[m, n] = ct * p[m, n - 1];
                            dp[m, n] = ct * dp[m, n - 1] - st * p[m, n - 1];
                        }
                        else if(n > 2 && n != m)
                        {
                            if (m > n - 2)
                                p[m, n - 2] = 0;
                            if (m > n - 2)
                                dp[m,n - 2] = 0;
                            p[m,n] = ct * p[m,n - 1] - k[m,n] * p[m,n - 2];
                            dp[m,n] = ct * dp[m,n - 1] - st * p[m,n - 1] - k[m,n] * dp[m,n - 2];
                        }
                    }
                    /* TIME ADJUST THE GAUSS COEFFICIENTS */
                    if (time != otime) {
                        tc[m,n] = c[m,n] + dt * cd[m,n];
                        if (m != 1)
                            tc[n, m - 1] = c[n,m - 1] + dt * cd[n,m - 1];
                    }
                    /* ACCUMULATE TERMS OF THE SPHERICAL HARMONIC EXPANSIONS */
                    double par = ar * p[m,n];

                    double temp1, temp2;
                    if (m == 1)
                    {
                        temp1 = tc[m,n] * cp[m-1];
                        temp2 = tc[m,n] * sp[m-1];
                    }
                    else
                    {
                        temp1 = tc[m,n] * cp[m-1] + tc[n,m - 1] * sp[m-1];
                        temp2 = tc[m,n] * sp[m-1] - tc[n,m - 1] * cp[m-1];
                    }
                    bt = bt - ar * temp1 * dp[m,n];
                    bp = bp + (fm[m-1] * temp2 * par);
                    br = br + (fn[n-1] * temp1 * par);
                    /* SPECIAL CASE:  NORTH/SOUTH GEOGRAPHIC POLES */
                    if (st == 0.0 && m == 2)
                        {
                        if (n == 2)
                        {
                            pp[n-1] = pp[n - 2];
                        }
                        else {
                            pp[n-1] = ct * pp[n - 2] - k[m, n] * pp[n - 3];
                            
                        }
                        parp = ar * pp[n - 1];
                        bpp = bpp + (fm[m - 1] * temp2 * parp);
                    }

                    D4 = D4 - 1;
                    m = m + 1;
                }
            }
            if (st == 0.0)
                bp = bpp;
            else
                bp = bp / st;
            /* ROTATE MAGNETIC VECTOR COMPONENTS FROM SPHERICAL TO GEODETIC COORDINATES */
            bx = -bt * ca - br * sa;
            by = bp;
            bz = bt * sa - br * ca;
            /* COMPUTE DECLINATION (DEC), INCLINATION (DIP) AND TOTAL INTENSITY (TI) */
            bh = Math.Sqrt((bx * bx) + (by * by));
            ti = Math.Sqrt((bh * bh) + (bz * bz));
            dec = 180/Math.PI*(Math.Atan2(by, bx));
            dip = 180/Math.PI*(Math.Atan2(bz, bh));
            /*
            COMPUTE MAGNETIC GRID VARIATION IF THE CURRENT
            GEODETIC POSITION IS IN THE ARCTIC OR ANTARCTIC
            (I.E. GLAT > +55 DEGREES OR GLAT < -55 DEGREES)

            OTHERWISE, SET MAGNETIC GRID VARIATION TO -999.0
            */
            gv = -999.0;
            if (Math.Abs(glat) >= 55) {
                if (glat > 0.0 && glon >= 0.0)
                    gv = dec - glon;
                if (glat > 0.0 && glon < 0.0)
                    gv = dec + Math.Abs(glon);
                if (glat < 0.0 && glon >= 0.0)
                    gv = dec + glon;
                if (glat < 0.0 && glon < 0.0)
                    gv = dec - Math.Abs(glon);
                if (gv > +180.0)
                    gv = gv - 360.0;
                if (gv < -180.0)
                    gv = gv + 360.0;
                }
            otime = time;
            oalt = alt;
            olat = glat;
            olon = glon;

        }
    }
}
