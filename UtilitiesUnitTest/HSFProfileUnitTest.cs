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
            //runtime constructor check
            HSFProfile<double> hSFProfileD = new HSFProfile<double>(0, 2);
            HSFProfile<int> hSFProfileI = new HSFProfile<int>(0, 2);
            HSFProfile<bool> hSFProfileB = new HSFProfile<bool>(0, true);
            HSFProfile<Quat> hSFProfile = new HSFProfile<Quat>(0, new Quat(1, new Vector(3)));
        }
        [Test]
        public void EmptyHSFProfile()
        {
            //act
            HSFProfile<double> hSFProfile = new HSFProfile<double>();
            //assert
            Assert.IsInstanceOf(typeof(HSFProfile<double>), hSFProfile);
        }
        [Test]
        public void BasicHSFProfile()
        {
            //act
            HSFProfile<double> hSFProfile = new HSFProfile<double>(0,2);
            //assert
            Assert.IsInstanceOf(typeof(HSFProfile<double>), hSFProfile);
            Assert.AreEqual(2, hSFProfile.FirstValue());
        }
        [Test]
         public void ListHSFProfile()
        {
            //arrange
            List<double> times = new List<double>();
            times.Add(0); times.Add(1); times.Add(2);
            List<double> vals = new List<double>();
            vals.Add(-1); vals.Add(1); vals.Add(3);
            //act
            HSFProfile<double> hSFProfile = new HSFProfile<double>(times, vals);
            //assert
            Assert.AreEqual(-1, hSFProfile.FirstValue());
             
        }
        [Test]
        public void KeyvalHSFProfile()
        {
            //arrange
            KeyValuePair<double, double> kvp = new KeyValuePair<double, double>(1,2);
            //act
            HSFProfile<double> hSFProfile = new HSFProfile<double>(kvp);
            //assert
            Assert.AreEqual(2, hSFProfile.FirstValue());
            Assert.AreEqual(1, hSFProfile.FirstTime());
        }
        [Test]
        public void Indexer()
        {
            //arrange
            KeyValuePair<double, double> kvp = new KeyValuePair<double, double>(0, 1);
            HSFProfile<double> hSFProfile = new HSFProfile<double>(kvp);

            //assert + act with indexing hSFProfile
            Assert.AreEqual(1, hSFProfile[0]);

            hSFProfile[1] = 2;
            Assert.AreEqual(2, hSFProfile[1]);

            hSFProfile[1] = 3;
            Assert.AreEqual(3, hSFProfile[1]);
        }
        [Test]
        public void FirstTime()
        {
            //arrange
            KeyValuePair<double, double> kvp = new KeyValuePair<double, double>(3, 3);
            HSFProfile<double> hSFProfile = new HSFProfile<double>(kvp);
            hSFProfile[1] = 2;
            hSFProfile[2] = 3;
            hSFProfile[0] = 1;

            //act
            double t0 = hSFProfile.FirstTime();

            //assert
            Assert.AreEqual(0, t0);
        }
        [Test]
        public void Last()
        {
            //arrange
            KeyValuePair<double, double> kvp = new KeyValuePair<double, double>(3, 3);
            HSFProfile<double> hSFProfile = new HSFProfile<double>(kvp);
            hSFProfile[1] = 2;
            hSFProfile[2] = 3;
            hSFProfile[0] = 1;

            //act
            KeyValuePair<double, double> kvpAct = hSFProfile.Last();

            //assert
            Assert.AreEqual(kvp, kvpAct);
        }
        [Test]
        public void LastVal()
        {
            //arrange
            KeyValuePair<double, double> kvp = new KeyValuePair<double, double>(3, 3);
            HSFProfile<double> hSFProfile = new HSFProfile<double>(kvp);
            hSFProfile[1] = 2;
            hSFProfile[2] = 3;
            hSFProfile[0] = 1;

            //act
            double lastValue = hSFProfile.LastValue();

            //assert
            Assert.AreEqual(3, lastValue);
        }
        [Test]
        public void LastTime()
        {
            //arrange
            KeyValuePair<double, double> kvp = new KeyValuePair<double, double>(3, 3);
            HSFProfile<double> hSFProfile = new HSFProfile<double>(kvp);
            hSFProfile[1] = 2;
            hSFProfile[2] = 3;
            hSFProfile[0] = 1;

            //act
            double lastTime = hSFProfile.LastTime();

            //assert
            Assert.AreEqual(3,lastTime);
        }
        [Test]
        public void Dataattime()
        {
            //arrange
            KeyValuePair<double, double> kvp = new KeyValuePair<double, double>(3, 3);
            HSFProfile<double> hSFProfile = new HSFProfile<double>(kvp);
            HSFProfile<double> hSFProfile2 = new HSFProfile<double>();
            hSFProfile[1] = 2;
            hSFProfile[2] = 3;
            hSFProfile[0] = 1;

            //act
            KeyValuePair<double, double>  dataAtTime0 = hSFProfile.DataAtTime(0);
            KeyValuePair<double, double>  dataAtTime1 = hSFProfile.DataAtTime(1);
            KeyValuePair<double, double> dataAtTime2 = hSFProfile.DataAtTime(-1);
            KeyValuePair<double, double> dataAtTime5 = hSFProfile2.DataAtTime(5);

            KeyValuePair<double, double> kvp1 = new KeyValuePair<double, double>(1, 2);
            KeyValuePair<double, double> kvp2 = new KeyValuePair<double, double>(1, 2);
            KeyValuePair<double, double> kvp0 = new KeyValuePair<double, double>(0, 1);
            KeyValuePair<double, double> kvp5 = new KeyValuePair<double, double>(0, 0);

            //assert
            Assert.AreEqual(kvp0, dataAtTime0);
            Assert.AreEqual(kvp1,dataAtTime1);
            Assert.AreEqual(kvp0, dataAtTime2);
            Assert.AreEqual(kvp5, dataAtTime5);

            try {hSFProfile.DataAtTime(4); }
            catch (ArgumentException) { Assert.Pass(); }

            

        }
        [Test]
        public void ValAtTime()
        {
            //arrange
            KeyValuePair<double, double> kvp = new KeyValuePair<double, double>(3, 3);
            HSFProfile<double> hSFProfile = new HSFProfile<double>(kvp);
            hSFProfile[1] = 2;
            hSFProfile[2] = 3;
            hSFProfile[0] = 1;

            //act
            double valAtTime = hSFProfile.ValueAtTime(1);

            //assert
            Assert.AreEqual(2, valAtTime);
        }
        [Test]
        public void Integrate() //TODO: HSFProfile.Integrate needs attention. See Github "Project" section: https://github.com/emehiel/Horizon/projects
        {
            //arrange
            KeyValuePair<double, double> kvp = new KeyValuePair<double, double>(3, 3);
            HSFProfile<double> hSFProfile = new HSFProfile<double>(kvp);
            hSFProfile[1] = 1;
            hSFProfile[2] = 2;
            hSFProfile[0] = 0;

            //act
            double integ = hSFProfile.Integrate(0, 3, 0);
            
            //assert
            Assert.AreEqual(6,integ);
        }
        [Test]
        public void Count()
        {
            //arrange
            HSFProfile<double> hSFProfile = new HSFProfile<double>(1,1);
            hSFProfile[0] = 2;
            hSFProfile[2] = 9;

            //act
            double count = hSFProfile.Count();

            //assert
            Assert.AreEqual(3, count);
        }
        [Test]
        public void Empty()
        {
            //arrange
            HSFProfile<double> hSFProfile = new HSFProfile<double>();
            HSFProfile<double> hSFProfile2 = new HSFProfile<double>();
            hSFProfile2[0] = 2;

            //act
            bool isEmpty = hSFProfile.Empty();
            bool isNotEmpty = hSFProfile2.Empty();

            //assert
            Assert.True(isEmpty);
            Assert.False(isNotEmpty);
        }
        /// <summary>
        /// 3 Addition overrides tested, profile + num; num + profile; profile + profile
        /// </summary>
        [Test]
        public void Add()
        {
            //arrange
            HSFProfile<double> hSFProfile = new HSFProfile<double>(0,1);
            HSFProfile<double> hSFProfile2 = new HSFProfile<double>(0, 1);
            HSFProfile<double> hSFProfile3 = new HSFProfile<double>(0, 1);



            //act
            hSFProfile[1] = 2;
            hSFProfile2[1] = 2;
            hSFProfile2 = hSFProfile2 + 4;
            hSFProfile3 = hSFProfile2 + hSFProfile;

            //assert
            Assert.AreEqual(1, hSFProfile[0]);
            Assert.AreEqual(2, hSFProfile[1]);
            Assert.AreEqual(5, hSFProfile2[0]);
            Assert.AreEqual(6, hSFProfile2[1]);
            Assert.AreEqual(6, hSFProfile3[0]);
            Assert.AreEqual(8, hSFProfile3[1]);

        }
        /// <summary>
        /// 3 subtraction overrides tested, profile - num; num - profile; profile - profile
        /// </summary>
        [Test]
        public void subtract()
        {
            //arrange
            HSFProfile<double> hSFProfile = new HSFProfile<double>(0, 10);
            hSFProfile[1] = 11;
            HSFProfile<double> hSFProfile2 = new HSFProfile<double>(0, 1);
            HSFProfile<double> hSFProfile1 = new HSFProfile<double>(0, 1);

            //act
            hSFProfile = hSFProfile - 2;
            hSFProfile1 = 3 - hSFProfile;
            hSFProfile2 = hSFProfile1 - hSFProfile;

            //assert
            Assert.AreEqual(8, hSFProfile[0]);
            Assert.AreEqual(9, hSFProfile[1]);
            Assert.AreEqual(-5, hSFProfile1[0]);
            Assert.AreEqual(-6, hSFProfile1[1]);
            Assert.AreEqual(-13, hSFProfile2[0]);
            Assert.AreEqual(-15, hSFProfile2[1]);
        }
        [Test]
        public void multiply()
        {
            //arrange
            HSFProfile<double> hSFProfile = new HSFProfile<double>(0, 2);
            HSFProfile<double> hSFProfile2 = new HSFProfile<double>(0, 2);
            hSFProfile[1] = 3;
            hSFProfile2[1] = 3;

            //act
            hSFProfile = hSFProfile * 2;
            hSFProfile2 = -3 * hSFProfile;

            //assert
            Assert.AreEqual(4, hSFProfile[0]);
            Assert.AreEqual(6, hSFProfile[1]);
            Assert.AreEqual(-12, hSFProfile2[0]);
            Assert.AreEqual(-18, hSFProfile2[1]);
        }
    }

}
