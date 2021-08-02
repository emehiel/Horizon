using System;
using NUnit.Framework;
using MissionElements;
using Utilities;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace UtilitiesUnitTest
{
    [TestFixture]
    public class HSFProfileUnitTest
    {
        [Test]
        public void GetProfile()
        {
            HSFProfile<double> hSFProfileD = new HSFProfile<double>(0, 2);
            HSFProfile<int> hSFProfileI = new HSFProfile<int>(0, 2);
            HSFProfile<bool> hSFProfileB = new HSFProfile<bool>(0, true);
            HSFProfile<Quat> hSFProfile = new HSFProfile<Quat>(0, new Quat(1, new Vector(3)));



        }
        [Test]
        public void EmptyHSFProfile()
        {
            HSFProfile<double> hSFProfile = new HSFProfile<double>();
            Assert.IsInstanceOf(typeof(HSFProfile<double>), hSFProfile);
        }
        [Test]
        public void BasicHSFProfile()
        {
            HSFProfile<double> hSFProfile = new HSFProfile<double>(0,2);
            Assert.IsInstanceOf(typeof(HSFProfile<double>), hSFProfile);
            Assert.AreEqual(2, hSFProfile.FirstValue());
        }
        [Test]
        public void ListHSFProfile()
        {
            List<double> times = new List<double>();
            times.Add(0); times.Add(1); times.Add(2);
            List<double> vals = new List<double>();
            vals.Add(-1); vals.Add(1); vals.Add(3);
            HSFProfile<double> hSFProfile = new HSFProfile<double>(times, vals);
            Assert.AreEqual(-1, hSFProfile.FirstValue());
             
        }
        [Test]
        public void KeyvalHSFProfile()
        {
            KeyValuePair<double, double> kvp = new KeyValuePair<double, double>(1,2);

            HSFProfile<double> hSFProfile = new HSFProfile<double>(kvp);
            Assert.AreEqual(2, hSFProfile.FirstValue());
            Assert.AreEqual(1, hSFProfile.FirstTime());
        }
        [Test]
        public void Indexer()
        {
            KeyValuePair<double, double> kvp = new KeyValuePair<double, double>(0, 1);
            HSFProfile<double> hSFProfile = new HSFProfile<double>(kvp);
            Assert.AreEqual(1, hSFProfile[0]);

            hSFProfile[1] = 2;
            Assert.AreEqual(2, hSFProfile[1]);

            hSFProfile[1] = 3;
            Assert.AreEqual(3, hSFProfile[1]);
        }
        [Test]
        public void FirstTime()
        {
            KeyValuePair<double, double> kvp = new KeyValuePair<double, double>(3, 3);
            HSFProfile<double> hSFProfile = new HSFProfile<double>(kvp);
            hSFProfile[1] = 2;
            hSFProfile[2] = 3;
            hSFProfile[0] = 1;
            Assert.AreEqual(0, hSFProfile.FirstTime());
        }
        [Test]
        public void Last()
        {
            KeyValuePair<double, double> kvp = new KeyValuePair<double, double>(3, 3);
            HSFProfile<double> hSFProfile = new HSFProfile<double>(kvp);
            hSFProfile[1] = 2;
            hSFProfile[2] = 3;
            hSFProfile[0] = 1;
            Assert.AreEqual(kvp, hSFProfile.Last());
        }
        [Test]
        public void LastVal()
        {
            KeyValuePair<double, double> kvp = new KeyValuePair<double, double>(3, 3);
            HSFProfile<double> hSFProfile = new HSFProfile<double>(kvp);
            hSFProfile[1] = 2;
            hSFProfile[2] = 3;
            hSFProfile[0] = 1;
            Assert.AreEqual(3, hSFProfile.LastValue());
        }
        [Test]
        public void LastTime()
        {
            KeyValuePair<double, double> kvp = new KeyValuePair<double, double>(3, 3);
            HSFProfile<double> hSFProfile = new HSFProfile<double>(kvp);
            hSFProfile[1] = 2;
            hSFProfile[2] = 3;
            hSFProfile[0] = 1;
            Assert.AreEqual(3, hSFProfile.LastTime());
        }
        [Test]
        public void Dataattime()
        {
            KeyValuePair<double, double> kvp = new KeyValuePair<double, double>(3, 3);
            HSFProfile<double> hSFProfile = new HSFProfile<double>(kvp);
            hSFProfile[1] = 2;
            hSFProfile[2] = 3;
            hSFProfile[0] = 1;
            Assert.AreEqual(kvp, hSFProfile.DataAtTime(3));
            Assert.AreEqual(new KeyValuePair<double,double>(1,2), hSFProfile.DataAtTime(1));

            Assert.AreEqual(new KeyValuePair<double, double>(0, 1), hSFProfile.DataAtTime(-1));

            
            try {hSFProfile.DataAtTime(4); }
            catch (ArgumentException) { Assert.Pass(); }

            HSFProfile<double> hSFProfile2 = new HSFProfile<double>();
            Assert.AreEqual(new KeyValuePair<double, double>(), hSFProfile2.DataAtTime(5));
        }
        [Test]
        public void ValAtTime()
        {
            KeyValuePair<double, double> kvp = new KeyValuePair<double, double>(3, 3);
            HSFProfile<double> hSFProfile = new HSFProfile<double>(kvp);
            hSFProfile[1] = 2;
            hSFProfile[2] = 3;
            hSFProfile[0] = 1;
            Assert.AreEqual(2, hSFProfile.ValueAtTime(1));
        }
        [Test]
        public void Integrate()//buuuuuuuuuuuuug
        {
            KeyValuePair<double, double> kvp = new KeyValuePair<double, double>(3, 3);
            HSFProfile<double> hSFProfile = new HSFProfile<double>(kvp);
            hSFProfile[1] = 1;
            hSFProfile[2] = 2;
            hSFProfile[0] = 0;
            Assert.AreEqual(4.5,hSFProfile.Integrate(0,3, 0));
            Assert.AreEqual(-7.5, hSFProfile.Integrate(0, 3, 1));
        }
        [Test]
        public void Count()
        {
            HSFProfile<double> hSFProfile = new HSFProfile<double>(1,1);
            hSFProfile[0] = 2;
            hSFProfile[2] = 9;
            Assert.AreEqual(3, hSFProfile.Count());
        }
        [Test]
        public void Empty()
        {
            HSFProfile<double> hSFProfile = new HSFProfile<double>();
            Assert.True(hSFProfile.Empty());
            hSFProfile[0] = 2;
            Assert.False(hSFProfile.Empty());
        }
        /// <summary>
        /// 3 Addition overrides tested, profile + num; num + profile; profile + profile
        /// </summary>
        [Test]
        public void Add()
        {
            HSFProfile<double> hSFProfile = new HSFProfile<double>(0,1);
            hSFProfile[1] = 2;
            hSFProfile = hSFProfile + 2;
            Assert.AreEqual(3, hSFProfile[0]);
            Assert.AreEqual(4, hSFProfile[1]);

            HSFProfile<double> hSFProfile4 = new HSFProfile<double>();
            hSFProfile = 3 + hSFProfile;
            Assert.AreEqual(6, hSFProfile[0]);
            Assert.AreEqual(7, hSFProfile[1]);

            HSFProfile<double> hSFProfile2 = new HSFProfile<double>(0, 1);
            hSFProfile2 = hSFProfile2 + hSFProfile;
            Assert.AreEqual(7, hSFProfile2[0]);
            Assert.AreEqual(8, hSFProfile2[1]);
        }
        /// <summary>
        /// 3 subtraction overrides tested, profile - num; num - profile; profile - profile
        /// </summary>
        [Test]
        public void subtract()
        {
            HSFProfile<double> hSFProfile = new HSFProfile<double>(0, 10);
            hSFProfile[1] = 11;
            hSFProfile = hSFProfile - 2;
            Assert.AreEqual(8, hSFProfile[0]);
            Assert.AreEqual(9, hSFProfile[1]);

            HSFProfile<double> hSFProfile4 = new HSFProfile<double>();
            hSFProfile = 3 - hSFProfile;
            Assert.AreEqual(-5, hSFProfile[0]);
            Assert.AreEqual(-6, hSFProfile[1]);

            HSFProfile<double> hSFProfile2 = new HSFProfile<double>(0, 1);
            hSFProfile2 = hSFProfile2 - hSFProfile;
            Assert.AreEqual(6, hSFProfile2[0]);
            Assert.AreEqual(7, hSFProfile2[1]);
        }
        [Test]
        public void multiply()
        {
            HSFProfile<double> hSFProfile = new HSFProfile<double>(0, 2);
            hSFProfile[1] = 3;
            hSFProfile = hSFProfile * 2;
            Assert.AreEqual(4, hSFProfile[0]);
            Assert.AreEqual(6, hSFProfile[1]);

            HSFProfile<double> hSFProfile4 = new HSFProfile<double>();
            hSFProfile = -3 * hSFProfile;
            Assert.AreEqual(-12, hSFProfile[0]);
            Assert.AreEqual(-18, hSFProfile[1]);
        }
    }

}
