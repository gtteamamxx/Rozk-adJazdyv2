using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RozkładJazdyv2.Model
{
    public sealed class Timetable
    {
        public static Timetable Instance { get; }

        private Timetable() { }

        public static bool LoadTimetableFromLocalCache()
        {
            if (!SQLServices.IsValidDatabase())
                return false;

            SQLServices.LoadTimetableFromDatabase();
            return true;
        }

        public async static Task<bool> DownloadTimetableFromInternetAsync()
        {
            if (!(InternetConnectionService.IsInternetConnectionAvailable()))
                return false;

            await TimetableDownloadService.DownloadNewTimetableAsync();
            return true;
        }
    }
}
