// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using HSFSystem;
using MissionElements;

namespace HSFScheduler
{
    public class DefaultEvaluator : Evaluator
    {
        #region Attributes
        #endregion

        #region Constructors
        public DefaultEvaluator()
        {
            
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
            //double sum = 0;
            double count = 0;
            foreach(Event eit in schedule.AllStates.Events)
            {
                count += 1; 
                //foreach (KeyValuePair<Asset, Task> assetTask in eit.Tasks)
                //{
                //    Task task = assetTask.Value;
                //    sum += task.Target.Value;
                //}
            }
            return count; // Returns number of events in schedule
            //return sum; // Returns sum of task target values
        }
        #endregion
    }
}
