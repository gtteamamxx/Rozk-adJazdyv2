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
    public static class SQLServices
    {
        private static readonly string _SQLFileName = "Timetable.sqlite";
        private static string _SQLFilePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, _SQLFileName);
        private static SQLiteConnection _SQLConnection;

        public static void InitSQL()
            => _SQLConnection = new SQLiteConnection(new SQLitePlatformWinRT(), _SQLFilePath);

        private static bool IsDatabaseFileExist()
            => File.Exists(_SQLFilePath);

        public static bool IsValidDatabase()
        {
            if (!IsDatabaseFileExist())
                return false;

            string query = "SELECT * FROM sqlite_master WHERE name LIKE 'Lines';";
            SQLiteCommand command = _SQLConnection.CreateCommand(query);
            string result = command.ExecuteScalar<string>();
            bool isValidDatabase = result != null && result.Length > 0;
            return isValidDatabase;
        }

        public static void LoadTimetableFromDatabase()
        {
            //todo
        }
    }
}
