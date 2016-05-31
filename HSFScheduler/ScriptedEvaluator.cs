using System;
using System.Collections.Generic;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting;
using System.Dynamic;
using HSFSystem;
using MissionElements;
using HSFUniverse;
using System.Xml;
using UserModel;

namespace HSFScheduler
{
    public class ScriptedEvaluator : Evaluator
    {
        #region Attributes
        private dynamic _pythonInstance;
        public Dependency Dependencies;
        #endregion

        #region Constructors
        public ScriptedEvaluator(XmlNode scriptedNode, Dependency deps)
        {
            Dependencies = deps;
            string pythonFilePath = "", className = "";
            XmlParser.ParseScriptedSrc(scriptedNode, ref pythonFilePath, ref className);
            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();
            var ops = engine.Operations;
            engine.ExecuteFile(pythonFilePath, scope);
            var pythonType = scope.GetVariable(className);
            _pythonInstance = ops.CreateInstance(pythonType, deps);
        }
        #endregion

        #region Methods
        public override double Evaluate(SystemSchedule schedule)
        {
            dynamic eval = _pythonInstance.Evaluate(schedule);
            return (double)eval;
        }

        #endregion
    }
}
