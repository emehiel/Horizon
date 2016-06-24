// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using HSFSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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
        public static Evaluator GetEvaluator(XmlNode evaluatorNode, Dependency dependencies)
        {
            Evaluator schedEvaluator = new TargetValueEvaluator(dependencies); // default
            if (evaluatorNode != null)
                schedEvaluator = new ScriptedEvaluator(evaluatorNode, dependencies);
            return schedEvaluator;
        }
    }
}
