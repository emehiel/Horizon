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
        private double _bufferSize = 4098;
        public static StateVarKey<double> DATABUFFERRATIO_KEY = new StateVarKey<double>("DataBufferFillRatio");

        public SSDR(XmlNode SSDRXmlNode, Dependencies dependencies, Asset asset)
        {
            DefaultSubName = "SSDR";
            getSubNameFromXmlNode(SSDRXmlNode);
            Asset = asset;
            if (SSDRXmlNode.Attributes["bufferSize"] != null)
                _bufferSize = (double)Convert.ChangeType(SSDRXmlNode.Attributes["bufferSize"].Value.ToString(), typeof(double));
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
        public override bool canPerform(SystemState oldState, SystemState newState,
                    Dictionary<Asset, Task> tasks, Universe environment)
        {
            if (!base.canPerform(oldState, newState, tasks, environment))
                return false;
            if (_task.Type == TaskType.IMAGING)
            {
                double ts = newState.TaskStart;
                double te = newState.TaskEnd;
                double oldbufferratio = oldState.getLastValue(Dkeys.First()).Value;
                HSFProfile<double> newdataratein = (DependencyCollector() / _bufferSize);

                bool exceeded = false;
                HSFProfile<double> newdataratio = newdataratein.upperLimitIntegrateToProf(ts, te, 5, 1, ref exceeded, 0, oldbufferratio);
                if (!exceeded)
                {
                    newState.addValue(DATABUFFERRATIO_KEY, newdataratio);
                    return true;
                }
                Logger.Report("SSDR");
                return false;
            }
            else if (_task.Type == TaskType.COMM)
            {
                double ts = newState.TaskStart;
                newState.TaskEnd = ts + 60.0;
                double te = newState.TaskEnd;

                double data = _bufferSize * oldState.getLastValue(Dkeys.First()).Value;
                double dataqueout = data / 2 > 50 ? data / 2 : data;

                if (data - dataqueout < 0)
                    dataqueout = data;

                if (dataqueout > 0)
                    newState.addValue(DATABUFFERRATIO_KEY, new KeyValuePair<double, double>(te, (data - dataqueout) / _bufferSize));
                return true;
            }
            return true;
        }

        private HSFProfile<double> DependencyCollector()
        {
            throw new NotImplementedException();
        }
    }
}
