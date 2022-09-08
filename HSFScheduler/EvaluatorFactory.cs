// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using HSFSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MissionElements;
using Utilities;

namespace HSFScheduler
{
    public class EvaluatorFactory
    {
        /// <summary>
        /// Static method to generate an evaluator from XML node
        /// </summary>
        /// <param name="evaluatorNode"></param>
        /// <param name="dependencies"></param>
        /// <returns></returns>
        //public static Evaluator GetEvaluator(XmlNode evaluatorNode, Dependency dependencies, List<Subsystem> SubList)
        public static Evaluator GetEvaluator(XmlNode evaluatorNode, Dependency dependencies, List<Asset> AssetList)
        {
            // search for correct fn here
            // read xml node for search parameters and find sub from list
            //string evalAssetName = evaluatorNode.Attributes["evalAssetName"].Value.ToString();
            //string evalSubName = evaluatorNode.Attributes["evalSubsystemName"].Value.ToString();
            //string evalFnName = evaluatorNode.Attributes["evalFcnName"].Value.ToString();
            //var evalSub = SubList.Find(s => s.Name == evalAssetName + "." + evalSubName);
            // search subsystems for specified eval function
            //var subType = Type.GetType("HSFSubsystem." + evalSubName).GetMethod(evalFnName);
            //var evalFnc = Delegate.CreateDelegate(typeof(Func<Event, HSFProfile<double>>), evalSub, subType);

            Evaluator schedEvaluator = new TargetValueEvaluator(dependencies); // default
            schedEvaluator.Ikeys.Add(new StateVariableKey<int>("testVariable"));
            if (evaluatorNode != null)
                schedEvaluator = new ScriptedEvaluator(evaluatorNode, dependencies);
            return schedEvaluator;
        }
    }
}
