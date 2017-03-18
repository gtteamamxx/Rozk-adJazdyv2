using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RozkładJazdyv2.Model
{
    internal static class ListExtension
    {
        internal static List<T> Add<T>(this List<T> list, T obj)
        {
            list.Add(obj);
            return list;
        }
    }
}
