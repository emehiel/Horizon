// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging
{
    public class HSFLog
    {
        public List<HSFLogData> Data { get; private set; }      // Log data list 

        public HSFLog()
        {
            Data = new List<HSFLogData>();
        }

        public void Add(HSFLogData logData)
        {
            Data.Add(logData);
        }
    }
}
