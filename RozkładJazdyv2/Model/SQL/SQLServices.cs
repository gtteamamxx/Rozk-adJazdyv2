using SQLite.Net;
using SQLite.Net.Async;
using SQLite.Net.Platform.WinRT;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace RozkładJazdyv2.Model
{
    public class SQLServices : Timetable
    {
        private static readonly string _SQLFileName = "Timetable.sqlite";
        private static readonly string _SQLTempFileName = "Timetable_T.sqlite";

        public static string SQLFilePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, _SQLFileName);
        public static string SQLTempFilePath => Path.Combine(ApplicationData.Current.LocalFolder.Path, _SQLTempFileName);

        private static SQLiteAsyncConnection _SQLConnection;

        private static int _SAVING_STEPS = 14;
        private static int _LOADING_STEPS = 6;

        private SQLServices() { }

        public static void InitSQL()
        {
            RenameDownloadedSqlFile();
            _SQLConnection = new SQLiteAsyncConnection(new Func<SQLiteConnectionWithLock>(
                   () => new SQLiteConnectionWithLock(new SQLitePlatformWinRT(),
                       new SQLiteConnectionString(SQLFilePath, storeDateTimeAsTicks: false))));
        }

        private static void RenameDownloadedSqlFile()
        {
            using (SQLiteConnection tempConnection = new SQLiteConnection(new SQLitePlatformWinRT(), SQLTempFilePath))
            {
                if (tempConnection == null)
                    return;
                string timetableExistString = string.Empty;
                if (string.IsNullOrEmpty((timetableExistString = tempConnection.ExecuteScalar<string>(
                        "SELECT * FROM sqlite_master WHERE name LIKE 'SQLiteStatus'"))))
                    return;
                SQLiteStatus sqliteStatus = tempConnection.Table<SQLiteStatus>().ElementAt(0);
                bool isTableValid = (sqliteStatus != null && sqliteStatus.Status == SQLiteStatus.LoadStatus.Succes);
                if (!isTableValid)
                    return;
                tempConnection.Close();
            }
            RenameTempFileToMainFile();
        }

        public static async Task<bool> IsValidDatabaseAsync()
        {
            if (!IsDatabaseFileExist())
                return false;
            string query = "SELECT * FROM sqlite_master WHERE name LIKE 'Line';";
            string result = await _SQLConnection.ExecuteScalarAsync<string>(query);
            return result != null && result.Length > 0;
        }

        public static async Task<bool> SaveDatabaseAsync()
        {
            InvokeOnSqlSavingChanged(1, _SAVING_STEPS);
            bool isFileRemoved = DeleteFile(SQLTempFilePath);
            if (!isFileRemoved)
                return false;
            SQLiteAsyncConnection tempSqlConnection = new SQLiteAsyncConnection(new Func<SQLiteConnectionWithLock>(
                () => new SQLiteConnectionWithLock(
                    new SQLitePlatformWinRT(),
                    new SQLiteConnectionString(SQLTempFilePath, storeDateTimeAsTicks: false))));
            if (!(await CreateDatabaseAsync(tempSqlConnection)))
                return false;
            if (!(await InsertIntoDatabaseAsync(tempSqlConnection)))
                return false;
            InvokeOnSqlSavingChanged(14, _SAVING_STEPS);
            isFileRemoved = DeleteFile(SQLFilePath);
            if (!isFileRemoved)
                return false;
            InvokeOnSqlSaved();
            return true;
        }

        private static bool RenameTempFileToMainFile()
        {
            try
            {
                File.Move(SQLTempFilePath, SQLFilePath);
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
                bool areListGot = GetTimetableListsFromInstance(ref listOfSchedules, ref listOfTracks,
                                                ref listOfBusStops, ref listOfHours);
                if (!areListGot)
                    return false;
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
                await sqlConnection.InsertAsync(new SQLiteStatus() { Status = SQLiteStatus.LoadStatus.Succes });
                return true;
            }
            catch
            {
                return false;
            }
        }
        private static bool GetTimetableListsFromInstance(ref List<Schedule> listOfSchedules, ref List<Track> listOfTracks,
                                                                    ref List<BusStop> listOfBusStops, ref List<Hour> listOfHours)
        {
            try
            {
                listOfSchedules = Timetable.Instance.Lines.SelectMany(p => p.Schedules).ToList();
                listOfTracks = listOfSchedules.SelectMany(p => p.Tracks).ToList();
                listOfBusStops = listOfTracks.SelectMany(p => p.BusStops).ToList();
                listOfHours = listOfBusStops.SelectMany(p => p.Hours).ToList();
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
                await sqlConnection.CreateTableAsync<SQLiteStatus>();
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
            catch (DirectoryNotFoundException)
            {
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> LoadTimetableFromDatabase()
        {
            if (_SQLConnection == null)
                return false;
            try
            {
                Timetable.Instance = new Timetable();
                InvokeOnSqlLoadingChanged(1, _LOADING_STEPS);
                Timetable.Instance.BusStopsNames = await _SQLConnection.Table<BusStopName>().ToListAsync();
                InvokeOnSqlLoadingChanged(2, _LOADING_STEPS);
                Timetable.Instance.HoursNames = await _SQLConnection.Table<HourName>().ToListAsync();
                InvokeOnSqlLoadingChanged(3, _LOADING_STEPS);
                Timetable.Instance.LettersNames = await _SQLConnection.Table<LetterName>().ToListAsync();
                InvokeOnSqlLoadingChanged(4, _LOADING_STEPS);
                Timetable.Instance.TracksNames = await _SQLConnection.Table<TrackName>().ToListAsync();
                InvokeOnSqlLoadingChanged(5, _LOADING_STEPS);
                Timetable.Instance.Lines = await _SQLConnection.Table<Line>().ToListAsync();
                InvokeOnSqlLoadingChanged(6, _LOADING_STEPS);
                Timetable.Instance.Letters = new List<Letter>();
                return true;
            }   
            catch
            {
                return false;
            }
        }

        private static bool IsDatabaseFileExist()
            => File.Exists(SQLFilePath);

        private static bool IsTempDatabaseExist()
            => File.Exists(SQLTempFilePath);
    }
}
