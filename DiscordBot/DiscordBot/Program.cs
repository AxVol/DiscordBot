using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace DiscordBot
{
    class Program
    {
        private DiscordSocketClient client;

        static Task Main() => new Program().MainAsync();

        public async Task MainAsync()
        {
            var config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All
            };

            client = new DiscordSocketClient(config);
            client.MessageReceived += CommandsHandler;
            client.Log += Log;

            string token = "MTA1Nzk4MjM2NzEyMzM5MDQ3NQ.GlsPM8.Ck5hDKptP1vqtZqWbIUSHOQ3X1yg25tIYlKxOM";

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            await Task.Delay(-1);
        }

        private Task CommandsHandler(SocketMessage msg)
        {
            if (!msg.Author.IsBot)
            {
                msg.Channel.SendMessageAsync(msg.Content);
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());

            return Task.CompletedTask;
        }
    }
}
