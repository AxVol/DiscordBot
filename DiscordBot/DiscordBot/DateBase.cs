using System;
using System.IO;

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

        public static void CreateDateBase(string serverName)
        {
            string path = $"{dirName}/{serverName}.db";

            if (File.Exists(path))
                return;
                
            var f = File.Create(path);
            f.Close();
        }
    }
}
