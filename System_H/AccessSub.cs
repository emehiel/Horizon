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
        public AccessSub(XmlNode subNode)
        {
            DefaultSubName = "AccessToTarget";
            getSubNameFromXmlNode(subNode);
        }
        public override bool canPerform(SystemState oldState, SystemState newState,
                            Dictionary<Asset, Task> tasks, Universe environment) 
        {
            if (!base.canPerform(oldState, newState, tasks, environment))
                return false;
            DynamicState position = Asset.AssetDynamicState;
            Matrix<double> assetPosECI = position.PositionECI(newState.TaskStart);
            Matrix<double> targetPosECI = _task.Target.DynamicState.PositionECI(newState.TaskStart);
            return GeometryUtilities.hasLOS(assetPosECI, targetPosECI);
        }

        public override bool canExtend(SystemState newState, Universe environment, double evalToTime)
        {
            if (newState.EventEnd < evalToTime)
                newState.EventEnd = evalToTime;
            return true;
        }
    }
}
