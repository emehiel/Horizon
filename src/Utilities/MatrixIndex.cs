// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    [Serializable]
    public class MatrixIndex
    {

        public int Lower { get; set; }
        public int Upper { get; set; }
        public int Step { get; set; }

        public int Length
        {
            get
            {
                if (Upper == Lower)
                    return 1;
                else
                    return (this.Upper - this.Lower) / this.Step + 1;
            }
        }

        public MatrixIndex()
        {
            this.Lower = 1;
            this.Upper = 1;
            this.Step = 0;
        }

        public MatrixIndex(int upperLower)
        {
            this.Lower = upperLower;
            this.Upper = upperLower;
            this.Step = 0;
        }

        public MatrixIndex(int lower, int upper)
        {
            this.Lower = lower;
            this.Upper = upper;
            this.Step = 1;
        }

        public MatrixIndex(int lower, int step, int upper)
        {
            this.Lower = lower;
            this.Upper = upper;
            this.Step = step;
        }

        public static implicit operator MatrixIndex(int index)
        {
            return new MatrixIndex(index);
        }

        public int this[int index]
        {
            get
            {
                if (index <= 0)
                    throw new IndexOutOfRangeException("Index must be non-negative");
                else if (index > this.Length)
                    throw new IndexOutOfRangeException("Index exceeds length of MatrixIndex");
                else
                    return this.Lower + this.Step * (index - 1);
            }
        }
    }
}
