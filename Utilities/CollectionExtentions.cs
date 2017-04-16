// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class CollectionExtentions
    {
        public static int UpperBoundIndex<T>(
                    this IList<T> sortedCollection, T key
                ) where T : IComparable<T>
        {
            int begin = 0;
            int end = sortedCollection.Count;
            while (end > begin)
            {
                int index = (begin + end) / 2;
                T el = sortedCollection[index];
                if (el.CompareTo(key) >= 0)
                    end = index;
                else
                    begin = index + 1;
            }
            return end;
        }

        public static int LowerBoundIndex<T>(
            this IList<T> sortedCollection, T key
        ) where T : IComparable<T>
        {
            int begin = 0;
            int end = sortedCollection.Count;
            while (end > begin)
            {
                int index = (begin + end) / 2;
                T el = sortedCollection[index];
                if (el.CompareTo(key) >= 0)
                    end = index;
                else
                    begin = index + 1;
            }
            return end - 1;
        }

        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>
            (this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct =
              new[] { Enumerable.Empty<T>() };
            IEnumerable<IEnumerable<T>> result = emptyProduct;
            foreach (IEnumerable<T> sequence in sequences)
            {
                var s = sequence;
                result = from accseq in result from item in s select accseq.Concat(new[] { item });
            }
            return result;
        }
    }
}
