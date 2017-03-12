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

        public delegate void LineDownloaded(Line line, int linesCount);
        public static event LineDownloaded OnLineDownloaded;

        protected EventHelper() { }

        protected internal static void InvokeOnLinesInfoDownloaded()
            => OnLinesInfoDownloaded?.Invoke();

        protected internal static void InvokeOnLineDownloaded(Line line, int linesCount)
            => OnLineDownloaded?.Invoke(line, linesCount);
    }
}
