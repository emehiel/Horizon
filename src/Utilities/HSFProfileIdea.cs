using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Utilities
{
    public class HSFProfileIdea<T>
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// A map containing time-ordered values which stores SystemState data
        /// </summary>
        private readonly SortedDictionary<double, T> data = new SortedDictionary<double, T>();

        #region Constructors

        public HSFProfileIdea()
        {
            //Default - do nothing
        }

        public HSFProfileIdea(double Time, T Value)
        {
            data.Add(Time, Value);
        }

        public HSFProfileIdea(List<double> Times, List<T> Values)
        {
            if(Times.Count != Values.Count)
            {
                throw new ArgumentException("Lists of Times/Values for HSFProfile are not of equal length");
            }
            else
            {
                foreach (var tup in Times.Zip(Values, (time, value) => (time, value)))
                    Add(tup.time, tup.value);
            }
        }
        // Limit to these constructors.  The old HSFProfile has a constructor that takes
        // a KeyValuePair as an input.  We could change to a Tuple, but I don't see the
        // value in doing this...
        #endregion



        #region Indexer
        public T this[double Time]
        {
            get
            {
                return ValueAtTime(Time);
            }
            set
            {
                if (data.ContainsKey(Time))
                    data[Time] = value;
                else
                    data.Add(Time, value);
            }
        }

        #endregion

        #region Accessors
        // Old HSFProfile passed KeyValuePairs back.  Switching to Tuples.  Faster.
        public (double Time, T Value) First()
        {
            return (data.First().Key, data.First().Value);
        }

        public T FirstValue()
        {
            return data.Values.First();
        }

        public double FirstTime()
        {
            return data.Keys.First();
        }

        public (double Time, T Value) Last()
        {
            return (data.Last().Key, data.Last().Value);
        }

        public T LastValue()
        {
            return data.Values.Last();
        }

        public double LastTime()
        {
            return data.Keys.Last();
        }

        private (double Time, T Value) DataAtTime(double Time)
        {
            try
            {
                if (Empty())
                {
                    log.Warn("WARNING - DataAtTime: Attemping to get data point from empty profile. Returning null pair.");
                    return (0, (T)new object());  // TODO:  What to return if profile is empty?
                }
                else
                {
                    var query = data.Where(item => item.Key <= Time);
                    if (query.Count() != 0)
                    {
                        var v = query.Last();
                        return (v.Key, v.Value);
                    }
                    else
                    {
                        log.Warn("WARNING - DataAtTime: Attempting to reference time before first data point in profile");
                        var v = data.Single(item => item.Key == data.Keys.Min());
                        return (v.Key, v.Value);
                    }
                }
            }
            catch
            {
                string msg = $"Attempt to find data point in profile failed at time {Time} failed";
                log.Warn(msg);
                throw new ArgumentException(msg);
            }
        }

        private T ValueAtTime(double Time)
        {
            return DataAtTime(Time).Value;
        }
        #endregion

        #region Add Data Methods
        public void Add(double Time, T Value)
        {
            try
            {
                data.Add(Time, Value);
            }
            catch (ArgumentException)
            {
                log.Warn("An element with Key/Value pair already exists in Profile - overwriting value.");
                Console.WriteLine("An element with Key/Value pair already exists in Profile - overwriting value.");

                data[Time] = Value;
            }
        }

        public void Add(HSFProfileIdea<T> Data)
        {
            if (!Data.Empty())
            {
                foreach(var item in Data.data)
                    Add(item.Key, item.Value);
            }
        }

        #endregion

        #region  Integration Methods

        /// <summary>
        /// Integrate a HSFProfile from the start to end time with initial condition
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="initialValue"></param>
        /// <returns></returns>
        public T Integrate(double startTime, double endTime, T initialValue)
        {
            if (endTime < startTime)
                return -1.0 * (dynamic)Integrate(endTime, startTime, initialValue);
            if (Count() == 0 || endTime <= data.First().Key)
                return (endTime - startTime) * (dynamic)initialValue;
            if (endTime == startTime)
                return default;

            var query = data.Where(item => item.Key >= startTime && item.Key <= endTime).ToList();
            query.Add(new KeyValuePair<double, T>(endTime, query.Last().Value));
            if (query.First().Key != startTime)
                query.Insert(0, new KeyValuePair<double, T>(startTime, this[startTime]));
            
            T result = default;
            double dt = 0;
            
            for(int i = 0; i < query.Count() - 1; i++)
            {
                dt = query[i + 1].Key - query[i].Key;
                result += dt * (dynamic)query[i].Value;
            }
            return result + (dynamic)initialValue;

        }

        #endregion

        #region Utilities

        public T Sum()
        {
            return (dynamic)data.Sum(item => (dynamic)item.Value);
        }
        public bool Empty()
        {
            return data.Count == 0;
        }

        public int Count()
        {
            return data.Count();
        }

        public T Max()
        {
            return (dynamic)data.Max();
        }

        public T Min()
        {
            return (dynamic)data.Min();
        }

        // Functions

        /// <summary>
        /// Removes all entries unnecessary to represent a zero-order hold.
        /// The IEquatable<T> interface has to be implemented on each custom data type stored in a profile
        /// </summary>
        void RemoveDuplicates()
        {
            var temp = data.Distinct();
            data.Clear();
            data.Union(temp);
        }
        #endregion

        #region Overrides

        public override string ToString()
        {
            string dataString = "";
            foreach (var point in data)
            {
                dataString += point.Key + "," + point.Value.ToString() + ",";
            }
            return dataString.TrimEnd(',');
        }

        /// <summary>
        /// Override addition operator for adding to hsfprofiles
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static HSFProfileIdea<T> operator +(HSFProfileIdea<T> p1, HSFProfileIdea<T> p2)
        {
            if (p1.Empty())
                return p2;

            if (p2.Empty())
                return p1;

            HSFProfileIdea<T> p3 = new HSFProfileIdea<T>();

            IEnumerable<double> p1Keys = p1.data.Keys;
            IEnumerable<double> p2Keys = p2.data.Keys;
            IEnumerable<double> uniqueTimes = p1Keys.Union(p2Keys).Distinct();

            foreach (double time in uniqueTimes)
            {
                if (time < p1.FirstTime())
                    p3.Add(time, (dynamic)p2[time]);
                else if (time < p2.FirstTime())
                    p3.Add(time, (dynamic)p1[time]);
                else
                    p3.Add(time, (dynamic)p1[time] + (dynamic)p2[time]);
            }

            return p3;
        }

        //TODO:  Other operators here

        /// <summary>
        /// Override the object equals method
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            HSFProfileIdea<T> p = obj as HSFProfileIdea<T>;
            bool areEqual = true;
            foreach (var item in data.Zip(p.data, Tuple.Create))
            {
                areEqual &= item.Item1.Key == item.Item2.Key;
                areEqual &= (dynamic)item.Item1.Value == item.Item2.Value;
            }
            return areEqual;
        }

        /// <summary>
        /// Override the logical equals operator
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static bool operator ==(HSFProfileIdea<T> p1, HSFProfileIdea<T> p2)
        {
            return p1.Equals(p2);
        }

        /// <summary>
        /// Override the logical not equals operator
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static bool operator !=(HSFProfileIdea<T> p1, HSFProfileIdea<T> p2)
        {
            return !(p1 == p2);
        }

        /// <summary>
        /// Override the object GetHashCode method
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return data.GetHashCode();
        }
        #endregion
    }
}
