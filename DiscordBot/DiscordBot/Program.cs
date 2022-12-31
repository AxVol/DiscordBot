using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot
{
    class Program
    {
        private DiscordSocketClient client;
        private CommandService commands;
        private IServiceProvider services;

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

            string token = DateBase.GetToken();
            commands = new CommandService();
            services = new ServiceCollection().AddSingleton(client).AddSingleton(commands).BuildServiceProvider();

            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            await Task.Delay(-1);
        }

        private async Task CommandsHandler(SocketMessage arg)
        {
            var msg = arg as SocketUserMessage;
            var context = new SocketCommandContext(client, msg);
            var guildUsers = context.Guild.GetUsersAsync(RequestOptions.Default);
            var usersName = context.Guild.Users.Select(user => user.Username);
            string serverName = context.Guild.Name;

            DateBase.CreateDataBase(serverName, usersName);

            if (msg.Author.IsBot)
                return;

            int argPos = 0;

            if (msg.HasStringPrefix("!", ref argPos))
            {
                var result = await commands.ExecuteAsync(context, argPos, services);
                
                if (!result.IsSuccess)
                    Console.WriteLine(result.ErrorReason);

                if (result.Error.Equals(CommandError.UnmetPrecondition))
                    await msg.Channel.SendMessageAsync(result.ErrorReason);
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());

            return Task.CompletedTask;
        }
    }
}
