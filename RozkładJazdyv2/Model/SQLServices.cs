using SQLite.Net;
using SQLite.Net.Async;
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
        private static string _SQLTempFilePath => Path.Combine(ApplicationData.Current.LocalFolder.Path, _SQLTempFileName);

        private static SQLiteAsyncConnection _SQLConnection;

        private static int _SAVING_STEPS = 14;

        private SQLServices() { }

        public static void InitSQL()
            => _SQLConnection = new SQLiteAsyncConnection(new Func<SQLiteConnectionWithLock>(
                () => new SQLiteConnectionWithLock(
                    new SQLitePlatformWinRT(),
                    new SQLiteConnectionString(_SQLFilePath, storeDateTimeAsTicks: false))));

        private static bool IsDatabaseFileExist()
            => File.Exists(_SQLFilePath);

        public static async Task<bool> IsValidDatabaseAsync()
        {
            if (!IsDatabaseFileExist())
                return false;
            string query = "SELECT * FROM sqlite_master WHERE name LIKE 'Line';";
            var result = await _SQLConnection.ExecuteScalarAsync<string>(query);
            bool isValidDatabase = result != null && result.Length > 0;
            return isValidDatabase;
        }

        public static async Task<bool> SaveDatabaseAsync()
        {
            InvokeOnSqlSavingChanged(1, _SAVING_STEPS);
            bool isFileRemoved = DeleteFile(_SQLTempFilePath);
            if (!isFileRemoved)
                return false;
            var tempSqlConnection = new SQLiteAsyncConnection(new Func<SQLiteConnectionWithLock>(
                () => new SQLiteConnectionWithLock(
                    new SQLitePlatformWinRT(),
                    new SQLiteConnectionString(_SQLTempFilePath, storeDateTimeAsTicks: false))));
            if (!(await CreateDatabaseAsync(tempSqlConnection)))
                return false;
            if (!(await InsertIntoDatabaseAsync(tempSqlConnection)))
                return false;
            InvokeOnSqlSavingChanged(14, _SAVING_STEPS);
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
        private static async Task<bool> InsertIntoDatabaseAsync(SQLiteAsyncConnection sqlConnection)
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
                        if (schedule.Tracks != null)
                        {
                            foreach (var track in schedule.Tracks)
                            {
                                listOfTracks.Add(track);
                                foreach (var busStop in track.BusStops)
                                {
                                    listOfBusStops.Add(busStop);
                                    foreach (var hour in busStop.Hours)
                                        listOfHours.Add(hour);
                                }
                            }
                        }
                    }
                }
                InvokeOnSqlSavingChanged(5, _SAVING_STEPS);
                await sqlConnection.InsertAllAsync(Timetable.Instance.BusStopsNames);
                InvokeOnSqlSavingChanged(6, _SAVING_STEPS);
                await sqlConnection.InsertAllAsync(Timetable.Instance.HoursNames);
                InvokeOnSqlSavingChanged(7, _SAVING_STEPS);
                await sqlConnection.InsertAllAsync(Timetable.Instance.Letters);
                InvokeOnSqlSavingChanged(8, _SAVING_STEPS);
                await sqlConnection.InsertAllAsync(Timetable.Instance.Lines);
                InvokeOnSqlSavingChanged(9, _SAVING_STEPS);
                await sqlConnection.InsertAllAsync(Timetable.Instance.TracksNames);
                await sqlConnection.InsertAllAsync(Timetable.Instance.LettersNames);
                InvokeOnSqlSavingChanged(10, _SAVING_STEPS);
                await sqlConnection.InsertAllAsync(listOfSchedules);
                InvokeOnSqlSavingChanged(11, _SAVING_STEPS);
                await sqlConnection.InsertAllAsync(listOfTracks);
                InvokeOnSqlSavingChanged(12, _SAVING_STEPS);
                await sqlConnection.InsertAllAsync(listOfBusStops);
                InvokeOnSqlSavingChanged(13, _SAVING_STEPS);
                await sqlConnection.InsertAllAsync(listOfHours);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<bool> CreateDatabaseAsync(SQLiteAsyncConnection sqlConnection)
        {
            try
            {
                InvokeOnSqlSavingChanged(2, _SAVING_STEPS);
                await sqlConnection.CreateTableAsync<Line>();
                await sqlConnection.CreateTableAsync<BusStopName>();
                await sqlConnection.CreateTableAsync<TrackName>();
                await sqlConnection.CreateTableAsync<HourName>();
                await sqlConnection.CreateTableAsync<Letter>();
                await sqlConnection.CreateTableAsync<Schedule>();
                await sqlConnection.CreateTableAsync<Track>();
                await sqlConnection.CreateTableAsync<BusStop>();
                await sqlConnection.CreateTableAsync<Hour>();
                await sqlConnection.CreateTableAsync<LetterName>();
                InvokeOnSqlSavingChanged(3, _SAVING_STEPS);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool DeleteFile(string path)
        {
            try
            {
                File.Delete(path);
                return true;
            }
            catch(DirectoryNotFoundException)
            {
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void LoadTimetableFromDatabase()
        {
            //todo
        }
    }
}
