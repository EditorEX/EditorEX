using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EditorEX.Util
{
    public static class Utils
    {
        public static (T, R) ExtractTupleFromKVP<T, R>(this KeyValuePair<T, R> pair)
        {
            return (pair.Key, pair.Value);
        }
    }
}