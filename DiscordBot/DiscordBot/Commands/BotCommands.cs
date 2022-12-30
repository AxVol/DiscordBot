using Discord.Commands;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    public class BotCommands : ModuleBase<SocketCommandContext>
    {   
         [Command("мойайди")]
        public async Task GetMyId()
        {
            var user = Context.User;

            await ReplyAsync($"<@{user.Id}>");
            await ReplyAsync($"Твой айди - {user.Id}");
        }

        [Command("привет")]
        public async Task SayHello()
        {
            await ReplyAsync("https://tenor.com/view/gifts-gif-19920977");
        }
    }
}
