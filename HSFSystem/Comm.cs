// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using HSFSystem;
using HSFUniverse;
using MissionElements;
using System;
using System.Collections.Generic;
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
        /// <summary>
        /// Constructor for built in subsystem
        /// </summary>
        /// <param name="CommXmlNode"></param>
        /// <param name="dependencies"></param>
        /// <param name="asset"></param>
        public Comm(XmlNode CommXmlNode, Dependency dependencies, Asset asset) : base(CommXmlNode, dependencies, asset)
        {
            DefaultSubName = "Comm";
            //Asset = asset;
            GetSubNameFromXmlNode(CommXmlNode);
            SubsystemDependencyFunctions = new Dictionary<string, Delegate>();
            DependentSubsystems = new List<Subsystem>();
            DATARATE_KEY = new StateVarKey<double>(Asset.Name + "." + "datarate(mb/s)");
            addKey(DATARATE_KEY);
            dependencies.Add("PowerfromComm" + "." + Asset.Name, new Func<Event, HSFProfile<double>>(POWERSUB_PowerProfile_COMMSUB));
        }

        /// <summary>
        /// Constructor for scripted subsystem
        /// </summary>
        /// <param name="CommXmlNode"></param>
        /// <param name="asset"></param>
        public Comm(XmlNode CommXmlNode, Asset asset) : base(CommXmlNode, asset)
        {
            DefaultSubName = "Comm";
            //Asset = asset;
            GetSubNameFromXmlNode(CommXmlNode);
            DATARATE_KEY = new StateVarKey<double>(Asset.Name + "." + "datarate(mb/s)");
            addKey(DATARATE_KEY);
        }
        #endregion

        #region Methods
        /// <summary>
        /// An override of the Subsystem CanPerform method
        /// </summary>
        /// <param name="proposedEvent"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        public override bool CanPerform(Event proposedEvent, Universe environment)
        {
            IsEvaluated = true;
            if (!base.CanPerform(proposedEvent, environment))
                return false;
            if (_task.Type == TaskType.COMM)
            {
                HSFProfile<double> newProf = DependencyCollector(proposedEvent);
                if (!newProf.Empty())
                    proposedEvent.State.SetProfile(DATARATE_KEY, newProf);
            }
            return true;
        }

        /// <summary>
        /// Dependency function for power subsystem
        /// </summary>
        /// <param name="currentEvent"></param>
        /// <returns></returns>
        public HSFProfile<double> POWERSUB_PowerProfile_COMMSUB(Event currentEvent)
        {
            return currentEvent.State.GetProfile(DATARATE_KEY) * 20;
        }
        #endregion
    }
}
