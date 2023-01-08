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
        // Параметры бота
        private DiscordSocketClient client;
        private CommandService commands;
        private IServiceProvider services;

        // Переменные для работы с пользователями и серверами
        private string servername;
        private ulong userId;
        public DateTime timeEvent = new DateTime(2023, 01, 04);

        static Task Main() => new Program().MainAsync();

        public async Task MainAsync()
        {
            var config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All
            };

            // Настройки клиента с подключением нужным обработчиков событий
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

        // Обработчик реагирущий на то, когда пользователь покинул сервер и соответственно, удаляет его из базы данных
        private async Task UserLeftHandler(SocketGuild guild, SocketUser user)
        {
            servername = guild.Name;
            userId = user.Id;

            DataBase.DeleteUser(userId, servername);

            await Task.CompletedTask;
        }

        // Обработчик реагирущий на то, когда пользователь присоединился к серверу, добавляя его в базу данных
        private async Task UserJoinHandler(SocketGuildUser arg)
        {
            servername = arg.Guild.Name;
            userId = arg.Id;

            DataBase.AddUser(userId, servername);

            await Task.CompletedTask;
        }

        /* Обработчик, когда бота добавляют на сервер, он создает базу данных для сервера добавляя
           туда всех пользователей которые были на сервере на момент его вступления */
        private async Task JoinHandler(SocketGuild arg)
        {
            var usersId = arg.Users.Select(user => user.Id); // Список айдишников всех юзеров сервера

            servername = arg.Name;
            DataBase.CreateDataBase(servername, usersId);

            await Task.CompletedTask;
        }

        // Обработчик когда бота удаляют с сервера, подчищая любую с ним связь
        private async Task LeftHandler(SocketGuild arg)
        {
            servername = arg.Name;
            DataBase.DeleteDB(servername);

            await Task.CompletedTask;
        }

        // Обработчик основных команд для бота
        private async Task CommandsHandler(SocketMessage arg)
        {
            // создание сообщения и контекста для него
            SocketUserMessage msg = arg as SocketUserMessage;

            if (msg == null) return;
            
            var context = new SocketCommandContext(client, msg);
            
            userId = msg.Author.Id;
            string[] banWord = DataBase.GetBanWords(servername);

            /* Так как бот может писать в личку пользователям, переменая с сервером будет пустовать и выпадать с ошибкой
            при попытке отправить пользователю сообщение */
            try
            {
                servername = context.Guild.Name;
            }
            catch
            {
                if (context.User.IsBot)
                    return;

                await context.Channel.SendMessageAsync("Что ты от меня хочешь? я не буду с тобой теперь общаться...ты злюка, а я бука, фыр");
            }

            // Блокировака для бота на ответ сообщений, своих или же ему подобных 
            if (msg.Author.IsBot)
                return;

            // Проверка для ежедневного эвента всех подключенных серверов
            if (timeEvent.Day < DateTime.Now.Day)
            {
                RandomEventForUser.StartEvent(client.Guilds);
                timeEvent = DateTime.Now;
            }

            // Обработчик команд для бота через "!", сами команды лежат в папке Commands
            int argPos = 0;

            if (msg.HasStringPrefix("!", ref argPos))
            {
                var result = await commands.ExecuteAsync(context, argPos, services);
                
                if (!result.IsSuccess)
                    Console.WriteLine(result.ErrorReason);

                if (result.Error.Equals(CommandError.UnmetPrecondition))
                    await msg.Channel.SendMessageAsync(result.ErrorReason);
            }
            // Проверка разбивающая входящее сообщение на слова и проверяет его совпадение из списка БАН слов
            else if (banWord.Any(word => msg.Content.ToLower().Contains(word)))
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
                foreach (var user in msg.MentionedUsers)
                {
                    if (user.IsBot)
                    {
                        await msg.Channel.SendMessageAsync($"<@{msg.Author.Id}>, Отъебись от меня, я занят спасением этого сервака от гадов...");
                    }
                }
                // Система рангов для активных юзеров
                if (DataBase.LevelUp(userId, servername))
                    await msg.Channel.SendMessageAsync($"Поздравляем {msg.Author.Username} с повышением уровня! *ХЛОП-ХЛОП*");
            }
        }

        /* Метод проверяющий являеться ли пользователь Администратором, чтобы бот пропускал
           мимо них некоторые проверки, таких как блокировака за нарушение правил */        
        public static bool IsAdministrator(ulong userId, IReadOnlyCollection<Discord.WebSocket.SocketRole> roles)
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

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());

            return Task.CompletedTask;
        }
    }
}
