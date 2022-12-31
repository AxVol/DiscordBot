using System.IO;
using System.Collections;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace DiscordBot
{
    class DateBase
    {
        private static readonly string dirName = "Documents";

        public static string GetToken()
        {
            try
            {
                if (!Directory.Exists(dirName))
                {
                    Directory.CreateDirectory(dirName);
                }

                using (FileStream file = new FileStream("Documents/token.txt", FileMode.OpenOrCreate))
                using (StreamReader stream = new StreamReader(file))
                {
                    string token = stream.ReadToEnd();
                    return token;
                }
            }
            catch
            {
                return "";
            }
        }

        public static void CreateDataBase(string serverName, IEnumerable usersNames)
        {
            string sql;
            string path = $"{dirName}/{serverName}.db";

            if (File.Exists(path))
                return;
                
            var f = File.Create(path);
            f.Close();

            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
            builder.DataSource = path;

            using (SQLiteConnection connection = new SQLiteConnection(builder.ConnectionString))
            {
                connection.Open();

                sql = "CREATE TABLE users (id INTEGER PRIMARY KEY AUTOINCREMENT, nickname TEXT, count_to_ban INTEGER, count_levelup INTEGER, level INTEGER)";

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                   command.ExecuteNonQuery();
                }

                foreach (string userName in usersNames)
                {
                    sql = $"INSERT INTO users (nickname, count_to_ban, count_levelup, level) VALUES ('{userName}', 0, 0, 0)";

                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
