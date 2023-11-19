// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Xml;
using Utilities;


namespace HSFUniverse{

    [Serializable]
    //[ExcludeFromCodeCoverage]
    public abstract class Domain
    {
        protected virtual void CreateUniverse() { }
        protected virtual void CreateUniverse(XmlNode environmentNode) { }
        public virtual double GetAtmosphere(string s, double h) { return 0; }
        public virtual void SetObject<T>(string s, T val) { }
        public abstract T GetObject<T>(string s);
    }

}