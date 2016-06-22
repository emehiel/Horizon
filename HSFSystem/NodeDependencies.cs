// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using HSFSubsystem;
using UserModel;
using System;
using System.Collections.Generic;
using Utilities;

namespace HSFSystem
{
    public class NodeDependencies
    {
        public Dependency DepInstance { get; private set; }
        public bool ScriptingEnabled;
        public PythonState PyState;
        public int ThreadNum;

        Dictionary<string, HSFProfile<int>> IntDependencies;
        Dictionary<string, HSFProfile<double>> DoubleDependencies;
     //   Dictionary<string, HSFProfile<float>> FloatDependencies;
        Dictionary<string, HSFProfile<bool>> BoolDependencies;
        Dictionary<string, HSFProfile<Matrix<double>>> MatrixDependencies;
        Dictionary<string, HSFProfile<Quat>> QuatDependencies;
        Dictionary<string, string> IntDependencies_scripted;
        Dictionary<string, string> DoubleDependencies_scripted;
        Dictionary<string, string> FloatDependencies_scripted;
        Dictionary<string, string> BoolDependencies_scripted;
        Dictionary<string, string> MatrixDependencies_scripted;
        Dictionary<string, string> QuatDependencies_scripted;
        private NodeDependencies subsystemDependencies;

        public NodeDependencies(NodeDependencies subsystemDependencies)
        {
            this.subsystemDependencies = subsystemDependencies;
        }

        void setPyState(PythonState state)
        {
            throw new NotImplementedException();
        }
    }
}
