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


namespace Utilities
{
    [Serializable]
    public class ScriptedEOMS : EOMS
    {
        #region Attributes
        [NonSerialized] private dynamic _pythonInstance;
        #endregion

        #region Constructors
        public ScriptedEOMS(XmlNode scriptedNode)
        {
            string pythonFilePath = "", className = "";
            XmlParser.ParseScriptedSrc(scriptedNode, ref pythonFilePath, ref className);
            var engine = Python.CreateEngine();
            //var engine = Python.CreateEngine();
            var scope = engine.CreateScope();
            var ops = engine.Operations;
            var p = engine.GetSearchPaths();
            p.Add(AppDomain.CurrentDomain.BaseDirectory + "\\..\\..\\..\\PythonScripting");
            engine.SetSearchPaths(p);
            engine.ExecuteFile(pythonFilePath, scope);
            var pythonType = scope.GetVariable(className);
            _pythonInstance = ops.CreateInstance(pythonType, scriptedNode);
        }
        #endregion

        #region Methods
        public override Matrix<double> this[double t, Matrix<double> y, IntegratorParameters param]
        {
            get
            {
                dynamic prop = _pythonInstance.PythonAccessor(t, y, param);
                
                return (Matrix<double>)prop;
            }
        }
        #endregion
    }
}
