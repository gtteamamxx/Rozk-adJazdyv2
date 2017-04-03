using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RozkładJazdyv2.Model
{
    public static class IEnumerableExtension
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> list)
        {
            var observableCollection = new ObservableCollection<T>();
            foreach (var p in list)
                observableCollection.Add(p);
            return observableCollection;
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IOrderedEnumerable<T> list)
        {
            var observableCollection = new ObservableCollection<T>();
            foreach (var p in list)
                observableCollection.Add(p);
            return observableCollection;
        }
    }
}
