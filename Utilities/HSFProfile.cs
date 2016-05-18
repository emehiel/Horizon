using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Utilities
{
    [Serializable]
    public class HSFProfile<T> : IHSFProfile   
    {
        // Class Data

        /// <summary>
        /// A map containing time-ordered values which stores SystemState data
        /// </summary>
        private SortedDictionary<double, T> data = new SortedDictionary<double, T>();

        public SortedDictionary<double, T> Data
        {
            get
            { return data; }
        }

        // Construcotrs

        /// <summary>
        /// Creates a new empty profile
        /// </summary>
        public HSFProfile()
        {
        }

        // Does this get used?
        // If not, don't include it.  Passing a reference causes otherProfile to change when Profile changes
        /// <summary>
        /// Create a new Profile from a copy of an existing profile
        /// This constructor create a deep copy of the Profile passed in as a reference
        /// </summary>
        /// <param name="otherProfile"></param>
        public HSFProfile(HSFProfile<T> otherProfile)
        {
            foreach (var item in otherProfile.data)
                data[item.Key] = item.Value;
        }

        /// <summary>
        /// Create a new Profile from an initial point
        /// as a KeyValuePair
        /// </summary>
        /// <param name="pointIn">The intial point in the new profile</param>
        public HSFProfile(KeyValuePair<double, T> pointIn)
        {
            data.Add(pointIn.Key, pointIn.Value);
        }

        public HSFProfile<double> upperLimitIntegrateToProf(double start, double end, double saveFreq, double upperBound, ref bool exceeded, double iv, double ic)
        {
            exceeded = false;
            HSFProfile<double> prof = new HSFProfile<double>();
            double last = ic;
            double time, result;
            for (time = start + saveFreq; time < end; time += saveFreq)
            {
                
                Double.TryParse(Integrate(time - saveFreq, time, iv).ToString(), out result); //Morgan isn't sure about this
                result += last;
                if (result > upperBound)
                {
                    result = upperBound;
                    exceeded = true;
                }
                if (result != last)
                {
                    prof[time] = result;
                }
                last = result;
            }
            time -= saveFreq;
            Double.TryParse(Integrate(time, end, iv).ToString(), out result); //Morgan isn't sure about this
            result += last;
            if (result > upperBound)
            {
                result = upperBound;
                exceeded = true;
            }
            if (result != last)
                prof[end] = result;

            if (prof.Empty())
                prof[start] = ic;
            return prof;
        }

        /// <summary>
        /// Create a new Profile fron an initial point
        /// as two different input parameters timeIn, valIn
        /// </summary>
        /// <param name="timeIn"></param>
        /// <param name="valIn"></param>
        public HSFProfile(double timeIn, T valIn)
        {
            data.Add(timeIn, valIn);
        }

        /// <summary>
        /// Create a new Profile from two list of equal length
        /// </summary>
        /// <param name="timeIn"></param>
        /// <param name="valIn"></param>
        public HSFProfile(List<double> timeIn, List<T> valIn)
        {
            // TODO:  Assert that both lists are of equal length
            int i = 0;
            foreach (var item in timeIn)
            {
                data.Add(item, valIn[i]);
                i++;
            }
        }

        /// <summary>
        /// Create a new Profile from a copy on an existing profile
        /// </summary>
        /// <param name="dataIn"></param>
        public HSFProfile(SortedDictionary<double, T> dataIn)
        {
            data = new SortedDictionary<double, T>(dataIn);
        }

        // Indexers

        public T this[double key]
        {
            get
            {
                return ValueAtTime(key);
            }
            set
            {
                data[key] = value;
            }
        }


        // Accessors

        /// <summary>
        /// Returns the first data point in the profile
        /// </summary>
        /// <returns>The KeyValuePair representing the data point in the profile</returns>
        public KeyValuePair<double, T> First()
        {
            return data.First();
        }

        /// <summary>
        /// Returns the first value in the profile
        /// </summary>
        /// <returns>The first value in the profile as T</returns>
        public T FirstValue()
        {
            return data.First().Value;
        }

        #region Methods

        //TODO: (morgan ask mehiel) THis should be templated
        public HSFProfile<double> limitIntegrateToProf(double start, double end, double saveFreq, double lowerBound, double upperBound, ref bool exceeded_lower, ref bool exceeded_upper, double iv, double ic)
        {
            HSFProfile<double> prof = new HSFProfile<double>();
            double last = ic;
            double result;
            double time;
            for (time = start + saveFreq; time < end; time += saveFreq)
            {
                Double.TryParse(Integrate(time - saveFreq, time, iv).ToString(), out result); //Morgan isn't sure about this
                result += last;
                if (result.CompareTo(upperBound) >=0)
                {
                    result = upperBound;
                    exceeded_upper = true;
                }
                if (result < lowerBound)
                {
                    result = lowerBound;
                    exceeded_lower = true;
                }
                if (result != last)
                {
                    prof[time] = result;
                }
                last = result;
            }
            Double.TryParse(Integrate(time -= saveFreq, end, iv).ToString(), out result); //Morgan isn't sure about this
            result += last;
            if (result > upperBound)
            {
                result = upperBound;
                exceeded_upper = true;
            }
            if (result < lowerBound)
            {
                result = lowerBound;
                exceeded_lower = true;
            }
            if (result != last)
                prof[end] = result;

            if (prof.Empty())
                prof[start] = ic;
            return prof;

        }
        /// <summary>
        ///  Integrates the Profile with a lower limit given an initial value and condition from the start to end time
        ///  If the integral falls below the lower limit, the value for the integral is set to the lower limit
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="saveFreq"></param>
        /// <param name="lowerBound"></param>
        /// <param name="exceeded"></param>
        /// <param name="iv"></param>
        /// <param name="ic"></param>
        /// <returns></returns>
        public HSFProfile<double> lowerLimitIntegrateToProf(double start, double end, double saveFreq, double lowerBound, ref bool exceeded, double iv, double ic)
        {
            HSFProfile<double> prof = new HSFProfile<double>();
            double last = ic, time, result = 0;
            for (time = start + saveFreq; time < end; time += saveFreq)
            {
                Double.TryParse(Integrate(time - saveFreq, time, iv).ToString(), out result); //TODO: Morgan isn't sure about this
                result += last;
                if (result < lowerBound)
                {
                    result = lowerBound;
                    exceeded = true;
                }
                if (result != last)
                {
                    prof[time] = result;
                }
                last = result;
            }
            Double.TryParse(Integrate(time -= saveFreq, end, iv).ToString(), out result); //TODO: Morgan isn't sure about this
            result += last;
            if (result < lowerBound)
            {
                result = lowerBound;
                exceeded = true;
            }
            if (result != last)
                prof[end] = result;

            if (prof.Empty())
                prof[start] = ic;
            return prof;
        }

        /// <summary>
        /// Returns the time of the first data point in the profile
        /// </summary>
        /// <returns>The time of the first data point as a double</returns>
        public double FirstTime()
        {
            return data.First().Key;
        }

        /// <summary>
        /// Returns the last data point in the profile
        /// </summary>
        /// <returns>The KeyValuePair representing the data point in the profile</returns>
        public KeyValuePair<double, T> Last()
        {
            return data.Last();
        }

        /// <summary>
        /// Returns the last value in the profile
        /// </summary>
        /// <returns>Returns the last value in the profile</returns>
        public T LastValue()
        {
            return data.Last().Value;
        }

        /// <summary>
        /// Retruns the time of the last data point in the profile
        /// </summary>
        /// <returns>The time of the last data point as a doubl</returns>
        public double LastTime()
        {
            return data.Last().Key;
        }

        /// <summary>
        /// Gets a profile data point at a given time
        /// </summary>
        /// <param name="time">a double representing the time at which to get the data point</param>
        /// <returns>a KeyValuePair representing the data point</returns>
        // MAJOR TODO: what should be returned when we try to access profile data before or after the first or last key (time)?
        public KeyValuePair<double, T> DataAtTime(double time)
        {

            try
            {
                if (!Empty())
                {
                    IEnumerable<KeyValuePair<double, T>> query = data.Where(item => item.Key <= time);
                    if (query.Count() != 0)
                    {
                        var v = query.Last();
                        return v;
                    }
                    else
                    {
                        Console.WriteLine("WARNING - DataAtTime: Attempting to reference time before first data point in profile");
                        return data.Single(item => item.Key == data.Keys.Min());
                    }

                }
                else
                {
                    Console.WriteLine("WARNING - DataAtTime: Attemping to get data point from empty profile. Returning null pair.");
                    return new KeyValuePair<double, T>();  // TODO:  What to return if profile is empty?
                }
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("attempt to find data point in profile failed at time {0} failed", time);
                throw;
            }
        }

        /// <summary>
        /// Gets a profile data value at a given time
        /// </summary>
        /// <param name="time">a double representing the time at which to get the data value</param>
        /// <returns>the data value of type T</returns>
        public T ValueAtTime(double time)
        {
            return DataAtTime(time).Value;
        }


        // Utilities

        /// <summary>
        /// Sums all elements in a profile
        /// </summary>
        /// <returns>dynamic type T sum of all elements of the profile</returns>
        public T Sum()
        {
            return (dynamic)data.Sum(item => (dynamic)item.Value);
        }

        /// <summary>
        /// Returns the number of data points in the profile
        /// </summary>
        /// <returns>An integer representing the total number of data points in the profile</returns>
        public int Count()
        {
            return data.Count();
        }

        /// <summary>
        /// Returns true if a profile is empty
        /// </summary>
        /// <returns>A boolean, true for empty, false for not empty</returns>
        public bool Empty()
        {
            return data.Count() == 0;
        }

        /// <summary>
        /// Find the maximum value in the profile (only works on standard variable types)
        /// </summary>
        /// <returns>The maximum value in the profile</returns>
        public T Max()
        {
            return data.Max().Value;
        }

        /// <summary>
        /// Find the minimum value in the profile (only works on standard variable types)
        /// </summary>
        /// <returns>The minimum value in the profile</returns>
        public T Min()
        {
            return data.Min().Value;
        }
        // Functions

        /// <summary>
        /// Removes all entries unnecessary to represent a zero-order hold.
        /// The IEquatable<T> interface has to be implemented on each custom data type stored in a profile
        /// </summary>
        void RemoveDuplicates()
        {
            data = (SortedDictionary<double, T>)data.Distinct();
        }


        // Add Data Funtions

        // TODO:  Do we want to always overwrite values with same key?  Or throw the exception and overwrite?
        /// <summary>
        /// Adds a new data point to an existing profile
        /// </summary>
        /// <param name="timeIn">the time of the point to add</param>
        /// <param name="valIn">the value of the point to add</param>
        public void Add(double timeIn, T valIn)
        {
            try
            {
                data.Add(timeIn, valIn);
            }
            catch (ArgumentException)
            {
               // Console.WriteLine("An element with Key/Value pair already exists in Profile - overwriting value.");
                data[timeIn] = valIn;
            }
        }

        /// <summary>
        /// Adds a new data point to an existing profile
        /// </summary>
        /// <param name="pointIn">The data point to add to the profile as a KeyValuePair</param>
        public void Add(KeyValuePair<double, T> pointIn)
        {
            try
            {
                data.Add(pointIn.Key, pointIn.Value);
            }
            catch (ArgumentException)
            {
               // Console.WriteLine("An element with Key/Value pair already exists in Profile - overwriting value.");
                data[pointIn.Key] = pointIn.Value;
            }
        }

        /// <summary>
        /// Adds a new data points to an existing profile from some other existing profile
        /// </summary>
        /// <param name="otherProfile">The existing profile which is merged with this profile</param>
        public void Add(HSFProfile<T> otherProfile)
        {
            if (!otherProfile.Empty())
            {
                foreach (var item in otherProfile.data)
                {
                    Add(item);
                }
            }
        }

        // Operators

        public static HSFProfile<T> MergeProfiles(HSFProfile<T> p1, HSFProfile<T> p2) //Morgan made this static
        {
            HSFProfile<T> p3 = new HSFProfile<T>();
            p3.data = (SortedDictionary<double, T>)(p1.data.Union(p2.data));
            p3.RemoveDuplicates();

            return p3;
        }
        #endregion

        #region Overrides
        public static HSFProfile<T> operator +(HSFProfile<T> p1, HSFProfile<T> p2)
        {
            if (p1.Empty())
                return p2;

            if (p2.Empty())
                return p1;

            HSFProfile<T> p3 = new HSFProfile<T>();

            IEnumerable<double> p1Keys = p1.data.Keys;
            IEnumerable<double> p2Keys = p2.data.Keys;
            IEnumerable<double> uniqueTimes = p1Keys.Union(p2Keys).Distinct();

            foreach (double time in uniqueTimes)
                p3[time] = (dynamic)p1[time] + (dynamic)p2[time];

            return p3;
        }

        public static HSFProfile<T> operator +(HSFProfile<T> p1, dynamic someNumber)
        {
            HSFProfile<T> pOut = new HSFProfile<T>();

            foreach (KeyValuePair<double, T> item in p1.data)
                pOut[item.Key] += someNumber;

            return pOut;
        }

        public static HSFProfile<T> operator +(dynamic someNumber, HSFProfile<T> p1)
        {
            HSFProfile<T> pOut = new HSFProfile<T>();

            return pOut = p1 + someNumber;
        }

        public static HSFProfile<T> operator -(HSFProfile<T> p1)
        {
            HSFProfile<T> pOut = new HSFProfile<T>();
            foreach (KeyValuePair<double, T> item in p1.data)
                pOut[item.Key] = -(dynamic)p1[item.Key];

            return pOut;
        }

        public static HSFProfile<T> operator -(HSFProfile<T> p1, HSFProfile<T> p2)
        {
            HSFProfile<T> pOut = new HSFProfile<T>();
            return pOut = p1 + (-p2);
        }

        public static HSFProfile<T> operator -(HSFProfile<T> p1, dynamic someNumber)
        {
            HSFProfile<T> pOut = new HSFProfile<T>();
            return pOut = p1 + (-someNumber);
        }

        public static HSFProfile<T> operator -(dynamic someNumber, HSFProfile<T> p1)
        {
            HSFProfile<T> pOut = new HSFProfile<T>();
            return pOut = p1 + (-someNumber);
        }

        public static HSFProfile<T> operator *(HSFProfile<T> p1, dynamic someNumber)
        {
            HSFProfile<T> pOut = new HSFProfile<T>();

            foreach (KeyValuePair<double, T> item in p1.data)
                pOut[item.Key] *= someNumber;

            return pOut;
        }

        public static HSFProfile<T> operator *(dynamic someNumber, HSFProfile<T> p1)
        {
            HSFProfile<T> pOut = new HSFProfile<T>();

            return pOut = p1 * someNumber;
        }

        public static HSFProfile<T> operator /(dynamic someNumber, HSFProfile<T> p1)
        {
            HSFProfile<T> pOut = new HSFProfile<T>();

            return pOut = p1 * (1.0 / someNumber);
        }

        public static HSFProfile<T> operator /(HSFProfile<T> p1, dynamic someNumber)
        {
            HSFProfile<T> pOut = new HSFProfile<T>();

            return pOut = p1 * (1.0 / someNumber);
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            // TODO: write your implementation of Equals() here
            HSFProfile<T> p = obj as HSFProfile<T>;
            return data == p.data;

        }

        public static bool operator == (HSFProfile<T> p1, HSFProfile<T> p2)
        {
            return p1.Equals(p2);
        }

        public static bool operator != (HSFProfile<T> p1, HSFProfile<T> p2)
        {
            return !(p1 == p2);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            // TODO: write your implementation of GetHashCode() here
            return data.GetHashCode();
        }

        public double Integrate(double startTime, double endTime, double initialValue)
        {
            if (endTime < startTime)
                return -1.0 * Integrate(startTime, endTime, initialValue);
            if (Count() == 0 || endTime <= data.First().Key)
                return (endTime - startTime) * initialValue;
            if (endTime == startTime)
                return 0;

            //  KeyValuePair<double, double> query = (SortedDictionary<double, double>)data.Where(item => item.Key >= startTime && item.Key <= endTime);
            double query = initialValue;
            foreach (var prof in data)
            {
                if (prof.Key >= startTime && prof.Key <= endTime)
                {
                    query+= (dynamic)prof.Value;
                }
            }
            return query;
          //  return query.Sum(queryItem => queryItem.Value) + initialValue;
        }
        #endregion

    }
}
