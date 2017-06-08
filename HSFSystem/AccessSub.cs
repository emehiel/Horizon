// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)
using System;
using System.Collections.Generic;
using System.Xml;
using HSFUniverse;
using MissionElements;
using Utilities;

namespace HSFSubsystem
{
    public class AccessSub : Subsystem
    {
        /// <summary>
        /// Constructor for the built in subsystem (cannot be scripted)
        /// </summary>
        /// <param name="subNode"></param>
        /// <param name="asset"></param>
        public AccessSub(XmlNode subNode, Asset asset)
        {
            Asset = asset;
            DefaultSubName = "AccessToTarget";
            GetSubNameFromXmlNode(subNode);
            DependentSubsystems = new List<Subsystem>();
            SubsystemDependencyFunctions = new Dictionary<string, Delegate>();
        }

        public override bool CanPerform( Event proposedEvent, Universe environment) 
        {
            if (!base.CanPerform( proposedEvent, environment))
                return false;
            DynamicState position = Asset.AssetDynamicState;
            Vector assetPosECI = position.PositionECI(proposedEvent.GetTaskStart(Asset));
            Vector targetPosECI = _task.Target.DynamicState.PositionECI(proposedEvent.GetTaskStart(Asset));
            return GeometryUtilities.hasLOS(assetPosECI, targetPosECI);
        }

        public override bool CanExtend(Event proposedEvent, Universe environment, double evalToTime)
        {
            if (proposedEvent.GetEventEnd(Asset) < evalToTime)
                proposedEvent.SetEventEnd(Asset, evalToTime);
            return true;
        }
    }
}
