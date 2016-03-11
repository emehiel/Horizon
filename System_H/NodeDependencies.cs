using HSFSubsystem;
using UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace HSFSystem
{
    public class NodeDependencies
    {
        public Dependencies DepInstance { get; private set; }
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
