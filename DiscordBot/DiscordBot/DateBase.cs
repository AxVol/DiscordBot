using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;

namespace DiscordBot
{
    class DataBase
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

        public static void CreateDataBase(string servername, IEnumerable usersNames)
        {
            string sql;
            string path = $"{dirName}/{servername}.db";
                
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

        public static bool CheckBan(string username, string servername)
        {
            string sql = $"SELECT nickname, count_to_ban, count_levelup, level FROM users WHERE nickname = '{username}'";

            int count = Convert.ToInt32(SelectCommand(sql, servername)[1]);
            count++;

            if (count > 20)
            {
                sql = $"DELETE FROM users WHERE nickname = '{username}'";
                SqlCommand(sql, servername);

                return true;
            }
            else
            {
                sql = $"UPDATE users SET count_to_ban = {count} WHERE nickname = '{username}'";
                SqlCommand(sql, servername);

                return false;
            }
        }

        private static List<string> SelectCommand(string sql, string servername)
        {
            string path = $"{dirName}/{servername}.db";
            List<string> output = new List<string>();

            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
            builder.DataSource = path;

            using (SQLiteConnection connection = new SQLiteConnection(builder.ConnectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            output.Add(reader.GetString(0));
                            output.Add(reader.GetInt32(1).ToString());
                            output.Add(reader.GetInt32(2).ToString());
                            output.Add(reader.GetInt32(3).ToString());
                        }
                    }
                }
            }

            return output;
        }

        private static void SqlCommand(string sql, string servername)
        {
            string path = $"{dirName}/{servername}.db";

            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
            builder.DataSource = path;

            using (SQLiteConnection connection = new SQLiteConnection(builder.ConnectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void AddUser(string username, string servername)
        {
            string sql = $"INSERT INTO users (nickname, count_to_ban, count_levelup, level) VALUES ('{username}', 0, 0, 0)";
            SqlCommand(sql, servername);
        }

        public static void DeleteUser(string username, string servername)
        {
            string sql = $"DELETE FROM users WHERE nickname = '{username}'";
            SqlCommand(sql, servername);
        }
    }
}
