// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronPython.Hosting;
using IronPython.Runtime;
using System.Dynamic;
using System.Xml;
using UserModel;
using Utilities;
using System.IO;
using Newtonsoft.Json.Linq;

namespace HSFUniverse
{
    public class ScriptedEOMS : DynamicEOMS
    {
        #region Attributes
        private dynamic _pythonInstance;

        private readonly string src;
        private readonly string className;
        #endregion

        #region Constructors
        public ScriptedEOMS(JToken scriptedEOMsJson)
        {
            src = (string)scriptedEOMsJson["src"];
            className = (string)scriptedEOMsJson["className"];
            InitPython(scriptedEOMsJson);
        }

        public ScriptedEOMS(XmlNode scriptedNode)
        {
            XmlParser.ParseScriptedSrc(scriptedNode, ref src, ref className);
            InitPython(scriptedNode);
        }

        private void InitPython(params object[] parameters)
        { 
            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();
            var ops = engine.Operations;
            var p = engine.GetSearchPaths();

            p.Add(AppDomain.CurrentDomain.BaseDirectory + "\\..\\..\\..\\PythonScripting");
            engine.SetSearchPaths(p);
            engine.ExecuteFile(src, scope);
            var pythonType = scope.GetVariable(className);
            _pythonInstance = ops.CreateInstance(pythonType, parameters);
        }
        #endregion

        #region Methods
        public override Matrix<double> this[double t, Matrix<double> y, IntegratorParameters param, Domain environment]
        {
            get
            {
                dynamic prop = _pythonInstance.PythonAccessor(t, y, param, environment);
                
                return (Matrix<double>)prop;
            }
        }
        #endregion
    }
}
