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
        protected StateVariableKey<double> maj_Key;
        //Dictionary<string, int> Ilookup;
        //protected StateVariableKey<int> IKey;
        //Dictionary<string, bool> Blookup;
        //protected StateVariableKey<bool> BKey;
        //Dictionary<string, Quaternion> Qlookup;
        //protected StateVariableKey<Quaternion> QKey;
        //Dictionary<string, Matrix<double>> Mlookup;
        //protected StateVariableKey<double> MKey;


        #endregion

        #region Constructors
        /// <summary>
        /// Constructor for built in subsystems
        /// Defaults: delay: 5s
        /// </summary>
        /// <param name="TestXmlNode"></param>

        public SubTest(XmlNode TestXmlNode)
        {

            lookup = getList();

            //maj_Key = new StateVariableKey<double>(Asset.Name + "." + "majorKey");
            //addKey(maj_Key);
            //IKey = new StateVariableKey<int>(Asset.Name + "." + "IKey");
            //addKey(IKey);
            //BKey = new StateVariableKey<bool>(Asset.Name + "." + "BKey");
            //addKey(BKey);
            //MKey = new StateVariableKey<Matrix<double>>(Asset.Name + "." + "MKey");
            //addKey(MKey);
            //QKey = new StateVariableKey<Quaternion>(Asset.Name + "." + "QKey");
            //addKey(QKey);
        }


        #endregion Constructors

        #region Methods
        public override bool CanPerform(Event proposedEvent, Domain environment)
        {
            double es = proposedEvent.GetEventStart(Asset);
            double ee = proposedEvent.GetEventEnd(Asset);
            double ts = proposedEvent.GetTaskStart(Asset);
            double te = proposedEvent.GetTaskEnd(Asset);

            string taskathand = proposedEvent.GetAssetTask(Asset).ToString();
           
            double tasknum = 0;
            lookup.TryGetValue(taskathand, out tasknum);
            if (tasknum == es)
            {
                //if (taskathand == "target1")
                //    proposedEvent.SetTaskEnd(Asset, ee + 0.25);
                //else
                //    proposedEvent.SetTaskEnd(Asset, ee - 0.25);
                return true;
            }
            else
            {
                return false;
            }
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