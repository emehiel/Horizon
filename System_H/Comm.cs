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
        public bool canPerform(SystemState oldState, SystemState newState,
                            Task task, DynamicState position,
                            Universe environment, List<SystemState> allStates)
        {
            if (task.Type == TaskType.COMM)
            {
                HSFProfile<double> newProf = DependencyCollector(allStates);
                if (!newProf.Empty())
                    newState.setProfile(DATARATE_KEY, newProf);
            }
            return true;
        }

        public HSFProfile<double> DependencyCollector(List<SystemState> allStates)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
