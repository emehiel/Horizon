using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using HSFUniverse;
using MissionElements;
using Utilities;
using HSFSystem;

namespace HSFSubsystem
{
    public class AccessSub : Subsystem
    {
        public AccessSub(XmlNode subNode, Asset asset)
        {
            Asset = asset;
            DefaultSubName = "AccessToTarget";
            getSubNameFromXmlNode(subNode);
            DependentSubsystems = new List<Subsystem>();
        }
        public override bool canPerform(Event proposedEvent, Universe environment) 
        {
            if (!base.canPerform(proposedEvent, environment))
                return false;
            DynamicState position = Asset.AssetDynamicState;
            Matrix<double> assetPosECI = position.PositionECI(proposedEvent.GetTaskStart(Asset));
            Matrix<double> targetPosECI = _task.Target.DynamicState.PositionECI(proposedEvent.GetTaskStart(Asset));
            return GeometryUtilities.hasLOS(assetPosECI, targetPosECI);
        }

        public override bool canExtend(Event proposedEvent, Universe environment, double evalToTime)
        {
            if (proposedEvent.GetEventEnd(Asset) < evalToTime)
                proposedEvent.SetEventEnd(Asset, evalToTime);
            return true;
        }
    }
}
