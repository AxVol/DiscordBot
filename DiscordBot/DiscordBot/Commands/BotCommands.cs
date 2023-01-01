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
            int level = Convert.ToInt32(DataBase.GetUser(servername, user.Id)[3]);

            await ReplyAsync($"Твой уровень - {level}, не плохо {user.Username}");
        }

        [Command("привет")]
        public async Task SayHello()
        {
            await ReplyAsync("https://tenor.com/view/gifts-gif-19920977");
        }
    }
}
