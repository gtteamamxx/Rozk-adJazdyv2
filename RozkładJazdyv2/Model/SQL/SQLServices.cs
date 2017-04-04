using SQLite.Net;
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

        private static SQLiteConnection _SQLTimetableConnection;
        private static SQLiteConnection _SQLTimetableFavouriteConnection;

        private static int _SAVING_STEPS = 14;
        private static int _LOADING_STEPS = 6;

        private SQLServices() { }

        public static void InitSQL()
        {
            RenameDownloadedSqlFile();
            CreateFavouriteDatabaseIfNotExist();

            _SQLTimetableConnection = new SQLiteConnection(new SQLitePlatformWinRT(), SQLFilePath);
            _SQLTimetableFavouriteConnection = new SQLiteConnection(new SQLitePlatformWinRT(), _SQLFavouriteFilePath);
        }

        private static void RenameDownloadedSqlFile()
        {
            if (!IsTempTimetableDatabaseExist())
                return;

            SQLiteConnection tempConnection = new SQLiteConnection(new SQLitePlatformWinRT(), SQLTempFilePath);
            if (tempConnection == null || !IsValidTimetableDatabase(tempConnection))
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
            favSqlConnection.CreateTable<BusStopName>();
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

        public static bool SaveTimetableDatabase()
        {
            InvokeOnSqlSavingChanged(1, _SAVING_STEPS);
            if (IsTempTimetableDatabaseExist())
            {
                bool isFileRemoved = DeleteFile(SQLTempFilePath);
                if (!isFileRemoved)
                    return false;
            }

            SQLiteConnection tempSqlConnection = new SQLiteConnection(new SQLitePlatformWinRT(), SQLTempFilePath);

            if (!CreateTimetableDatabase(tempSqlConnection))
                return false;
            if (!InsertIntoTimetableDatabase(tempSqlConnection))
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

        private static bool InsertIntoTimetableDatabase(SQLiteConnection sqlConnection)
        {
            try
            {
                List<Schedule> listOfSchedules = new List<Schedule>();
                List<Track> listOfTracks = new List<Track>();
                List<BusStop> listOfBusStops = new List<BusStop>();
                List<Hour> listOfHours = new List<Hour>();

                InvokeOnSqlSavingChanged(4, _SAVING_STEPS);
                bool areListGot = GetTimetableListsFromInstance(listOfSchedules, listOfTracks,
                                                listOfBusStops, listOfHours);
                if (!areListGot)
                    return false;

                new Task(() =>
                {
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
                    sqlConnection.InsertAll(Timetable.Instance.LettersNames);

                    InvokeOnSqlSavingChanged(10, _SAVING_STEPS);
                    sqlConnection.InsertAll(listOfSchedules);

                    InvokeOnSqlSavingChanged(11, _SAVING_STEPS);
                    sqlConnection.InsertAll(listOfTracks);

                    InvokeOnSqlSavingChanged(12, _SAVING_STEPS);
                    sqlConnection.InsertAll(listOfBusStops);

                    InvokeOnSqlSavingChanged(13, _SAVING_STEPS);
                    sqlConnection.InsertAll(listOfHours);
                    sqlConnection.Insert(new SQLiteStatus() { Status = SQLiteStatus.LoadStatus.Succes });
                });

                return true;
            }
            catch
            {
                return false;
            }
        }
        private static bool GetTimetableListsFromInstance(List<Schedule> listOfSchedules, List<Track> listOfTracks,
                                                                    List<BusStop> listOfBusStops, List<Hour> listOfHours)
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

        private static bool CreateTimetableDatabase(SQLiteConnection sqlConnection)
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
                sqlConnection.CreateTable<LetterName>();
                sqlConnection.CreateTable<SQLiteStatus>();
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
                Task task = new Task(() =>
                {
                    Timetable.Instance = new Timetable();
                    InvokeOnSqlLoadingChanged(1, _LOADING_STEPS);
                    Timetable.Instance.BusStopsNames = _SQLTimetableConnection.Table<BusStopName>().ToList();

                    InvokeOnSqlLoadingChanged(2, _LOADING_STEPS);
                    Timetable.Instance.HoursNames = _SQLTimetableConnection.Table<HourName>().ToList();

                    InvokeOnSqlLoadingChanged(3, _LOADING_STEPS);
                    Timetable.Instance.LettersNames = _SQLTimetableConnection.Table<LetterName>().ToList();

                    InvokeOnSqlLoadingChanged(4, _LOADING_STEPS);
                    Timetable.Instance.TracksNames = _SQLTimetableConnection.Table<TrackName>().ToList();

                    InvokeOnSqlLoadingChanged(5, _LOADING_STEPS);
                    Timetable.Instance.Lines = _SQLTimetableConnection.Table<Line>().ToList();

                    InvokeOnSqlLoadingChanged(6, _LOADING_STEPS);

                    Timetable.Instance.Letters = new List<Letter>();
                    UpdateFavourites();
                });
                task.Start();
                await task.AsAsyncAction();

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void UpdateFavourites()
        {
            UpdateFavouriteLines();

            void UpdateFavouriteLines()
            {
                var favLines = _SQLTimetableFavouriteConnection.Table<Line>().ToList();
                foreach (var favLine in favLines)
                {
                    var lineToUpdate = Timetable.Instance.Lines.FirstOrDefault(p => p.EditedName == favLine.Name);
                    if (lineToUpdate == null)
                        continue;

                    lineToUpdate.Type |= Line.FAVOURITE_BIT;
                }
            }
        }

        public static List<T> QueryTimetable<T>(string query, params object[] args) where T : class
            =>  _SQLTimetableConnection.Query<T>(query, args);

        public static void ExecuteTimetableQuery(string query, params object[] args)
            =>  _SQLTimetableConnection.Execute(query, args);

        public static T ExecuteTimetableScalar<T>(string query, params object[] args) where T : class
            =>  _SQLTimetableConnection.ExecuteScalar<T>(query, args);

        public static List<T> QueryFavourite<T>(string query, params object[] args) where T : class
            =>  _SQLTimetableFavouriteConnection.Query<T>(query, args);
        
        public static void ExecuteFavourite(string query, params object[] args)
            =>  _SQLTimetableFavouriteConnection.Execute(query, args);

        public static T ExecuteFavouriteScalar<T>(string query, params object[] args) where T : class
            =>  _SQLTimetableFavouriteConnection.ExecuteScalar<T>(query, args);

        public static List<T> TableFavourite<T>() where T : class
            =>  _SQLTimetableFavouriteConnection.Table<T>().ToList();

        private static bool IsDatabaseFileExist(string path)
            => File.Exists(path);

        private static bool IsTempTimetableDatabaseExist()
            => File.Exists(SQLTempFilePath);
    }
}
