using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    [Serializable]
    public class IntegratorParameters
    {
        public ConcurrentDictionary<StateVarKey<double>, double> Ddata;
        public ConcurrentDictionary<StateVarKey<int>, int> Idata;
        public ConcurrentDictionary<StateVarKey<bool>, bool> Bdata;
        public ConcurrentDictionary<StateVarKey<Matrix<double>>, Matrix<double>> Mdata;
        public ConcurrentDictionary<StateVarKey<Vector>, Vector> Vdata;
        public ConcurrentDictionary<StateVarKey<Quat>, Quat> Qdata;

        public IntegratorParameters()
        {
            Ddata = new ConcurrentDictionary<StateVarKey<double>, double>();
            Idata = new ConcurrentDictionary<StateVarKey<int>, int>();
            Bdata = new ConcurrentDictionary<StateVarKey<bool>, bool>();
            Mdata = new ConcurrentDictionary<StateVarKey<Matrix<double>>, Matrix<double>>();
            Vdata = new ConcurrentDictionary<StateVarKey<Vector>, Vector>();
            Qdata = new ConcurrentDictionary<StateVarKey<Quat>, Quat>();
        }

        public void Add(StateVarKey<double> key, double value)
        {
            Ddata.AddOrUpdate(key, value, (k, oldValue) => value);
        }
        public double GetValue(StateVarKey<double> key)
        {
            double value;
            if (!Ddata.TryGetValue(key, out value))
                throw new KeyNotFoundException();
            return value;
        }
        public void Add(StateVarKey<int> key, int value)
        {
            Idata.AddOrUpdate(key, value, (k, oldValue) => value);
        }
        public int GetValue(StateVarKey<int> key)
        {
            int value;
            if (!Idata.TryGetValue(key, out value))
                throw new KeyNotFoundException();
            return value;
        }
        public void Add(StateVarKey<bool> key, bool value)
        {
            Bdata.AddOrUpdate(key, value, (k, oldValue) => value);
        }
        public bool GetValue(StateVarKey<bool> key)
        {
            bool value;
            if (!Bdata.TryGetValue(key, out value))
                throw new KeyNotFoundException();
            return value;
        }
        public void Add(StateVarKey<Matrix<double>> key, Matrix<double> value)
        {
            Mdata.AddOrUpdate(key, value, (k, oldValue) => value);
        }
        public Matrix<double> GetValue(StateVarKey<Matrix<double>> key)
        {
            Matrix<double> value;
            if (!Mdata.TryGetValue(key, out value))
                throw new KeyNotFoundException();
            return value;
        }
        public void Add(StateVarKey<Vector> key, Vector value)
        {
            Vdata.AddOrUpdate(key, value, (k, oldValue) => value);
        }
        public Vector GetValue(StateVarKey<Vector> key)
        {
            Vector value;
            if (!Vdata.TryGetValue(key, out value))
                throw new KeyNotFoundException();
            return value;
        }
        public void Add(StateVarKey<Quat> key, Quat value)
        {
            Qdata.AddOrUpdate(key, value, (k, oldValue) => value);
        }
        public Quat GetValue(StateVarKey<Quat> key)
        {
            Quat value;
            if (!Qdata.TryGetValue(key, out value))
                throw new KeyNotFoundException();
            return value;
        }
    }
}
