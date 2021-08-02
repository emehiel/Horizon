


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
using System.IO;

namespace HSFUniverse
{
    [Serializable]
    public class ScriptedUniverse : Domain
    {
        #region Attributes
        [NonSerialized] private dynamic _pythonInstance;
        #endregion

        #region Constructors
        public ScriptedUniverse(XmlNode scriptedNode)
        {
            string pythonFilePath = "", className = "";
            XmlParser.ParseScriptedSrc(scriptedNode, ref pythonFilePath, ref className);

            if (!pythonFilePath.StartsWith("..\\")) //patch work for nunit testing which struggles with relative paths
            {
                string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
                pythonFilePath = Path.Combine(baselocation, @pythonFilePath);
            }

            var engine = Python.CreateEngine();
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
        /// <summary>
        /// Returns object specified by string.
        /// The environment used may be composed of various objects.
        /// functions need to be defined within scripted code.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <returns></returns>
        public override T GetObject<T>(string s)
        {
            return (T)_pythonInstance.get_Object(s.ToLower());
        }

        /// <summary>
        /// Sets new value to specified object.
        /// functions need to be defined within scripted code.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <param name="val"></param>
        public override void SetObject<T>(string s, T val)
        {
            _pythonInstance.set_Object(s.ToLower(), val);
        }

        /// <summary>
        /// Returns atmosphere data for specified geometric height.
        /// functions need to be defined within scripted code.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public override double GetAtmosphere(string s, double h)
        {
            return _pythonInstance.get_Atmosphere(s.ToLower(), h);
        }
        #endregion
    }
}
