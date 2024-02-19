// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)


using IronPython.Hosting;
using HSFSystem;
using System.Xml;
using UserModel;
using System.IO;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

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
        /// <summary>
        /// Constructor to create the scriped evaluator and initialize the python instance
        /// </summary>
        /// <param name="scriptedNode"></param>
        /// <param name="deps"></param>
        //public ScriptedEvaluator(XmlNode scriptedNode, Dependency deps)
        public ScriptedEvaluator(JObject scriptedJson, List<dynamic> keychain)
        {
            string src = "", className = "";
            //XmlParser.ParseScriptedSrc(scriptedNode, ref src, ref className);

            JsonLoader<string>.TryGetValue("src", scriptedJson, out src);
            JsonLoader<string>.TryGetValue("ClassName", scriptedJson, out className);


            if (!src.StartsWith("..\\")) //patch work for nunit testing which struggles with relative paths
            {
                string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
                src = Path.Combine(baselocation, src);
            }

            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();
            var ops = engine.Operations;
            engine.ExecuteFile (src, scope);
            var pythonType = scope.GetVariable(className);
            _pythonInstance = ops.CreateInstance(pythonType, keychain);
            //_pythonInstance = ops.CreateInstance(pythonType, deps);
            //Dependencies = deps;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Implementation of the evaluate method
        /// </summary>
        /// <param name="schedule"></param>
        /// <returns></returns>
        public override double Evaluate(SystemSchedule schedule)
        {
            dynamic eval = _pythonInstance.Evaluate(schedule);
            double test = 1;
            return (double)eval;
        }
        #endregion
    }
}
