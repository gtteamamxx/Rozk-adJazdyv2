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

        public delegate void AllLinesDownloaded();
        public static event AllLinesDownloaded OnAllLinesDownloaded;

        public delegate void SqlSavingChanged(int step, int maxSteps);
        public static event SqlSavingChanged OnSqlSavingChanged;

        public delegate void SqlSaved();
        public static event SqlSaved OnSqlSaved;

        public delegate void SqlLoadingChanged(int step, int maxSteps);
        public static event SqlLoadingChanged OnSqlLoadingChanged;

        protected EventHelper() { }

        protected static void InvokeOnSqlSaved()
            => OnSqlSaved?.Invoke();

        protected static void InvokeOnSqlSavingChanged(int step, int maxSteps)
            => OnSqlSavingChanged?.Invoke(step, maxSteps);

        protected static void InvokeOnAllLinesDownloaded()
            => OnAllLinesDownloaded?.Invoke();

        protected static void InvokeOnLinesInfoDownloaded()
            => OnLinesInfoDownloaded?.Invoke();

        protected static void InvokeOnLineDownloaded(Line line, int linesCount)
            => OnLineDownloaded?.Invoke(line, linesCount);

        protected static void InvokeOnSqlLoadingChanged(int step, int maxSteps)
            => OnSqlLoadingChanged?.Invoke(step, maxSteps);
    }
}
