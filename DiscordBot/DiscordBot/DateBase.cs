using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;

namespace DiscordBot
{
    class DataBase
    {
        // Путь до папки с базами данных, пока что заглушка ведущая в соседнею папку с исполняймым файлом
        private static readonly string dirName = "Documents";

        /* Метод возвращающий токен для бота, в случае если установка бота была проведенна не коректно,
           он создаст путь и файл в который нужно поместить токен */
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

        // Метод, отвечающий за первичное создание базы данных и её заполнения
        public static void CreateDataBase(string servername, IEnumerable usersId)
        {
            string sql;
            string path = $"{dirName}/{servername}/{servername}.db"; // Создания пути до базы данных, где название соответствует названию сервера

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
        
        // Метод отвечающий за удаление базы данных
        public static void DeleteDB(string servername)
        {
            string path = $"{dirName}/{servername}/{servername}.db";

            FileInfo f = new FileInfo(path);
            f.Delete();
        }

        /* Метод проверяющий сколько пользователю осталось до бана, и если он уже достиг границы бана,
           получает его, в ином случае, увеличивает счетчик */
        public static bool CheckBan(ulong userId, string servername)
        {
            string sql;

            List<ulong> user = GetUser(servername, userId);
            ulong count = user[1];
            count++;

            if (count >= 20)
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

        // Команда SELECT для конекта с базой, возвращает список из всех полей в таблицы соответствующее айди пользователя
        private static List<ulong> SelectCommand(string sql, string servername)
        {
            string path = $"{dirName}/{servername}/{servername}.db";
            List<ulong> output = new List<ulong>(); // выходной список с айдишниками

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

        // Медот с отсальными SQL командами, так как они сами по себе, ничего не возвращают
        private static void SqlCommand(string sql, string servername)
        {
            string path = $"{dirName}/{servername}/{servername}.db";

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

        // Метод добавляющий пользователя 
        public static void AddUser(ulong userId, string servername)
        {
            string sql = $"INSERT INTO users (userId, count_to_ban, count_levelup, level) VALUES ('{userId}', 0, 0, 0)";
            SqlCommand(sql, servername);
        }

        // Метод удаления пользователя
        public static void DeleteUser(ulong userId, string servername)
        {
            string sql = $"DELETE FROM users WHERE userId = '{userId}'";
            SqlCommand(sql, servername);
        }

        // Метод проверящий сколько активному юзеру осталось до повышения уровня, в ином случае просто повышает его поинты
        public static bool LevelUp(ulong userId, string servername)
        {
            string sql;

            List<ulong> user = GetUser(servername, userId);
            ulong count = user[2];
            int level = Convert.ToInt32(user[3]);
            
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

        // Метод дающий список данных о пользователе сервера
        public static List<ulong> GetUser(string servername, ulong userId)
        {
            string sql = $"SELECT userId, count_to_ban, count_levelup, level FROM users WHERE userId = '{userId}'";
            List<ulong> output = SelectCommand(sql, servername);

            return output;
        }

        // Метод отвечающий за получение списка запрещенных слов на данном сервере, сделанна заглушка в виде txt файла
        public static string[] GetBanWords(string servername)
        {
            string path = $"{dirName}/{servername}/{servername}.txt";
            string[] output;

            using (FileStream file = new FileStream(path, FileMode.OpenOrCreate))
            using (StreamReader reader = new StreamReader(file))
            {
                string temp = reader.ReadToEnd();
                string banWord = temp.Replace(" ", ""); 
                output = banWord.Split(',');

                return output;
            }
        }
    }
}
