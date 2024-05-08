// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using HSFUniverse;
using MissionElements;
using Utilities;
using log4net;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;

namespace HSFSystem
{
    //[ExcludeFromCodeCoverage]
    public class SSDR : Subsystem
    {
        // Default Values
        protected double _bufferSize = 4098;
        protected StateVariableKey<double> DATABUFFERRATIO_KEY;
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SSDR(JObject ssdrJson)
        {
            StringComparison stringCompare = StringComparison.CurrentCultureIgnoreCase;
            JToken paramJson;
            if (ssdrJson.TryGetValue("bufferSize", stringCompare, out paramJson))
                this._bufferSize = paramJson.Value<double>();
        }
        /// <summary>
        /// Constructor for built in subsystem
        /// Default: BufferSize = 4098
        /// </summary>
        /// <param name="SSDRXmlNode"></param>
        /// <param name="asset"></param>
        public SSDR(XmlNode SSDRXmlNode)
        {
            //DefaultSubName = "SSDR";

            if (SSDRXmlNode.Attributes["bufferSize"] != null)
                _bufferSize = (double)Convert.ChangeType(SSDRXmlNode.Attributes["bufferSize"].Value.ToString(), typeof(double));
        }

        /// <summary>
        /// An override of the Subsystem CanPerform method
        /// </summary>
        /// <param name="proposedEvent"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        public override bool CanPerform(Event proposedEvent, Domain environment)
        {
            var DATABUFFERRATIO_KEY = Dkeys[0];


            //var DATABUFFERRATIO_KEY2 = this.Dkeys.Find(s => s.VariableName == "asset1.databufferfillratio");
            if (_task.Type == "imaging")
            {
                double ts = proposedEvent.GetTaskStart(Asset);
                double te = proposedEvent.GetTaskEnd(Asset);

                double oldbufferratio = _newState.GetLastValue(DATABUFFERRATIO_KEY).Value;

                //Delegate DepCollector;
                //SubsystemDependencyFunctions.TryGetValue("DepCollector", out DepCollector);
                //HSFProfile<double> newdataratein = ((HSFProfile<double>)DepCollector.DynamicInvoke(proposedEvent) / _bufferSize);

                Delegate DepNeeded;
                SubsystemDependencyFunctions.TryGetValue("SSDR_asset1_from_EOSensor_asset1", out DepNeeded);
                HSFProfile<double> newdataratein = ((HSFProfile<double>)DepNeeded.DynamicInvoke(proposedEvent) / _bufferSize);

                bool exceeded = false;
                HSFProfile<double> newdataratio = newdataratein.upperLimitIntegrateToProf(ts, te, 1, 1, ref exceeded, 0, oldbufferratio);
                if (!exceeded)
                {
                    _newState.AddValues(DATABUFFERRATIO_KEY, newdataratio);
                    return true;
                }

                // TODO: Change to logger
                Console.WriteLine("SSDR buffer full");
                return false;
            }
            else if (_task.Type == "comm")
            {
                double ts = proposedEvent.GetTaskStart(Asset);
                proposedEvent.SetTaskEnd(Asset, ts + 60.0);
                double te = proposedEvent.GetTaskEnd(Asset);

                double data = _bufferSize * _newState.GetLastValue(Dkeys.First()).Value;
                double dataqueout = data / 2 > 50 ? data / 2 : data;

                if (data - dataqueout < 0)
                    dataqueout = data;

                if (dataqueout > 0)
                    _newState.AddValue(DATABUFFERRATIO_KEY, te, (data - dataqueout) / _bufferSize);
                return true;
            }
            return true;
        }

        /// <summary>
        /// Dependecy Function for the power subsystem
        /// </summary>
        /// <param name="currentState"></param>
        /// <returns></returns>
        public HSFProfile<double> Power_asset1_from_SSDR_asset1(Event currentEvent)
        {
            HSFProfile<double> prof1 = new HSFProfile<double>();
            prof1[currentEvent.GetEventStart(Asset)] = 15;
            return prof1;
        }

        /// <summary>
        /// Dependecy function for the ssdr subsystem
        /// </summary>
        /// <param name="currentEvent"></param>
        /// <returns></returns>
        public HSFProfile<double> Comm_asset1_from_SSDR_asset1(Event currentEvent)
        {
            var DATABUFFERRATIO_KEY = Dkeys[0];
            //var Asset = Asset;
            double datarate = 5000 * (currentEvent.State.GetValueAtTime(DATABUFFERRATIO_KEY, currentEvent.GetTaskStart(Asset)).Value - currentEvent.State.GetValueAtTime(DATABUFFERRATIO_KEY, currentEvent.GetTaskEnd(Asset)).Value) / (currentEvent.GetTaskEnd(Asset) - currentEvent.GetTaskStart(Asset));
            HSFProfile<double> prof1 = new HSFProfile<double>();
            if (datarate != 0)
            {
                prof1[currentEvent.GetTaskStart(Asset)] = datarate;
                prof1[currentEvent.GetTaskEnd(Asset)] = 0;
            }
            return prof1;
        }
    }
}
