// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting;
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
            var scope = engine.CreateScope();
            var ops = engine.Operations;
            engine.ExecuteFile(pythonFilePath, scope);
            var pythonType = scope.GetVariable(className);
            _pythonInstance = ops.CreateInstance(pythonType);
        }
        #endregion

        #region Methods
        public override Matrix<double> this[double t, Matrix<double> y]
        {
            get
            {
                dynamic prop = _pythonInstance.PythonAccessor(t, y);
                return (Matrix<double>)prop;
            }
        }
        #endregion
    }
}
