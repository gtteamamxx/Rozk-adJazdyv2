using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace RozkładJazdyv2.Model
{
    public class SQLServices : Timetable
    {
        private static readonly string _SQLFileName = "Timetable.sqlite";
        private static readonly string _SQLTempFileName = "Timetable_T.sqlite";

        private static string _SQLFilePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, _SQLFileName);
        private static string _SQLTempFilePath => Path.Combine(ApplicationData.Current.LocalFolder.Path, _SQLTempFilePath);

        private static SQLiteConnection _SQLConnection;

        private static int _SAVING_STEPS = 14;

        private SQLServices() { }

        public static void InitSQL()
            => _SQLConnection = new SQLiteConnection(new SQLitePlatformWinRT(), _SQLFilePath);

        private static bool IsDatabaseFileExist()
            => File.Exists(_SQLFilePath);

        public static bool IsValidDatabase()
        {
            if (!IsDatabaseFileExist())
                return false;
            string query = "SELECT * FROM sqlite_master WHERE name LIKE 'Line';";
            SQLiteCommand command = _SQLConnection.CreateCommand(query);
            string result = command.ExecuteScalar<string>();
            bool isValidDatabase = result != null && result.Length > 0;
            return isValidDatabase;
        }

        public static bool SaveDatabase()
        {
            InvokeOnSqlSavingChanged(1, _SAVING_STEPS);
            bool isFileRemoved = DeleteFile(_SQLTempFilePath);
            if (!isFileRemoved)
                return false;
            var tempSqlConnection = new SQLiteConnection(new SQLitePlatformWinRT(), _SQLTempFilePath);
            if (!CreateDatabase(tempSqlConnection))
                return false;
            if (!InsertIntoDatabase(tempSqlConnection))
                return false;
            InvokeOnSqlSavingChanged(14, _SAVING_STEPS);
            tempSqlConnection.Close();
            isFileRemoved = DeleteFile(_SQLFilePath);
            if (!isFileRemoved)
                return false;
            bool isFileRenamed = RenameTempFileToMainFile();
            if (!isFileRemoved)
                return false;
            InvokeOnSqlSaved();
            return true;
        }

        private static bool RenameTempFileToMainFile()
        {
            try
            {
                File.Move(_SQLTempFilePath, _SQLFilePath);
                return true;
            }
            catch
            {
                return false;
            }
        }
        private static bool InsertIntoDatabase(SQLiteConnection sqlConnection)
        {
            try
            {
                List<Schedule> listOfSchedules = new List<Schedule>();
                List<Track> listOfTracks = new List<Track>();
                List<BusStop> listOfBusStops = new List<BusStop>();
                List<Hour> listOfHours = new List<Hour>();
                InvokeOnSqlSavingChanged(4, _SAVING_STEPS);
                foreach (var line in Timetable.Instance.Lines)
                {
                    foreach(var schedule in line.Schedules)
                    {
                        listOfSchedules.Add(schedule);
                        foreach(var track in schedule.Tracks)
                        {
                            listOfTracks.Add(track);
                            foreach(var busStop in track.BusStops)
                            {
                                listOfBusStops.Add(busStop);
                                foreach(var hour in busStop.Hours)
                                    listOfHours.Add(hour);
                            }
                        }
                    }
                }
                InvokeOnSqlSavingChanged(5, _SAVING_STEPS);
                sqlConnection.InsertAll(Timetable.Instance.BusStopsNames);
                InvokeOnSqlSavingChanged(6, _SAVING_STEPS);
                sqlConnection.InsertAll(Timetable.Instance.HoursNames);
                InvokeOnSqlSavingChanged(7, _SAVING_STEPS);
                sqlConnection.InsertAll(Timetable.Instance.Letters);
                InvokeOnSqlSavingChanged(8, _SAVING_STEPS);
                sqlConnection.InsertAll(Timetable.Instance.Lines);
                InvokeOnSqlSavingChanged(9, _SAVING_STEPS);
                sqlConnection.InsertAll(Timetable.Instance.TracksNames);
                InvokeOnSqlSavingChanged(10, _SAVING_STEPS);
                sqlConnection.InsertAll(listOfSchedules);
                InvokeOnSqlSavingChanged(11, _SAVING_STEPS);
                sqlConnection.InsertAll(listOfTracks);
                InvokeOnSqlSavingChanged(12, _SAVING_STEPS);
                sqlConnection.InsertAll(listOfBusStops);
                InvokeOnSqlSavingChanged(13, _SAVING_STEPS);
                sqlConnection.InsertAll(listOfHours);
            }
            catch
            {
                return false;
            }
            return true;
        }

        private static bool CreateDatabase(SQLiteConnection sqlConnection)
        {
            try
            {
                InvokeOnSqlSavingChanged(2, _SAVING_STEPS);
                sqlConnection.CreateTable<Line>();
                sqlConnection.CreateTable<BusStopName>();
                sqlConnection.CreateTable<TrackName>();
                sqlConnection.CreateTable<HourName>();
                sqlConnection.CreateTable<Letter>();
                sqlConnection.CreateTable<Schedule>();
                sqlConnection.CreateTable<Track>();
                sqlConnection.CreateTable<BusStop>();
                sqlConnection.CreateTable<Hour>();
                InvokeOnSqlSavingChanged(3, _SAVING_STEPS);
            }
            catch
            {
                return false;
            }
            return true;
        }

        private static bool DeleteFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch(DirectoryNotFoundException)
            {
                return true;
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static void LoadTimetableFromDatabase()
        {
            //todo
        }
    }
}
