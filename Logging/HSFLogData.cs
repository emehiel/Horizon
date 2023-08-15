// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MissionElements;
using HSFSystem;

namespace Logging
{
    public class HSFLogData
    {

        public string AssetName { get; private set; }     // Asset Name 
        public string SubName { get; private set; }       // Subsystem Name 
        public string TaskName { get; private set; }      // Task Name 
        public string TargetName { get; private set; }       // Target Name 
        public string ConstraintName { get; private set; }       // Constraint Name 
        public string Violation { get; private set; }       // State variable requirement number that violated a constraint 
        public double Value { get; private set; }         // State variable number 
        public double TimeInfo { get; private set; }      // Time information 


        public HSFLogData(Subsystem subsystem, Task task, string violation, double value, double timeInfo)
        {
            AssetName = subsystem.Asset.Name;
            SubName = subsystem.Name;
            TaskName = task.Type.ToString();
            TargetName = task.Target.Name;
            ConstraintName = null;
            Violation = violation;
            Value = value;
            TimeInfo = timeInfo;
        }
        public HSFLogData(Constraint constraint, Subsystem subsystem, Task task, double value, double timeInfo)
        {
            AssetName = constraint.Name;
            SubName = subsystem.Name;
            TaskName = task.Type.ToString();
            TargetName = task.Target.Name;
            ConstraintName = constraint.Name;
            Violation = "Constraint Failed";
            Value = value;
            TimeInfo = timeInfo;
        }
    }
}
