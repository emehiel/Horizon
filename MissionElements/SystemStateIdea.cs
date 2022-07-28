using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using System.Xml;
using UserModel;

namespace MissionElements
{
    public class SystemStateIdea
    {
        /** The previous state, upon which this state is based */
        public SystemStateIdea Previous { get; set; }

        /** The Dictionary of integer Profiles. */
        private Dictionary<string, HSFProfile<int>> Idata = new Dictionary<string, HSFProfile<int>>();

        /** The Dictionary of double precision Profiles. */
        private Dictionary<string, HSFProfile<double>> Ddata = new Dictionary<string, HSFProfile<double>>();

        /** The Dictionary of boolean Profiles. */
        private Dictionary<string, HSFProfile<bool>> Bdata = new Dictionary<string, HSFProfile<bool>>();

        /** The Dictionary of Matrix Profiles. */
        private Dictionary<string, HSFProfile<Matrix<double>>> Mdata = new Dictionary<string, HSFProfile<Matrix<double>>>();

        /** The Dictionary of Quaternion Profiles. */
        private Dictionary<string, HSFProfile<Quaternion>> Qdata = new Dictionary<string, HSFProfile<Quaternion>>();

        /** The Dictionary of Vector Profiles. */
        private Dictionary<string, HSFProfile<Vector>> Vdata = new Dictionary<string, HSFProfile<Vector>>();

        public SystemStateIdea()
        {
            Previous = null;
        }
        public SystemStateIdea(SystemStateIdea previous)
        {
            Previous = previous;
        }

        public void AddValue(string stateVariableName, double time, int stateValue)
        {
            if (Idata.TryGetValue(stateVariableName, out HSFProfile<int> valueOut)) // If there's a Profile matching that key,add this data point to the existing Profile.
                valueOut[time] = stateValue;
            else // Otherwise, insert a new one.
                Idata[stateVariableName] = new HSFProfile<int>(time, stateValue);
        }

        public void AddValue(string stateVariableName, double time, double stateValue)
        {
            if (Ddata.TryGetValue(stateVariableName, out HSFProfile<double> valueOut)) // If there's a Profile matching that key,add this data point to the existing Profile.
                valueOut[time] = stateValue;
            else // Otherwise, insert a new one.
                Ddata[stateVariableName] = new HSFProfile<double>(time, stateValue);
        }
        public void AddValue(string stateVariableName, double time, bool stateValue)
        {
            if (Bdata.TryGetValue(stateVariableName, out HSFProfile<bool> valueOut)) // If there's a Profile matching that key,add this data point to the existing Profile.
                valueOut[time] = stateValue;
            else // Otherwise, insert a new one.
                Bdata[stateVariableName] = new HSFProfile<bool>(time, stateValue);
        }
        public void AddValue(string stateVariableName, double time, Matrix<double> stateValue)
        {
            if (Mdata.TryGetValue(stateVariableName, out HSFProfile<Matrix<double>> valueOut)) // If there's a Profile matching that key,add this data point to the existing Profile.
                valueOut[time] = stateValue;
            else // Otherwise, insert a new one.
                Mdata[stateVariableName] = new HSFProfile<Matrix<double>>(time, stateValue);
        }
        public void AddValue(string stateVariableName, double time, Quaternion stateValue)
        {
            if (Qdata.TryGetValue(stateVariableName, out HSFProfile<Quaternion> valueOut)) // If there's a Profile matching that key,add this data point to the existing Profile.
                valueOut[time] = stateValue;
            else // Otherwise, insert a new one.
                Qdata[stateVariableName] = new HSFProfile<Quaternion>(time, stateValue);
        }
        public void AddValue(string stateVariableName, double time, Vector stateValue)
        {
            if (Vdata.TryGetValue(stateVariableName, out HSFProfile<Vector> valueOut)) // If there's a Profile matching that key,add this data point to the existing Profile.
                valueOut[time] = stateValue;
            else // Otherwise, insert a new one.
                Vdata[stateVariableName] = new HSFProfile<Vector>(time, stateValue);
        }

        public void AddValues(string stateVariableName, List<Tuple<double, int>> stateValues)
        {
            foreach(var stateValue in stateValues)
                AddValue(stateVariableName, stateValue.Item1, stateValue.Item2);
        }
        /// <summary>
        /// This method returns the last state variable value stored for the given state variable name.
        /// This method requires no two state variables have the same name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stateVariableName"></param>
        /// <returns></returns>
        public T GetLastValue<T>(string stateVariableName)
        {
            if(Idata.TryGetValue(stateVariableName, out HSFProfile<int> iValueOut))
            {
                return (T)Convert.ChangeType(iValueOut.LastValue(), typeof(T));
            }
            else if (Ddata.TryGetValue(stateVariableName, out HSFProfile<double> dValueOut))
            {
                return (T)Convert.ChangeType(dValueOut.LastValue(), typeof(T));
            }
            else if (Mdata.TryGetValue(stateVariableName, out HSFProfile<Matrix<double>> mValueOut))
            {
                return (T)Convert.ChangeType(mValueOut.LastValue(), typeof(T));
            }
            else
            {
                Console.WriteLine($"State variable {stateVariableName} not found");
                return default;
            }
        }

        public void SetInitialSystemState(XmlNode ICNode, Asset asset)
        {
            string type = ICNode.Attributes["type"].Value;
            string stateVariableName = asset.Name + "." + ICNode.Attributes["key"].Value.ToLower(); // This may be changing to not use asset.name
            double time = SimParameters.SimStartSeconds;

            if (type.ToLower().Equals("int") || type.ToLower().Equals("integer"))
            {
                Int32.TryParse(ICNode.Attributes["value"].Value, out int stateValue);
                AddValue(stateVariableName, time, stateValue);
            }
            else if (type.ToLower().Equals("double"))
            {
                Double.TryParse(ICNode.Attributes["value"].Value, out double stateValue);
                AddValue(stateVariableName, time, stateValue);
            }
            else if (type.ToLower().Equals("bool"))
            {
                string val = ICNode.Attributes["value"].Value;
                bool stateValue = false;
                if (val.ToLower().Equals("true") || val.Equals("1"))
                    stateValue = true;
                AddValue(stateVariableName, time, stateValue);
            }
            else if (type.ToLower().Equals("matrix"))
            {
                Matrix<double> stateValue = new Matrix<double>(ICNode.Attributes["value"].Value);
                AddValue(stateVariableName, time, stateValue);
            }
            else if (type.ToLower().Equals("quat") || type.ToLower().Equals("quaternion"))
            {
                Quaternion stateValue = new Quaternion(ICNode.Attributes["value"].Value);
                AddValue(stateVariableName, time, stateValue);
            }
            else if (type.ToLower().Equals("vector"))
            {
                Vector stateValue = new Vector(ICNode.Attributes["value"].Value);
                AddValue(stateVariableName, time, stateValue);
            }
            else
            {
                Console.WriteLine($"State variable {stateVariableName} of type {type} is not supported by HSF.");
            }
        }

        public override string ToString()
        {
            string stateData = "";
            foreach (var data in Ddata)
            {
                stateData += data.Key + "," + data.Value.ToString();
            }
            return stateData;
        }
    }
}
