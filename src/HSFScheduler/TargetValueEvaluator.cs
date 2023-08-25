// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using HSFSystem;
using MissionElements;
using Utilities;

namespace HSFScheduler
{
    public class TargetValueEvaluator : Evaluator
    {
        #region Attributes
        public List<dynamic> _keychain;
        #endregion

        #region Constructors
        public TargetValueEvaluator()
        {
            
        }
        public TargetValueEvaluator(List<dynamic> keychain)
        {
            _keychain = keychain;
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
                foreach (KeyValuePair<Asset, Task> assetTask in eit.Tasks)
                {
                    Task task = assetTask.Value;
                    Asset asset = assetTask.Key;
                    sum += task.Target.Value;
                    if (task.Type == "comm")
                    {
                        StateVariableKey<double> DATABUFFERRATIOKEY = _keychain.Find(s => s.VariableName == asset.Name + ".databufferfillratio");
                        double StartTime = eit.GetTaskStart(asset);
                        double EndTime = eit.GetTaskEnd(asset);
                        var dataBufferRatioStart = eit.State.GetValueAtTime(DATABUFFERRATIOKEY, StartTime).Value;
                        double dataBufferRatioEnd = eit.State.GetValueAtTime(DATABUFFERRATIOKEY, EndTime).Value;
                        sum += (dataBufferRatioStart - dataBufferRatioEnd) * 50;
                    }
                }
            }
            return sum;
        }
        #endregion
    }
}
