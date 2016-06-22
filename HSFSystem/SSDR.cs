// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

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
    public class SSDR : Subsystem
    {
        //Some Defaults
        protected double _bufferSize = 4098;
        protected StateVarKey<double> DATABUFFERRATIO_KEY; 

        public SSDR(XmlNode SSDRXmlNode, Dependency dependencies, Asset asset)
        {
            DefaultSubName = "SSDR";
            Asset = asset;
            GetSubNameFromXmlNode(SSDRXmlNode);
            if (SSDRXmlNode.Attributes["bufferSize"] != null)
                _bufferSize = (double)Convert.ChangeType(SSDRXmlNode.Attributes["bufferSize"].Value.ToString(), typeof(double));
            DATABUFFERRATIO_KEY = new StateVarKey<double>(Asset.Name + "." +"databufferfillratio");
            addKey(DATABUFFERRATIO_KEY);
            SubsystemDependencyFunctions = new Dictionary<string, Delegate>();
            DependentSubsystems = new List<Subsystem>();
            dependencies.Add("PowerfromSSDR" + "." + Asset.Name, new Func<Event, HSFProfile<double>>(POWERSUB_PowerProfile_SSDRSUB));
            dependencies.Add("CommfromSSDR" + "." + Asset.Name, new Func<Event, HSFProfile<double>>(COMMSUB_DataRateProfile_SSDRSUB));
            dependencies.Add("EvalfromSSDR" + "." + Asset.Name, new Func<Event, double>(EVAL_DataRateProfile_SSDRSUB));
        }
        public SSDR(XmlNode SSDRXmlNode, Asset asset)
        {
            DefaultSubName = "SSDR";
            Asset = asset;
            GetSubNameFromXmlNode(SSDRXmlNode);
            if (SSDRXmlNode.Attributes["bufferSize"] != null)
                _bufferSize = (double)Convert.ChangeType(SSDRXmlNode.Attributes["bufferSize"].Value.ToString(), typeof(double));
            DATABUFFERRATIO_KEY = new StateVarKey<double>(Asset.Name + "." + "databufferfillratio");
            addKey(DATABUFFERRATIO_KEY);
        }
        /// <summary>
        /// An override of the subsystem canPerform method
        /// </summary>
        /// <param name="oldState"></param>
        /// <param name="newState"></param>
        /// <param name="tasks"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        public override bool CanPerform(Event proposedEvent, Universe environment)
        {
            if (!base.CanPerform(proposedEvent, environment))
                return false;
            if (_task.Type == TaskType.IMAGING)
            {
                double ts = proposedEvent.GetTaskStart(Asset);
                double te = proposedEvent.GetTaskEnd(Asset);

                double oldbufferratio = _newState.getLastValue(Dkeys[0]).Value;

                Delegate DepCollector;
                SubsystemDependencyFunctions.TryGetValue("DepCollector", out DepCollector);
                HSFProfile<double> newdataratein = ((HSFProfile<double>)DepCollector.DynamicInvoke(proposedEvent) / _bufferSize);

                bool exceeded = false;
                HSFProfile<double> newdataratio = newdataratein.upperLimitIntegrateToProf(ts, te, 1, 1, ref exceeded, 0, oldbufferratio);
                if (oldbufferratio > newdataratio.LastValue())
                    Console.WriteLine("old>new");
                if (!exceeded)
                {
                //    if(newdataratio.Data.Keys.Min() >ts && newdataratio.Data.Keys.Max() <te)
                        _newState.addValue(DATABUFFERRATIO_KEY, newdataratio);
                    return true;
                }
                Logger.Report("SSDR");
                return false;
            }
            else if (_task.Type == TaskType.COMM)
            {
                double ts = proposedEvent.GetTaskStart(Asset);
                proposedEvent.SetTaskEnd(Asset, ts + 60.0);
                double te = proposedEvent.GetTaskEnd(Asset);

                double data = _bufferSize * _newState.getLastValue(Dkeys.First()).Value;
                double dataqueout = data / 2 > 50 ? data / 2 : data;

                if (data - dataqueout < 0)
                    dataqueout = data;

                if (dataqueout > 0)
                    _newState.addValue(DATABUFFERRATIO_KEY, new KeyValuePair<double, double>(te, (data - dataqueout) / _bufferSize));
                return true;
            }
            return true;
        }

        /// <summary>
        /// Dependecy Function for the power subsystem
        /// </summary>
        /// <param name="currentState"></param>
        /// <returns></returns>
        public HSFProfile<double> POWERSUB_PowerProfile_SSDRSUB(Event currentEvent)
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
        public HSFProfile<double> COMMSUB_DataRateProfile_SSDRSUB(Event currentEvent)
        {
            double datarate = 5000 * (currentEvent.State.getValueAtTime(DATABUFFERRATIO_KEY, currentEvent.GetTaskStart(Asset)).Value - currentEvent.State.getValueAtTime(DATABUFFERRATIO_KEY, currentEvent.GetTaskEnd(Asset)).Value) / (currentEvent.GetTaskEnd(Asset) - currentEvent.GetTaskStart(Asset));
            HSFProfile<double> prof1 = new HSFProfile<double>();
            if (datarate != 0)
            {
                prof1[currentEvent.GetTaskStart(Asset)] = datarate;
                prof1[currentEvent.GetTaskEnd(Asset)] = 0;
            }
            return prof1;
        }

        public double EVAL_DataRateProfile_SSDRSUB(Event currentEvent)
        {
            return (currentEvent.State.getValueAtTime(DATABUFFERRATIO_KEY, currentEvent.GetTaskEnd(Asset)).Value
            - currentEvent.State.getValueAtTime(DATABUFFERRATIO_KEY, currentEvent.GetTaskEnd(Asset)).Value) *50;

            //return 10;//(currentEvent.State.getLastValue(DATABUFFERRATIO_KEY).Value) * 500;
        }
    }
}
