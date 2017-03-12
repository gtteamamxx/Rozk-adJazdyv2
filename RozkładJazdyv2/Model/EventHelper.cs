using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RozkładJazdyv2.Model
{
    public class EventHelper
    {
        public delegate void LinesInfoDownloaded();
        public static event LinesInfoDownloaded OnLinesInfoDownloaded;

        protected EventHelper() { }

        protected internal static void InvokeOnLinesInfoDownloaded()
            => OnLinesInfoDownloaded?.Invoke();
    }
}
