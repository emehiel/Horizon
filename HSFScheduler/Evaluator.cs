// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using HSFSystem;
using System.Collections.Generic;
using Utilities;

namespace HSFScheduler
{
    public abstract class Evaluator
    {
        public List<StateVariableKey<int>> Ikeys { get; set; } = new List<StateVariableKey<int>>();
        /// <summary>
        /// Abstract class that analyzes the given schedule and assigns a value to it.
        /// </summary>
        /// <param name="schedule"></param>
        /// <returns></returns>
        public abstract double Evaluate(SystemSchedule schedule);

        //public Evaluator(List<StateVariableKey<int>> ikeys)
        //{
        //    Ikeys = ikeys;
        //}
    }
}
