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
        public static Evaluator GetEvaluator(XmlNode evaluatorNode, Dependency dependencies)
        {
            Evaluator schedEvaluator = new TargetValueEvaluator(dependencies); // default
            if (evaluatorNode != null)
                schedEvaluator = new ScriptedEvaluator(evaluatorNode, dependencies);
            return schedEvaluator;
        }
    }
}
