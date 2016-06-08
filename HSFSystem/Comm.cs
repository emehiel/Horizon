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
        protected StateVarKey<double> DATARATE_KEY;
        #endregion

        #region Constructors
        public Comm(XmlNode CommXmlNode, Dependency dependencies, Asset asset)
        {
            DefaultSubName = "Comm";
            Asset = asset;
            GetSubNameFromXmlNode(CommXmlNode);
            SubsystemDependencyFunctions = new Dictionary<string, Delegate>();
            DependentSubsystems = new List<Subsystem>();
            DATARATE_KEY = new StateVarKey<double>(Asset.Name + "." + "datarate(mb/s)");
            addKey(DATARATE_KEY);
            dependencies.Add("PowerfromComm" + "." + Asset.Name, new Func<Event, HSFProfile<double>>(POWERSUB_PowerProfile_COMMSUB));
        }

        public Comm(XmlNode CommXmlNode, Asset asset)
        {
            DefaultSubName = "Comm";
            Asset = asset;
            GetSubNameFromXmlNode(CommXmlNode);
            DATARATE_KEY = new StateVarKey<double>(Asset.Name + "." + "datarate(mb/s)");
            addKey(DATARATE_KEY);
        }
        #endregion

        #region Methods
        public override bool CanPerform(Event proposedEvent, Universe environment)
        {
            IsEvaluated = true;
            if (!base.CanPerform(proposedEvent, environment))
                return false;
            if (_task.Type == TaskType.COMM)
            {
                HSFProfile<double> newProf = DependencyCollector(proposedEvent);
                if (!newProf.Empty())
                    proposedEvent.State.setProfile(DATARATE_KEY, newProf);
            }
            return true;
        }

        public HSFProfile<double> POWERSUB_PowerProfile_COMMSUB(Event currentEvent)
        {
            return currentEvent.State.GetProfile(DATARATE_KEY) * 20;
        }
        #endregion
    }
}
