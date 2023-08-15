// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using HSFUniverse;
using MissionElements;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using Utilities;

namespace HSFSystem
{
    //[ExcludeFromCodeCoverage]
    public class Comm : Subsystem
    {
        #region Attributes
        protected StateVariableKey<double> DATARATE_KEY;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor for built in subsystem
        /// </summary>
        /// <param name="CommXmlNode"></param>
        /// <param name="asset"></param>
        public Comm(XmlNode CommXmlNode)
        {
            //DefaultSubName = "Comm";
        }

        /// <summary>
        /// Constructor for scripted subsystem
        /// </summary>
        /// <param name="CommXmlNode"></param>
        /// <param name="asset"></param>
        /*
        public Comm(XmlNode CommXmlNode, Asset asset) : base(CommXmlNode, asset)
        {
            
        }
        */
        #endregion

        #region Methods
        /// <summary>
        /// An override of the Subsystem CanPerform method
        /// </summary>
        /// <param name="proposedEvent"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        public override bool CanPerform(Event proposedEvent, Domain environment)
        {
            var DATARATE_KEY = Dkeys[0];

            if (_task.Type == "comm")
            {
                HSFProfile<double> newProf = DependencyCollector(proposedEvent);
                if (!newProf.Empty())
                    proposedEvent.State.AddValues(DATARATE_KEY, newProf);
                    //proposedEvent.State.SetProfile(DATARATE_KEY, newProf);
            }
            return true;
        }

        /// <summary>
        /// Dependency function for power subsystem
        /// </summary>
        /// <param name="currentEvent"></param>
        /// <returns></returns>
        public HSFProfile<double> Power_asset1_from_Comm_asset1(Event currentEvent)
        {
            var DATARATE_KEY = Dkeys[0];
            return currentEvent.State.GetProfile(DATARATE_KEY) * 20;
        }
        #endregion
    }
}
