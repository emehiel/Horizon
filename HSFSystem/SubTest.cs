using HSFSystem;
using HSFUniverse;
using MissionElements;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using Utilities;

namespace HSFSubsystem
{
    //[ExcludeFromCodeCoverage]
    public class SubTest : Subsystem
    {
        #region Attributes

        Dictionary<string, double> lookup;
        protected StateVarKey<double> maj_Key;
        Dictionary<string, int> Ilookup;
        protected StateVarKey<int> IKey;
        Dictionary<string, bool> Blookup;
        protected StateVarKey<bool> BKey;
        Dictionary<string, Quat> Qlookup;
        protected StateVarKey<Quat> QKey;
        Dictionary<string, Matrix<double>> Mlookup;
        protected StateVarKey<double> MKey;


        #endregion

        #region Constructors
        /// <summary>
        /// Constructor for built in subsystems
        /// Defaults: delay: 5s
        /// </summary>
        /// <param name="TestXmlNode"></param>
        /// <param name="dependencies"></param>
        /// <param name="asset"></param>
        public SubTest(XmlNode TestXmlNode, Dependency dependencies, Asset asset)
        {
            Asset = asset;
            DependentSubsystems = new List<Subsystem>();
            SubsystemDependencyFunctions = new Dictionary<string, Delegate>();

            GetSubNameFromXmlNode(TestXmlNode);
            if (this.Name == "asset1.subtest_crop")
            {
                lookup = getList(1);
            }
            else
            {
                lookup = getList();
            }
            maj_Key = new StateVarKey<double>(Asset.Name + "." + "majorKey");
            addKey(maj_Key);
            IKey = new StateVarKey<int>(Asset.Name + "." + "IKey");
            addKey(IKey);
            BKey = new StateVarKey<bool>(Asset.Name + "." + "BKey");
            addKey(BKey);
            MKey = new StateVarKey<Matrix<double>>(Asset.Name + "." + "MKey");
            addKey(MKey);
            QKey = new StateVarKey<Quat>(Asset.Name + "." + "QKey");
            addKey(QKey);
        }

        /// <summary>
        /// Constructor for built in subsystems
        /// Defaults: delay: 5s
        /// </summary>
        /// <param name="TestNode"></param>
        /// <param name="asset"></param>

        public SubTest(XmlNode TestNode, Asset asset) : base(TestNode, asset)
        {
            Asset = asset;
        }
        #endregion Constructors

        #region Methods
        public override bool CanPerform(Event proposedEvent, Domain environment)
        {
            double es = proposedEvent.GetEventStart(Asset);
            double ts = proposedEvent.GetTaskStart(Asset);
            double te = proposedEvent.GetTaskEnd(Asset);

            string taskathand = proposedEvent.GetAssetTask(Asset).ToString();
           
            double tasknum = 0;
            lookup.TryGetValue(taskathand, out tasknum);
            if (tasknum == es)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override bool CanExtend(Event proposedEvent, Domain environment, double evalToTime)
        {
            //double es = proposedEvent.GetEventStart(Asset);
            //double ts = proposedEvent.GetTaskStart(Asset);
            //double te = proposedEvent.GetTaskEnd(Asset);
            //if (es > delayend)
            //{
            //    return false;
            //}
            //return base.CanExtend(proposedEvent, environment, evalToTime);
            return true;

        }
        static Dictionary<string, double> getList() 
        {
            Dictionary<string, double> lookup = new Dictionary<string, double>();
            lookup.Add("target0", 0);
            lookup.Add("target1", 1);
            lookup.Add("target1.1", 1);
            lookup.Add("target2", 2);
            lookup.Add("target3", 3);
            return lookup;
        }
        static Dictionary<string,double> getList(double time)
        {
            Dictionary<string, double> lookup = new Dictionary<string, double>();
            lookup.Add("target0", 0);
            lookup.Add("target1", 0);
            lookup.Add("target1.1", time);
            lookup.Add("target2", time);
            lookup.Add("target3", time);
            return lookup;
        }
        public double depFunc(Event currentEvent)
        {
            return currentEvent.EventEnds[Asset]; //no reason for this, just need to return something
        }

        #endregion Methods
    }
} 