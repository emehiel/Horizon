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
        public Dependency Dependencies
        {
            get { return (Dependency)_pythonInstance.Dependencies; }
            set { _pythonInstance.Dependencies = value; }
        }
        #endregion

        #region Constructors
        public ScriptedEvaluator(XmlNode scriptedNode, Dependency deps)
        {
            string pythonFilePath = "", className = "";
            XmlParser.ParseScriptedSrc(scriptedNode, ref pythonFilePath, ref className);
            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();
            var ops = engine.Operations;
            engine.ExecuteFile(pythonFilePath, scope);
            var pythonType = scope.GetVariable(className);
            _pythonInstance = ops.CreateInstance(pythonType, deps);
            Dependencies = deps;
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
