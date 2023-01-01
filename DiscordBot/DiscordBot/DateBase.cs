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

        public static void CreateDataBase(string servername, IEnumerable usersId)
        {
            string sql;
            string path = $"{dirName}/{servername}.db";
                
            FileStream f = File.Create(path);
            f.Close();

            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
            builder.DataSource = path;

            using (SQLiteConnection connection = new SQLiteConnection(builder.ConnectionString))
            {
                connection.Open();

                sql = "CREATE TABLE users (id INTEGER PRIMARY KEY AUTOINCREMENT, userId TEXT, count_to_ban INTEGER, count_levelup INTEGER, level INTEGER)";

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                   command.ExecuteNonQuery();
                }

                foreach (string userId in usersId)
                {
                    sql = $"INSERT INTO users (userId, count_to_ban, count_levelup, level) VALUES ('{userId}', 0, 0, 0)";

                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public static void DeleteDB(string servername)
        {
            string path = $"{dirName}/{servername}.db";

            FileInfo f = new FileInfo(path);
            f.Delete();
        }

        public static bool CheckBan(ulong userId, string servername)
        {
            string sql = $"SELECT userId, count_to_ban, count_levelup, level FROM users WHERE userId = '{userId}'";

            ulong count = SelectCommand(sql, servername)[1];
            count++;

            if (count > 20)
            {
                sql = $"DELETE FROM users WHERE userId = '{userId}'";
                SqlCommand(sql, servername);

                return true;
            }
            else
            {
                sql = $"UPDATE users SET count_to_ban = {count} WHERE userId = '{userId}'";
                SqlCommand(sql, servername);

                return false;
            }
        }

        public static List<ulong> SelectCommand(string sql, string servername)
        {
            string path = $"{dirName}/{servername}.db";
            List<ulong> output = new List<ulong>();

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
                            output.Add(Convert.ToUInt64(reader.GetString(0)));
                            output.Add(Convert.ToUInt64(reader.GetInt32(1)));
                            output.Add(Convert.ToUInt64(reader.GetInt32(2)));
                            output.Add(Convert.ToUInt64(reader.GetInt32(3)));
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

        public static void AddUser(ulong userId, string servername)
        {
            string sql = $"INSERT INTO users (userId, count_to_ban, count_levelup, level) VALUES ('{userId}', 0, 0, 0)";
            SqlCommand(sql, servername);
        }

        public static void DeleteUser(ulong userId, string servername)
        {
            string sql = $"DELETE FROM users WHERE userId = '{userId}'";
            SqlCommand(sql, servername);
        }

        public static bool LevelUp(ulong userId, string servername)
        {
            string sql = $"SELECT userId, count_to_ban, count_levelup, level FROM users WHERE userId = '{userId}'";

            ulong count = SelectCommand(sql, servername)[2];
            int level = Convert.ToInt32(SelectCommand(sql, servername)[3]);
            
            count++;

            if (count > 100)
            {
                level++;

                sql = $"UPDATE users SET count_levelup = 0, level = {level} WHERE userId = '{userId}'";
                SqlCommand(sql, servername);

                return true;
            }
            else
            {
                sql = $"UPDATE users SET count_levelup = {count} WHERE userId = '{userId}'";
                SqlCommand(sql, servername);

                return false;
            }
        }
    }
}
