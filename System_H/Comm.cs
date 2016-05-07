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
        public static StateVarKey<double> DATARATE_KEY = new StateVarKey<double>("DataRate(MB/s)");
        #endregion

        #region Constructors
        public Comm(XmlNode CommXmlNode, Dependencies dependencies, Asset asset)
        {
            DefaultSubName = "Comm";
            getSubNameFromXmlNode(CommXmlNode);
            Asset = asset;
            addKey(DATARATE_KEY);
        }
        #endregion

        #region Methods
        public override bool canPerform(SystemState oldState, SystemState newState,
                            Dictionary<Asset, Task> tasks, Universe environment)
        {
            if(!base.canPerform(oldState, newState, tasks, environment))
                return false;
            if (_task.Type == TaskType.COMM)
            {
                HSFProfile<double> newProf = DependencyCollector(newState);
                if (!newProf.Empty())
                    newState.setProfile(DATARATE_KEY, newProf);
            }
            return true;
        }

        public HSFProfile<double> DependencyCollector(SystemState currentState)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
