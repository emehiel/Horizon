// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Xml;
using HSFUniverse;
using MissionElements;
using Utilities;

namespace HSFSystem
{
    public class AccessSub : Subsystem
    {
        /// <summary>
        /// Constructor for the built in subsystem (cannot be scripted)
        /// </summary>
        /// <param name="subNode"></param>
        /// <param name="asset"></param>
        public AccessSub(XmlNode subNode)
        {
            //DefaultSubName = "AccessToTarget";
        }

        public override bool CanPerform( Event proposedEvent, Domain environment) 
        {
            DynamicState position = Asset.AssetDynamicState;
            Vector assetPosECI = position.PositionECI(proposedEvent.GetTaskStart(Asset));
            Vector targetPosECI = _task.Target.DynamicState.PositionECI(proposedEvent.GetTaskStart(Asset));
            return GeometryUtilities.hasLOS(assetPosECI, targetPosECI);
        }

        public override bool CanExtend(Event proposedEvent, Domain environment, double evalToTime)
        {
            if (proposedEvent.GetEventEnd(Asset) < evalToTime)
                proposedEvent.SetEventEnd(Asset, evalToTime);
            return true;
        }
    }
}
