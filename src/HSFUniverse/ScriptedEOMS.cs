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

namespace HSFUniverse
{
    [Serializable]
    public class ScriptedEOMS : DynamicEOMS
    {
        #region Attributes
        [NonSerialized] private dynamic _pythonInstance;
        #endregion

        #region Constructors
        public ScriptedEOMS(XmlNode scriptedNode)
        {
            string pythonFilePath = "", className = "";
            XmlParser.ParseScriptedSrc(scriptedNode, ref pythonFilePath, ref className);

            //  I believe this was added by Jack B. for unit testing.  Still need to sort out IO issues, but with this commented out
            //  the execuitable will look for python files in the same directory as the .exe file is located.
            //  Need to do better specifying the input and output paths.
            //if (!pythonFilePath.StartsWith("..\\")) //patch work for nunit testing which struggles with relative paths
            //{
            //    string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
            //    pythonFilePath = Path.Combine(baselocation, @pythonFilePath);
            //}
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
