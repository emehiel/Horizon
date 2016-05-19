using HSFSystem;
using HSFUniverse;
using MissionElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Utilities;

namespace HSFSubsystem
{
    public class Comm : Subsystem
    {
        #region Attributes
        private StateVarKey<double> DATARATE_KEY;
        #endregion

        #region Constructors
        public Comm(XmlNode CommXmlNode, Dependencies dependencies, Asset asset)
        {
            DefaultSubName = "Comm";
            Asset = asset;
            getSubNameFromXmlNode(CommXmlNode);
            SubsystemDependencyFunctions = new Dictionary<string, Delegate>();
            DependentSubsystems = new List<Subsystem>();
            DATARATE_KEY = new StateVarKey<double>(Asset.Name + "." + "datarate(mb/s)");
            addKey(DATARATE_KEY);
            dependencies.Add("PowerfromComm", new Func<Event, HSFProfile<double>>(POWERSUB_PowerProfile_COMMSUB));
        }
        #endregion

        #region Methods
        public override bool canPerform(Event proposedEvent, Universe environment)
        {
            IsEvaluated = true;
            if (!base.canPerform(proposedEvent, environment))
                return false;
            if (_task.Type == TaskType.COMM)
            {
                HSFProfile<double> newProf = DependencyCollector(proposedEvent);
                if (!newProf.Empty())
                    proposedEvent.State.setProfile(DATARATE_KEY, newProf);
            }
            return true;
        }

        HSFProfile<double> POWERSUB_PowerProfile_COMMSUB(Event currentEvent)
        {
            return (HSFProfile<double>)currentEvent.State.GetProfile(DATARATE_KEY) * 20;
        }
        #endregion
    }
}
