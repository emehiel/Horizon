﻿// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using HSFSystem;
using MissionElements;

namespace HSFScheduler
{
    public class TargetValueEvaluator : Evaluator
    {
        #region Attributes
        public Dependency Dependencies;
        private Delegate _evalFunction;
        #endregion

        #region Constructors
        public TargetValueEvaluator(Dependency dependencies)
        {
            Dependencies = dependencies;
        }
        public TargetValueEvaluator(Delegate evalFunction)
        {
            _evalFunction = evalFunction;

        }
        #endregion

        #region Methods
        /// <summary>
        /// Override of the Evaluate method
        /// </summary>
        /// <param name="schedule"></param>
        /// <returns></returns>
        public override double Evaluate(SystemSchedule schedule)
        {
            double sum = 0;
            foreach(Event eit in schedule.AllStates.Events)
            {
                foreach(KeyValuePair<Asset, Task> assetTask in eit.Tasks)
                {
                    Task task = assetTask.Value;
                    sum += task.Target.Value;
                    if (task.Type == "comm")
                    {
                        sum += 0;
                        //sum += (double)_evalFunction.DynamicInvoke(eit);
                        //sum = sum + (double)Dependencies.GetDependencyFunc("EvalfromSSDR" + "." + assetTask.Key.Name).DynamicInvoke(eit);
                    }
                }
            }
            return sum;
        }
        #endregion
    }
}
