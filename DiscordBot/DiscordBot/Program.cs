using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Runtime;
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
        ulong userId;

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
            client.LeftGuild += LeftHandler;
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
            userId = user.Id;

            DataBase.DeleteUser(userId, servername);

            await Task.CompletedTask;
        }

        private async Task UserJoinHandler(SocketGuildUser arg)
        {
            servername = arg.Guild.Name;
            userId = arg.Id;

            DataBase.AddUser(userId, servername);

            await Task.CompletedTask;
        }

        private async Task JoinHandler(SocketGuild arg)
        {
            var guildUsers = arg.GetUsersAsync(RequestOptions.Default);
            var usersId = arg.Users.Select(user => user.Id);

            servername = arg.Name;
            DataBase.CreateDataBase(servername, usersId);

            await Task.CompletedTask;
        }

        private async Task LeftHandler(SocketGuild arg)
        {
            servername = arg.Name;
            DataBase.DeleteDB(servername);

            await Task.CompletedTask;
        }

        private async Task CommandsHandler(SocketMessage arg)
        {
            var msg = arg as SocketUserMessage;

            if (msg == null) return;
            
            var context = new SocketCommandContext(client, msg);
            
            servername = context.Guild.Name;
            userId = msg.Author.Id;

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
                var roles = context.Guild.Roles;

                if (!IsAdministrator(userId, roles))
                {
                    if (DataBase.CheckBan(userId, servername))
                    {
                        await context.Guild.AddBanAsync(msg.Author, 1, "Ах ты мелкий черт...");
                    }
                }
            }
            else
            {
                if (DataBase.LevelUp(userId, servername))
                    await msg.Channel.SendMessageAsync($"Поздравляем {msg.Author.Username} с повышением уровня! *ХЛОП-ХЛОП*");
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());

            return Task.CompletedTask;
        }

        private bool IsAdministrator(ulong userId, IReadOnlyCollection<Discord.WebSocket.SocketRole> roles)
        {
            foreach (var role in roles)
            {
                if (role.Permissions.Administrator)
                {
                    var userRole = role.Members;
                    foreach (var user in userRole)
                    {
                        if (user.Id == userId) return true;
                    }
                }
            }
            return false;
        }
    }
}
