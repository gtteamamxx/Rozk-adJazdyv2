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
        private static readonly string _SQLFavouriteFileName = "TimetableFavourites.sqlite";
        public static string SQLFilePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, _SQLFileName);
        public static string SQLTempFilePath => Path.Combine(ApplicationData.Current.LocalFolder.Path, _SQLTempFileName);
        public static string _SQLFavouriteFilePath => Path.Combine(ApplicationData.Current.LocalFolder.Path, _SQLFavouriteFileName);
        private static SQLiteAsyncConnection _SQLTimetableConnection;
        private static SQLiteAsyncConnection _SQLTimetableFavouriteConnection;

        private static int _SAVING_STEPS = 14;
        private static int _LOADING_STEPS = 6;

        private SQLServices() { }

        public static void InitSQL()
        {
            RenameDownloadedSqlFile();
            CreateFavouriteDatabaseIfNotExist();
            _SQLTimetableConnection = new SQLiteAsyncConnection(new Func<SQLiteConnectionWithLock>(
                   () => new SQLiteConnectionWithLock(new SQLitePlatformWinRT(),
                       new SQLiteConnectionString(SQLFilePath, false))));
            _SQLTimetableFavouriteConnection = new SQLiteAsyncConnection(new Func<SQLiteConnectionWithLock>(
                   () => new SQLiteConnectionWithLock(new SQLitePlatformWinRT(),
                       new SQLiteConnectionString(_SQLFavouriteFilePath, false))));
        }

        private static void RenameDownloadedSqlFile()
        {
            if (!IsTempTimetableDatabaseExist())
                return;
            SQLiteConnection tempConnection = new SQLiteConnection(new SQLitePlatformWinRT(), SQLTempFilePath);
            if (tempConnection == null)
                return;
            if (!IsValidTimetableDatabase(tempConnection))
                return;
            if (IsDatabaseFileExist(SQLFilePath))
                DeleteFile(SQLFilePath);
            RenameTimetableTempFileToMainTimetableFile();
        }

        private static void CreateFavouriteDatabaseIfNotExist()
        {
            using (SQLiteConnection favSqlConnection = new SQLiteConnection(new SQLitePlatformWinRT(), _SQLFavouriteFilePath))
            {
                if(!IsValidTimetableDatabase(favSqlConnection))
                    CreateFavouriteDatabase(favSqlConnection);
                favSqlConnection.Close();
            }
        }

        private static void CreateFavouriteDatabase(SQLiteConnection favSqlConnection)
        {
            favSqlConnection.CreateTable<Line>();
            favSqlConnection.CreateTable<BusStop>();
            favSqlConnection.CreateTable<SQLiteStatus>();
            favSqlConnection.Insert(new SQLiteStatus() { Status = SQLiteStatus.LoadStatus.Succes });
        }

        public static bool IsValidTimetableDatabase(SQLiteConnection sqlConnection = null)
        {
            if (sqlConnection == null)
                sqlConnection = new SQLiteConnection(new SQLitePlatformWinRT(), SQLFilePath);
            if (!IsDatabaseFileExist(sqlConnection.DatabasePath))
                return false;
            string timetableExistString = string.Empty;
            if (string.IsNullOrEmpty((timetableExistString = sqlConnection.ExecuteScalar<string>(
                    "SELECT * FROM sqlite_master WHERE name LIKE 'SQLiteStatus'"))))
                return false;
            var sqLiteStatusTable = sqlConnection.Table<SQLiteStatus>();
            if (sqLiteStatusTable == null || sqLiteStatusTable.Count() == 0)
                return false;
            SQLiteStatus sqliteStatus = sqLiteStatusTable.ElementAt(0);
            bool isTableValid = (sqliteStatus != null && sqliteStatus.Status == SQLiteStatus.LoadStatus.Succes);
            if (!isTableValid)
                return false;
            sqlConnection.Dispose();
            sqlConnection.Close();
            return true;
        }

        public static async Task<bool> SaveTimetableDatabaseAsync()
        {
            InvokeOnSqlSavingChanged(1, _SAVING_STEPS);
            if (IsTempTimetableDatabaseExist())
            {
                bool isFileRemoved = DeleteFile(SQLTempFilePath);
                if (!isFileRemoved)
                    return false;
            }
            SQLiteAsyncConnection tempSqlConnection = new SQLiteAsyncConnection(new Func<SQLiteConnectionWithLock>(
                () => new SQLiteConnectionWithLock(
                    new SQLitePlatformWinRT(),
                    new SQLiteConnectionString(SQLTempFilePath, storeDateTimeAsTicks: false))));
            if (!(await CreateTimetableDatabaseAsync(tempSqlConnection)))
                return false;
            if (!(await InsertIntoTimetableDatabaseAsync(tempSqlConnection)))
                return false;
            InvokeOnSqlSavingChanged(14, _SAVING_STEPS);
            InvokeOnSqlSaved();
            return true;
        }

        private static bool RenameTimetableTempFileToMainTimetableFile()
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

        private static async Task<bool> InsertIntoTimetableDatabaseAsync(SQLiteAsyncConnection sqlConnection)
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
                listOfSchedules = Timetable.Instance.Lines.Where(p => p.Schedules != null).SelectMany(p => p.Schedules).ToList();
                listOfTracks = listOfSchedules.Where(p => p.Tracks != null).SelectMany(p => p.Tracks).ToList();
                listOfBusStops = listOfTracks.SelectMany(p => p.BusStops).ToList();
                listOfHours = listOfBusStops.SelectMany(p => p.Hours).ToList();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<bool> CreateTimetableDatabaseAsync(SQLiteAsyncConnection sqlConnection)
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
            if (_SQLTimetableConnection == null)
                return false;
            try
            {
                Timetable.Instance = new Timetable();
                InvokeOnSqlLoadingChanged(1, _LOADING_STEPS);
                Timetable.Instance.BusStopsNames = await _SQLTimetableConnection.Table<BusStopName>().ToListAsync();
                InvokeOnSqlLoadingChanged(2, _LOADING_STEPS);
                Timetable.Instance.HoursNames = await _SQLTimetableConnection.Table<HourName>().ToListAsync();
                InvokeOnSqlLoadingChanged(3, _LOADING_STEPS);
                Timetable.Instance.LettersNames = await _SQLTimetableConnection.Table<LetterName>().ToListAsync();
                InvokeOnSqlLoadingChanged(4, _LOADING_STEPS);
                Timetable.Instance.TracksNames = await _SQLTimetableConnection.Table<TrackName>().ToListAsync();
                InvokeOnSqlLoadingChanged(5, _LOADING_STEPS);
                Timetable.Instance.Lines = await _SQLTimetableConnection.Table<Line>().ToListAsync();
                InvokeOnSqlLoadingChanged(6, _LOADING_STEPS);
                await UpdateFavourites();
                Timetable.Instance.Letters = new List<Letter>();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static async Task UpdateFavourites()
        {
            var favLines = await _SQLTimetableFavouriteConnection.Table<Line>().ToListAsync();
            foreach (var favLine in favLines)
            {
                var lineToUpdate = Timetable.Instance.Lines.FirstOrDefault(p => p.EditedName == favLine.Name);
                if (lineToUpdate == null)
                    continue;
                lineToUpdate.Type |= Line.FAVOURITE_BIT;
            }
        }

        public static async Task<List<T>> QueryTimetableAsync<T>(string query, params object[] args) where T : class
            => await _SQLTimetableConnection.QueryAsync<T>(query, args);

        public static async Task ExecuteTimetableQueryAsync(string query, params object[] args)
            => await _SQLTimetableConnection.ExecuteAsync(query, args);

        public static async Task<T> ExecuteTimetableScalarAsync<T>(string query, params object[] args) where T : class
            => await _SQLTimetableConnection.ExecuteScalarAsync<T>(query, args);

        public static async Task<List<T>> QueryFavouriteAsync<T>(string query, params object[] args) where T : class
            => await _SQLTimetableFavouriteConnection.QueryAsync<T>(query, args);

        public static async Task ExecuteFavouriteAsync(string query, params object[] args)
            => await _SQLTimetableFavouriteConnection.ExecuteAsync(query, args);

        public static async Task<T> ExecuteFavouriteScalarAsync<T>(string query, params object[] args) where T : class
            => await _SQLTimetableFavouriteConnection.ExecuteScalarAsync<T>(query, args);

        public static async Task InsertFavouriteAsync<T>(T item) where T : class
            => await _SQLTimetableFavouriteConnection.InsertAsync(item);

        private static bool IsDatabaseFileExist(string path)
            => File.Exists(path);

        private static bool IsTempTimetableDatabaseExist()
            => File.Exists(SQLTempFilePath);
    }
}
