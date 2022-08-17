using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    [Serializable]
    //[ExcludeFromCodeCoverage]
    public class IntegratorParameters
    {
        public ConcurrentDictionary<StateVariableKey<double>, double> Ddata;
        public ConcurrentDictionary<StateVariableKey<int>, int> Idata;
        public ConcurrentDictionary<StateVariableKey<bool>, bool> Bdata;
        public ConcurrentDictionary<StateVariableKey<Matrix<double>>, Matrix<double>> Mdata;
        public ConcurrentDictionary<StateVariableKey<Vector>, Vector> Vdata;
        public ConcurrentDictionary<StateVariableKey<Quaternion>, Quaternion> Qdata;

        public IntegratorParameters()
        {
            Ddata = new ConcurrentDictionary<StateVariableKey<double>, double>();
            Idata = new ConcurrentDictionary<StateVariableKey<int>, int>();
            Bdata = new ConcurrentDictionary<StateVariableKey<bool>, bool>();
            Mdata = new ConcurrentDictionary<StateVariableKey<Matrix<double>>, Matrix<double>>();
            Vdata = new ConcurrentDictionary<StateVariableKey<Vector>, Vector>();
            Qdata = new ConcurrentDictionary<StateVariableKey<Quaternion>, Quaternion>();
        }

        public void Add(StateVariableKey<double> key, double value)
        {
            Ddata.AddOrUpdate(key, value, (k, oldValue) => value);
        }
        public double GetValue(StateVariableKey<double> key)
        {
            double value;
            if (!Ddata.TryGetValue(key, out value))
                throw new KeyNotFoundException();
            return value;
        }
        public void Add(StateVariableKey<int> key, int value)
        {
            Idata.AddOrUpdate(key, value, (k, oldValue) => value);
        }
        public int GetValue(StateVariableKey<int> key)
        {
            int value;
            if (!Idata.TryGetValue(key, out value))
                throw new KeyNotFoundException();
            return value;
        }
        public void Add(StateVariableKey<bool> key, bool value)
        {
            Bdata.AddOrUpdate(key, value, (k, oldValue) => value);
        }
        public bool GetValue(StateVariableKey<bool> key)
        {
            bool value;
            if (!Bdata.TryGetValue(key, out value))
                throw new KeyNotFoundException();
            return value;
        }
        public void Add(StateVariableKey<Matrix<double>> key, Matrix<double> value)
        {
            Mdata.AddOrUpdate(key, value, (k, oldValue) => value);
        }
        public Matrix<double> GetValue(StateVariableKey<Matrix<double>> key)
        {
            Matrix<double> value;
            if (!Mdata.TryGetValue(key, out value))
                throw new KeyNotFoundException();
            return value;
        }
        public void Add(StateVariableKey<Vector> key, Vector value)
        {
            Vdata.AddOrUpdate(key, value, (k, oldValue) => value);
        }
        public Vector GetValue(StateVariableKey<Vector> key)
        {
            Vector value;
            if (!Vdata.TryGetValue(key, out value))
                throw new KeyNotFoundException();
            return value;
        }
        public void Add(StateVariableKey<Quaternion> key, Quaternion value)
        {
            Qdata.AddOrUpdate(key, value, (k, oldValue) => value);
        }
        public Quaternion GetValue(StateVariableKey<Quaternion> key)
        {
            Quaternion value;
            if (!Qdata.TryGetValue(key, out value))
                throw new KeyNotFoundException();
            return value;
        }
    }
}
