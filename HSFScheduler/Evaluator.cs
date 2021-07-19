// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using HSFSystem;

namespace HSFScheduler
{
    public abstract class Evaluator
    {
        /// <summary>
        /// Abstract class that analyzes the given schedule and assigns a value to it.
        /// </summary>
        /// <param name="schedule"></param>
        /// <returns></returns>
        public abstract double Evaluate(SystemSchedule schedule);
    }
}
