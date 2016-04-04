using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using Utilities;
using HSFUniverse;
using HSFScheduler;

namespace HSFSchedulerUnitTest
{
    [TestClass]
    public class AccessUnitTest
    {
        [TestMethod]
        public void PregenAccessUnitTest()
        {
            double startTime = 0;
            double endTime = 100;
            double stepTime = 10;

            Matrix<double> position = new Matrix<double>(3, 1);
            Target target = new Target(position);
            Task task1 = new Task(target);

            Stack<Task> tasks = new Stack<Task>();
            tasks.Push(task1);

            List<Asset> assets = new List<Asset>();
            assets.Add(new Asset(assetDynamicState));

            SystemClass system = new SystemClass(assets);

            // DOING THIS UNTIL SYSTEMCLASS CAN BE COMPLIED - EAM
            Stack<Access> accesses = Access.pregenerateAccessesByAsset(system, tasks, startTime, endTime, stepTime);

        }

    }

    // Empty classes for unit testing only! - EAM
    class SystemClass
    {
        public List<Asset> Assets { get; set; }

        public SystemClass(List<Asset> assets)
        {
            Assets = assets;
        }
    }

    public class Asset
    {
        public DynamicState AssetDynamicState { get; private set; } //was protected, why?
        //TODO:make isTaskable mean something
        public bool IsTaskable { get; private set; }//was protected, why?

        public Asset()
        {
            IsTaskable = false;
        }
        public Asset(DynamicState dynamicState)
        {
            AssetDynamicState = dynamicState;
            IsTaskable = false;
        }

        public Asset(XmlNode assetXMLNode)
        {
            AssetDynamicState = new DynamicState(assetXMLNode["DynamicState"]);  // XmlInput Change - position => DynamicState
            IsTaskable = false;
        }

    }

    
    }
