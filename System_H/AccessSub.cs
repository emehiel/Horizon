using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using HSFUniverse;
using MissionElements;
using Utilities;

namespace HSFSubsystem
{
    public class AccessSub : Subsystem
    {
        public AccessSub(XmlNode subNode)
        {
            DefaultSubName = "AccessToTarget";
            getSubNameFromXmlNode(subNode);
        }
        public virtual bool canPerform(SystemState oldState, SystemState newState,
                            Task task, DynamicState position, Universe environment) 
        {
            Matrix<double> assetPosECI = position.PositionECI(newState.TaskStart);
            Matrix<double> targetPosECI = task.Target.DynamicState.PositionECI(newState.TaskStart);
            return GeometryUtilities.hasLOS(assetPosECI, targetPosECI);
        }

        public override bool canExtend(SystemState newState, DynamicState position, Universe environment, double evalToTime)
        {
            if (newState.EventEnd < evalToTime)
                newState.EventEnd = evalToTime;
            return true;
        }
    }
}
