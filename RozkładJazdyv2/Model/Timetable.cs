using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RozkładJazdyv2.Model
{
    public class Timetable : EventHelper
    {
        public static Timetable Instance { get; protected internal set; }

        protected internal Timetable() { }

        public List<Line> Lines { get; set; }
        public List<BusStopName> BusStopsNames { get; set; }
        public List<TrackName> TracksNames { get; set; }
        public List<HourName> HoursNames { get; set; }
        public List<LetterName> LettersNames { get; set; }
        public List<Letter> Letters { get; set; }

        public static async Task<bool> LoadTimetableFromLocalCacheAsync()
        {
            if (!(await SQLServices.IsValidDatabaseAsync()))
                return false;
            SQLServices.LoadTimetableFromDatabase();
            return true;
        }

        public static async Task<bool> DownloadTimetableFromInternetAsync()
        {
            if (!(InternetConnectionService.IsInternetConnectionAvailable()))
                return false;
            bool isTimetableDownloaded = await TimetableDownloadService.DownloadNewTimetableAsync();
            return isTimetableDownloaded;
        }
    }
}
