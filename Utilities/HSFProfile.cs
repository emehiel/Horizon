// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace Utilities
{
    [Serializable]
    // Assume the value of the data prior to the first time entry is zero (zero order hold)
    // Assume the value of the data after the last time entry is the last value in data
    public class HSFProfile<T> //: SortedDictionary<double, T>   
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /*
        public new void Add(double time, T value)
        {
            base.Add(time, value);
        }

        public new T this[double time]
        {
            get { return base[time]; }
            set { base[time] = value; }
        }
        */

        /// <summary>
        /// A map containing time-ordered values which stores SystemState data
        /// </summary>
        private SortedDictionary<double, T> _data = new SortedDictionary<double, T>();

        public SortedDictionary<double, T> Data
        {
            get
            { return _data; }
        }

        /// <summary>
        /// Creates a new empty profile
        /// </summary>
        public HSFProfile()
        {
        }

        /// <summary>
        /// Create a new Profile from an initial point
        /// as a KeyValuePair
        /// </summary>
        /// <param name="pointIn">The intial point in the new profile</param>
        public HSFProfile(KeyValuePair<double, T> pointIn)
        {
            _data.Add(pointIn.Key, pointIn.Value);
        }

        /// <summary>
        /// Create a new Profile fron an initial point
        /// as two different input parameters timeIn, valIn
        /// </summary>
        /// <param name="timeIn"></param>
        /// <param name="valIn"></param>
        public HSFProfile(double timeIn, T valIn)
        {
            _data.Add(timeIn, valIn);
        }

        /// <summary>
        /// Create a new Profile from two lists of equal length
        /// </summary>
        /// <param name="timeIn"></param>
        /// <param name="valIn"></param>
        public HSFProfile(List<double> timeIn, List<T> valIn)
        {
            // TODO:  Assert that both lists are of equal length
            int i = 0;
            foreach (var item in timeIn)
            {
                _data.Add(item, valIn[i]);
                i++;
            }
        }

        /// <summary>
        /// Create a new Profile from a copy on an existing profile
        /// </summary>
        /// <param name="dataIn"></param>
        public HSFProfile(SortedDictionary<double, T> dataIn)
        {
            _data = new SortedDictionary<double, T>(dataIn);
        }

        #region Indexers
        public T this[double key]
        {
            get
            {
                return ValueAtTime(key);
            }
            set
            {
            if (_data.ContainsKey(key))
                _data[key] = value;
            else
                _data.Add(key, value);
            }
        }
        #endregion

        #region Accessors
        /// <summary>
        /// Returns the first data point in the profile
        /// </summary>
        /// <returns>The KeyValuePair representing the data point in the profile</returns>
        public KeyValuePair<double, T> First()
        {
            return _data.First();
        }

        /// <summary>
        /// Returns the first value in the profile
        /// </summary>
        /// <returns>The first value in the profile as T</returns>
        public T FirstValue()
        {
            return _data.First().Value;
        }
        /// <summary>
        /// Returns the time of the first data point in the profile
        /// </summary>
        /// <returns>The time of the first data point as a double</returns>
        public double FirstTime()
        {
            return _data.First().Key;
        }

        /// <summary>
        /// Returns the last data point in the profile
        /// </summary>
        /// <returns>The KeyValuePair representing the data point in the profile</returns>
        public (double Time, T Value) Last()
        {
            return (_data.Last().Key, _data.Last().Value);
        }

        /// <summary>
        /// Returns the last value in the profile
        /// </summary>
        /// <returns>Returns the last value in the profile</returns>
        public T LastValue()
        {
            return _data.Last().Value;
        }

        /// <summary>
        /// Retruns the time of the last data point in the profile
        /// </summary>
        /// <returns>The time of the last data point as a doubl</returns>
        public double LastTime()
        {
            return _data.Last().Key;
        }

        /// <summary>
        /// Gets a profile data point at a given time
        /// </summary>
        /// <param name="time">a double representing the time at which to get the data point</param>
        /// <returns>a KeyValuePair representing the data point</returns>
        // MAJOR TODO: what should be returned when we try to access profile data before or after the first or last key (time)?
        public (double Time, T Value) DataAtTime(double time)
        {

            try
            {
                if (!Empty())
                {
                    IEnumerable<KeyValuePair<double, T>> query = _data.Where(item => item.Key <= time);
                    if (query.Count() != 0)
                    {
                        var v = query.Last();
                        return (v.Key, v.Value);
                    }
                    else
                    {
                        log.Warn("WARNING - DataAtTime: Attempting to reference time before first data point in profile");
                        var r = _data.Single(item => item.Key == _data.Keys.Min());
                        return (r.Key, r.Value);
                    }

                }
                else
                {
                    log.Warn("WARNING - DataAtTime: Attemping to get data point from empty profile. Returning null pair.");
                    return (0, default);  // TODO:  What to return if profile is empty?
                }
            }
            catch (ArgumentException e)
            {
                log.Warn("attempt to find data point in profile failed at time " + time + "failed");
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
            return DataAtTime(time).Item2;
        }
        #endregion

        #region Integration Methods
        /// <summary>
        /// Integrate the profile from start to end time and set to upper limit if limit is exceeded
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="saveFreq"></param>
        /// <param name="upperBound"></param>
        /// <param name="exceeded"></param>
        /// <param name="iv"></param>
        /// <param name="ic"></param>
        /// <returns></returns>
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

        //TODO: This should be templated
        /// <summary>
        /// Integrate the profile from start to end time and set to limit if exceeded
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="saveFreq"></param>
        /// <param name="lowerBound"></param>
        /// <param name="upperBound"></param>
        /// <param name="exceeded_lower"></param>
        /// <param name="exceeded_upper"></param>
        /// <param name="iv"></param>
        /// <param name="ic"></param>
        /// <returns></returns>
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
        /// Integrate a value from the start to end time
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="initialValue"></param>
        /// <returns></returns>
        public double Integrate(double startTime, double endTime, double initialValue)
        {
            if (endTime < startTime)
                return -1.0 * Integrate(endTime, startTime, initialValue);
            if (Count() == 0 || endTime <= _data.First().Key)
                return (endTime - startTime) * initialValue;
            if (endTime == startTime)
                return 0;

            //  KeyValuePair<double, double> query = (SortedDictionary<double, double>)data.Where(item => item.Key >= startTime && item.Key <= endTime);
            double query = initialValue;
            foreach (var prof in _data)
            {
                if (prof.Key >= startTime && prof.Key <= endTime)
                {
                    query += (dynamic)prof.Value;
                }
            }
            return query;
            //  return query.Sum(queryItem => queryItem.Value) + initialValue;
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Sums all elements in a profile
        /// </summary>
        /// <returns>dynamic type T sum of all elements of the profile</returns>
        public T Sum()
        {
            return (dynamic)_data.Sum(item => (dynamic)item.Value);
        }

        /// <summary>
        /// Returns the number of data points in the profile
        /// </summary>
        /// <returns>An integer representing the total number of data points in the profile</returns>
        public int Count()
        {
            return _data.Count();
        }

        /// <summary>
        /// Returns true if a profile is empty
        /// </summary>
        /// <returns>A boolean, true for empty, false for not empty</returns>
        public bool Empty()
        {
            return _data.Count() == 0;
        }

        /// <summary>
        /// Find the maximum value in the profile (only works on standard variable types)
        /// </summary>
        /// <returns>The maximum value in the profile</returns>
        public T Max()
        {
            dynamic max = _data.First().Value;
            foreach (var item in _data)
                if (item.Value > max)
                    max = item.Value;

            return max;
        }

        /// <summary>
        /// Find the minimum value in the profile (only works on standard variable types)
        /// </summary>
        /// <returns>The minimum value in the profile</returns>
        public T Min()
        {
            dynamic min = _data.First().Value;
            foreach (var item in _data)
                if (item.Value > min)
                    min = item.Value;

            return min;
        }
        // Functions

        /// <summary>
        /// Removes all entries unnecessary to represent a zero-order hold.
        /// The IEquatable<T> interface has to be implemented on each custom data type stored in a profile
        /// </summary>
        void RemoveDuplicates()
        {
            _data = (SortedDictionary<double, T>)_data.Distinct();
        }
        #endregion

        #region Add Data Funtions
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
                _data.Add(timeIn, valIn);
            }
            catch (ArgumentException)
            {
                log.Warn("An element with Key/Value pair already exists in Profile - overwriting value.");
                _data[timeIn] = valIn;
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
                _data.Add(pointIn.Key, pointIn.Value);
            }
            catch (ArgumentException)
            {
                log.Warn("An element with Key/Value pair already exists in Profile - overwriting value.");
                _data[pointIn.Key] = pointIn.Value;
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
                foreach (var item in otherProfile._data)
                {
                    Add(item);
                }
            }
        }
        #endregion

        #region Operators
        /// <summary>
        /// Merge the data in two profiles by performing a union
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static HSFProfile<T> MergeProfiles(HSFProfile<T> p1, HSFProfile<T> p2) //Morgan made this static
        {
            HSFProfile<T> p3 = new HSFProfile<T>();
            p3._data = (SortedDictionary<double, T>)(p1._data.Union(p2._data));
            p3.RemoveDuplicates();

            return p3;
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Override of the ToString method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string dataString = "";
            foreach (var data in _data)
            {
                dataString += data.Key + "," + data.Value + ",";
            }
            return dataString;
        }

        /// <summary>
        /// Override addition operator for adding to hsfprofiles
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static HSFProfile<T> operator +(HSFProfile<T> p1, HSFProfile<T> p2)
        {
            if (p1.Empty())
                return p2;

            if (p2.Empty())
                return p1;

            HSFProfile<T> p3 = new HSFProfile<T>();

            IEnumerable<double> p1Keys = p1._data.Keys;
            IEnumerable<double> p2Keys = p2._data.Keys;
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

        /// <summary>
        /// Override addition operator to add a hsfprofile with a number
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="someNumber"></param>
        /// <returns></returns>
        public static HSFProfile<T> operator +(HSFProfile<T> p1, dynamic someNumber)
        {
            HSFProfile<T> pOut = new HSFProfile<T>();

            foreach (KeyValuePair<double, T> item in p1._data)
                pOut.Add(item.Key, item.Value + someNumber);

            return pOut;
        }

        /// <summary>
        /// Override addition operator to add a hsfprofile with a number
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="someNumber"></param>
        /// <returns></returns>
        public static HSFProfile<T> operator +(dynamic someNumber, HSFProfile<T> p1)
        {
            HSFProfile<T> pOut = new HSFProfile<T>();

            return pOut = p1 + someNumber;
        }

        /// <summary>
        /// Override subtraction operator to make a hsfprofile negative
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="someNumber"></param>
        /// <returns></returns>
        public static HSFProfile<T> operator -(HSFProfile<T> p1)
        {
            HSFProfile<T> pOut = new HSFProfile<T>();
            foreach (KeyValuePair<double, T> item in p1._data)
                pOut.Add(item.Key, -1 * (dynamic)item.Value);

            return pOut;
        }

        /// <summary>
        /// Override subtraction operator to subtract two hsfprofiles
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="someNumber"></param>
        /// <returns></returns>
        public static HSFProfile<T> operator -(HSFProfile<T> p1, HSFProfile<T> p2)
        {
            HSFProfile<T> pOut = new HSFProfile<T>();
            return pOut = p1 + (-p2);
        }

        /// <summary>
        /// Override subtraction operator to subtract two hsfprofiles
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="someNumber"></param>
        /// <returns></returns>
        public static HSFProfile<T> operator -(HSFProfile<T> p1, dynamic someNumber)
        {
            HSFProfile<T> pOut = new HSFProfile<T>();
            return pOut = p1 + (-someNumber);
        }

        /// <summary>
        /// Override subtraction operator to subtract a number from a hsfprofile
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="someNumber"></param>
        /// <returns></returns>
        public static HSFProfile<T> operator -(dynamic someNumber, HSFProfile<T> p1)
        {
            HSFProfile<T> pOut = new HSFProfile<T>();
            return pOut = -p1 + someNumber;
        }

        /// <summary>
        /// Override subtraction operator to subtract a number from a hsfprofile
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="someNumber"></param>
        /// <returns></returns>
        public static HSFProfile<T> operator *(HSFProfile<T> p1, dynamic someNumber)
        {
            HSFProfile<T> pOut = new HSFProfile<T>();

            foreach (KeyValuePair<double, T> item in p1._data)
                pOut.Add(item.Key, item.Value * someNumber);

            return pOut;
        }

        /// <summary>
        /// Override the multiplcation operator to scale a profile by a number
        /// </summary>
        /// <param name="someNumber"></param>
        /// <param name="p1"></param>
        /// <returns></returns>
        public static HSFProfile<T> operator *(dynamic someNumber, HSFProfile<T> p1)
        {
            HSFProfile<T> pOut = new HSFProfile<T>();

            return pOut = p1 * someNumber;
        }

        /// <summary>
        /// Override the division operator to divide a profile by a number
        /// </summary>
        /// <param name="someNumber"></param>
        /// <param name="p1"></param>
        /// <returns></returns>
        public static HSFProfile<T> operator /(dynamic someNumber, HSFProfile<T> p1)
        {
            HSFProfile<T> pOut = new HSFProfile<T>();
            foreach (KeyValuePair<double, T> item in p1._data)
                pOut.Add(item.Key, someNumber / item.Value);

            return pOut;
        }

        /// <summary>
        /// Override the division operator to divide a profile by a number
        /// </summary>
        /// <param name="someNumber"></param>
        /// <param name="p1"></param>
        /// <returns></returns>
        public static HSFProfile<T> operator /(HSFProfile<T> p1, dynamic someNumber)
        {
            HSFProfile<T> pOut = new HSFProfile<T>();
            foreach (KeyValuePair<double, T> item in p1._data)
                pOut.Add(item.Key, item.Value / someNumber);

            return pOut;
        }
    
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

            // TODO: write your implementation of Equals() here
            HSFProfile<T> p = obj as HSFProfile<T>;
            bool areEqual = true;
            foreach (var item in _data.Zip(p._data, Tuple.Create))
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
        public static bool operator == (HSFProfile<T> p1, HSFProfile<T> p2)
        {
            return p1.Equals(p2);
        }

        /// <summary>
        /// Override the logical not equals operator
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static bool operator != (HSFProfile<T> p1, HSFProfile<T> p2)
        {
            return !(p1 == p2);
        }

        /// <summary>
        /// Override the object GetHashCode method
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            // TODO: write your implementation of GetHashCode() here
            return _data.GetHashCode();
        }
        #endregion

       

    }
}
