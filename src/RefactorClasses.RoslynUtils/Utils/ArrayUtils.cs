using System;
using System.Collections.Generic;
using System.Text;

namespace RefactorClasses.RoslynUtils.Utils
{
    public static class ArrayUtils
    {
        public static T[] CreateArray<T>(int size, T value)
        {
            var res = new T[size];
            for (int i = 0; i < res.Length; ++i)
                res[i] = value;

            return res;
        }
    }
}
