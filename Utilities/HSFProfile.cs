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
    public class HSFProfile<T>
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// A map containing time-ordered values which stores SystemState data
        /// </summary>
        private readonly SortedDictionary<double, T> data = new SortedDictionary<double, T>();

        public SortedDictionary<double, T> Data
        {
            get
            { return data; }
        }
        
        #region Constructors
        /// <summary>
        /// Creates a new empty profile
        /// </summary>
        public HSFProfile()
        {
            //Default - Nothing to do
        }

        /// <summary>
        /// Creates a new profile with one initial (time, value) pair
        /// </summary>
        /// <param name="Time"></param>
        /// <param name="Value"></param>
        public HSFProfile(double Time, T Value)
        {
            data.Add(Time, Value);
        }

        /// <summary>
        /// Create a new Profile from an initial point
        /// as a KeyValuePair
        /// </summary>
        /// <param name="pointIn">The intial point in the new profile</param>
        public HSFProfile((double Time, T Value) pointIn)
        {
            data.Add(pointIn.Time, pointIn.Value);
        }


        /// <summary>
        /// Create a new Profile from two lists of equal length
        /// </summary>
        /// <param name="timeIn"></param>
        /// <param name="valIn"></param>
        public HSFProfile(List<double> Times, List<T> Values)
        {
            if (Times.Count != Values.Count)
            {
                throw new ArgumentException("Lists of Times/Values for HSFProfile are not of equal length");
            }
            else
            {
                foreach (var tup in Times.Zip(Values, (time, value) => (time, value)))
                    Add(tup.time, tup.value);
            }
        }

        public HSFProfile(List<(double Time, T Value)> TimeValuePairs)
        {
            foreach (var timeValuePair in TimeValuePairs)
                Add(timeValuePair.Time, timeValuePair.Value);
        }
        /// <summary>
        /// Create a new Profile from a copy on an existing profile
        /// </summary>
        /// <param name="dataIn"></param>
        public HSFProfile(SortedDictionary<double, T> dataIn)
        {
            data = new SortedDictionary<double, T>(dataIn);
        }

        #endregion
        
        #region Indexers
        /// <summary>
        /// Returns the Value in the HSFProfile at time Time, or if the there is a value at Time, overwrite the value,
        /// or if there is no value at Time, Add() the value at Time.
        /// </summary>
        /// <param name="Time"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Returns the first data point in the profile
        /// </summary>
        /// <returns>The KeyValuePair representing the data point in the profile</returns>
        // Old HSFProfile passed KeyValuePairs back.  Switching to Tuples.  Faster.
        public (double Time, T Value) First()
        {
            return (data.First().Key, data.First().Value);
        }

        /// <summary>
        /// Returns the first value in the profile
        /// </summary>
        /// <returns>The first value in the profile as T</returns>
        public T FirstValue()
        {
            return data.First().Value;
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
        public (double Time, T Value) Last()
        {
            return (data.Last().Key, data.Last().Value);
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
        public (double Time, T Value) DataAtTime(double Time)
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

        /// <summary>
        /// Gets a profile data value at a given time
        /// </summary>
        /// <param name="time">a double representing the time at which to get the data value</param>
        /// <returns>the data value of type T</returns>
        public T ValueAtTime(double time)
        {
            return DataAtTime(time).Value;
        }
        #endregion

        #region Integration Methods
        //TODO: This should be templated
        //TODO:  Do we want to keep these utilities?
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
        //TODO:  Do we want to keep these utilities?
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
        //TODO: This should be templated
        //TODO:  Do we want to keep these utilities?
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
        // Eric got this working correctly and tested 8/5/2022
        public T Integrate(double startTime, double endTime, double initialValue = 0)
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

            for (int i = 0; i < query.Count() - 1; i++)
            {
                dt = query[i + 1].Key - query[i].Key;
                result += dt * (dynamic)query[i].Value;
            }
            return result + (dynamic)initialValue;

        }
        #endregion

        #region Utilities
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
            var temp = data.Distinct();
            data.Clear();
            data.Union(temp);
        }
        #endregion

        #region Add Data Funtions
        // TODO:  Do we want to always overwrite values with same key?  Or throw the exception and overwrite?
        /// <summary>
        /// Adds a new data point to an existing profile
        /// </summary>
        /// <param name="Time">the time of the point to add</param>
        /// <param name="Value">the value of the point to add</param>
        public void Add(double Time, T Value)
        {
            try
            {
                data.Add(Time, Value);
            }
            catch (ArgumentException)
            {
                string msg = "An element with Time/Value pair already exists in Profile - overwriting value.";
                log.Warn(msg);
                Console.WriteLine(msg);

                data[Time] = Value;
            }
        }

        /// <summary>
        /// Adds a new data point to an existing profile
        /// </summary>
        /// <param name="pointIn">The data point to add to the profile as a KeyValuePair</param>
        public void Add((double Time, T Value) TimeValuePair)
        {
            try
            {
                data.Add(TimeValuePair.Time, TimeValuePair.Value);
            }
            catch (ArgumentException)
            {
                log.Warn("An element with Key/Value pair already exists in Profile - overwriting value.");
                data[TimeValuePair.Time] = TimeValuePair.Value;
            }
        }

        // TODO: Should this be called Merge()?
        /// <summary>
        /// Adds a new data points to an existing profile from some other existing profile
        /// </summary>
        /// <param name="otherProfile">The existing profile which is merged with this profile</param>
        public void Union(HSFProfile<T> otherProfile)
        {
            data.Union(otherProfile.Data);
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
            foreach (var data in data)
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

        /// <summary>
        /// Override addition operator to add a hsfprofile with a number
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="someNumber"></param>
        /// <returns></returns>
        public static HSFProfile<T> operator +(HSFProfile<T> p1, dynamic someNumber)
        {
            HSFProfile<T> pOut = new HSFProfile<T>();

            foreach (KeyValuePair<double, T> item in p1.data)
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
            foreach (KeyValuePair<double, T> item in p1.data)
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
            return pOut = someNumber + (-p1);
        }

        /// <summary>
        /// Override multiplication operator to scale a HSFprofile by some number
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="someNumber"></param>
        /// <returns></returns>
        public static HSFProfile<T> operator *(HSFProfile<T> p1, dynamic someNumber)
        {
            HSFProfile<T> pOut = new HSFProfile<T>();

            foreach (KeyValuePair<double, T> item in p1.data)
                pOut.Add(item.Key, item.Value * someNumber);

            return pOut;
        }

        /// <summary>
        /// Override the multiplcation operator to scale a HSFprofile by a number
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
        /// Override the division operator to divide a number by a profile
        /// </summary>
        /// <param name="someNumber"></param>
        /// <param name="p1"></param>
        /// <returns></returns>
        public static HSFProfile<T> operator /(dynamic someNumber, HSFProfile<T> p1)
        {
            HSFProfile<T> pOut = new HSFProfile<T>();
            foreach (KeyValuePair<double, T> item in p1.data)
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
            foreach (KeyValuePair<double, T> item in p1.data)
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
            return data.GetHashCode();
        }
        #endregion

       

    }
}
