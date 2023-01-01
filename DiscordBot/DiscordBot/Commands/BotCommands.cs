using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace DiscordBot.Commands
{
    public class BotCommands : ModuleBase<SocketCommandContext>
    {   
         [Command("уровень")]
        public async Task GetMyId()
        {
            var user = Context.User;
            var servername = Context.Guild.Name;
            string sql = $"SELECT userId, count_to_ban, count_levelup, level FROM users WHERE userId = '{user.Id}'";
            int level = Convert.ToInt32(DataBase.SelectCommand(sql, servername)[3]);

            await ReplyAsync($"Твой уровень - {level}, не плохо {user.Username}");
        }

        [Command("привет")]
        public async Task SayHello()
        {
            await ReplyAsync("https://tenor.com/view/gifts-gif-19920977");
        }
    }
}
