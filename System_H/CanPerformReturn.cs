using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;

namespace HSFSystem
{
    public class DependencyFuncReturn<T>
    {
        public bool Performable { get; set; }
        public HSFProfile<T> Data { get; set; }

        public DependencyFuncReturn()
        {
            Performable = false;
        }

        public DependencyFuncReturn(bool performable, HSFProfile<T> data)
        {
            Performable = performable;
            Data = data;
        }
    }
}
