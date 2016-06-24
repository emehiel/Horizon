// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging
{
    static public class HSFLogger   // Logs log data 
    {
        public static HSFLog Log { get; private set; }

        public static void Output(HSFLogData logData)
        {
            if (Log == null)
                Log = new HSFLog();
            Log.Add(logData);
        }

    }
}

