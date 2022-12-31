using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
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

        string servername;
        string username;

        static Task Main() => new Program().MainAsync();

        public async Task MainAsync()
        {
            var config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All
            };

            client = new DiscordSocketClient(config);
            client.MessageReceived += CommandsHandler;
            client.JoinedGuild += JoinHandler;
            client.UserJoined += UserJoinHandler;
            client.UserLeft += UserLeftHandler;
            client.Log += Log;

            string token = DataBase.GetToken();
            commands = new CommandService();
            services = new ServiceCollection().AddSingleton(client).AddSingleton(commands).BuildServiceProvider();

            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            await Task.Delay(-1);
        }

        private async Task UserLeftHandler(SocketGuild guild, SocketUser user)
        {
            servername = guild.Name;
            username = user.Username;

            DataBase.DeleteUser(username, servername);

            await Task.CompletedTask;
        }

        private async Task UserJoinHandler(SocketGuildUser arg)
        {
            servername = arg.Guild.Name;
            username = arg.Username;

            DataBase.AddUser(username, servername);

            await Task.CompletedTask;
        }

        private async Task JoinHandler(SocketGuild arg)
        {
            var guildUsers = arg.GetUsersAsync(RequestOptions.Default);
            var usersName = arg.Users.Select(user => user.Username);

            servername = arg.Name;
            DataBase.CreateDataBase(servername, usersName);

            await Task.CompletedTask;
        }

        private async Task CommandsHandler(SocketMessage arg)
        {
            var msg = arg as SocketUserMessage;

            if (msg == null) return;
            
            var context = new SocketCommandContext(client, msg);
            
            servername = context.Guild.Name;

            List<string> banWord = new List<string>();
            banWord.Add("хуй");
            banWord.Add("пизда");
            banWord.Add("пидор");

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
            else if (banWord.Contains(msg.Content.ToLower()))
            {
                if (DataBase.CheckBan(msg.Author.Username, servername))
                {
                    await context.Guild.AddBanAsync(msg.Author, 1, "Ах ты мелкий черт...");
                }
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());

            return Task.CompletedTask;
        }
    }
}
