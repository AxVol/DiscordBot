using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace DiscordBot
{
    class RandomEventForUser
    {
        /* Этот класс отвечает за эвент для одного случайного пользователя с сервера.
           Смысл этого эвента в том, что бот каждый день выбирает случайного человека и начинает ему докучать разными
           способами, пишет в личку, кидает картинки, постоянно тегает человека и так далее */

        // Список с фразами для спама в личку и сервер чат
        private static readonly List<string> directMessage = new List<string> {"Ты лох", ":clown: :clown: :clown:",
            "Слушай...тут просили передать, что ты пидорок","{user}", ":sunglasses: :sunglasses: :sunglasses:", "Разрешите доебаться?", 
            "https://media.tenor.com/Z0A3V-aC7T8AAAAM/gachi.gif", "https://media.tenor.com/sF0Eyj9Hjx4AAAAM/gachi.gif", 
            "file_lox", "file_sorry", "https://media.tenor.com/ZoulznUJfIUAAAAM/van-darkholme-what.gif",
            "https://media.tenor.com/CtiUeUsRVT8AAAAM/fortnite-%D0%BB%D0%BE%D1%85.gif", "https://media.tenor.com/jL70RMxQlTkAAAAM/mute-kaneki.gif",
            "https://media.tenor.com/PGMRXfJxtAQAAAAM/hi-nobu.gif", "https://media.tenor.com/6BYfHK6j-4oAAAAM/tina-templeton-baby.gif"};

        private static readonly List<string> serverMessage = new List<string> {"{love}", 
            "Зачем такое скидывать в личку? https://media.tenor.com/dI9cw1d3VNgAAAAM/chris-evans-captain-america.gif",
            "Слышь...", "Пошли ебаться) :kissing_heart:", ":clown: :clown: :clown:", "Лошара",
            "Слушай, тут одно тусовка намечается, ты же любишь? https://media.tenor.com/Mb9DB6cucGYAAAAM/gachi.gif",
            "https://media.tenor.com/FiyhE4ITwdkAAAAM/better-ttv-twitch.gif"};

        private static Random random = new Random();

        // Начало эвента для случайного пользователя с сервера
        public static void StartEvent(IReadOnlyCollection<SocketGuild> servers)
        {
            foreach (var server in servers)
            {
                FirstStage(server);
            }
        }

        // Первая его стадия, которая выбирает пользователя и отправляет все нужные данные, в следующие приколы с отдельным юзером
        private async static void FirstStage(SocketGuild server)
        {
            IEnumerable<SocketGuildUser> users = server.Users.Select(user => user);
            int userEvent = random.Next(0, users.Count());
            int counter = 0;

            // перебор всех юзеров с сервера для выбора избранного на сегодняшний день
            foreach (SocketGuildUser user in users)
            {
                if (counter == userEvent)
                {
                    if (user.IsBot)
                    {
                        FirstStage(server);
                        return;
                    }

                    IEnumerable<SocketTextChannel> textChanels = server.TextChannels;

                    foreach (var textChanel in textChanels)
                    {
                        if (textChanel.Name == "general") //Заглушка с текстовыми каналами, название канала должно браться из файла настроек пользователя *исправить*
                        {
                            await textChanel.SendMessageAsync("Ого, сегодня моей целью станет...");
                            await Task.Delay(5000);
                            await textChanel.SendMessageAsync("Кого же выбрать... :thinking:");
                            await Task.Delay(5000);
                            await textChanel.SendMessageAsync($"Привет, <@{user.Id}> сегодня ты мой раб! ♂Dungeon master♂ просил передать тебе привет:)");

                            // Список с действиями бота
                            List<Action> actions = new List<Action>();
                            //actions.Add(() => MessageOnServer(server, user));
                            actions.Add(() => MessageOnDM(server, user));
                            //actions.Add(() => SpamMentioned(server, user));

                            OtherActions(actions, server, user);
                        }
                    }
                }
                counter++;
            }
        }

        // Метод отвечайщий за правильную последовательность действий бота с юзером, разбитом в рандомном порядке
        private async static void OtherActions(List<Action> actions, SocketGuild server, SocketGuildUser user)
        {
            for (int actionCount = 0; actionCount <= 20; actionCount++)
            {
                int actionEvent = 0; //random.Next(0, actions.Count);
                int delayBetweenEvent = random.Next(100000, 200000); // 300000, 1800000 диапазон в милисекундах от 3 минут до 30

                actions[actionEvent]();

                await Task.Delay(delayBetweenEvent);
            }
            Kick(server, user);  //Как на закуску, кик пользователя с сервера, с приглашением его обратно через пару часов
        }

        // Эвенты, их набора - Спам в личку юзера, Спам по нему на сервере, и на конец, кик с сервера
        private async static void MessageOnDM(SocketGuild server, SocketGuildUser user)
        {
            string path;
            string messageForUser;

            for (int i = 0; i <= 5; i++)
            {
                int delay = random.Next(2000, 5000);
                int choice = random.Next(0, directMessage.Count);
                string message = directMessage[choice];

                switch (message)
                {
                    case "file_lox":
                        path = "event/mymindforu.exe";
                        messageForUser = "Впрочем, этот файл говорит все мое мнение о тебе...";
                        await user.SendMessageAsync(messageForUser);
                        await user.SendFileAsync(path, messageForUser);
                        break;
                    case "file_sorry":
                        path = "event/sorry.exe";
                        messageForUser = "Ладно, извини меня что потревожил :cry:";
                        await user.SendMessageAsync(messageForUser);
                        await user.SendFileAsync(path, messageForUser);
                        break;
                    case "{user}":
                        await user.SendMessageAsync(user.Mention);
                        break;
                    default:
                        await user.SendMessageAsync(message);
                        break;
                }
                await Task.Delay(delay);
            }
        }

        private async static void MessageOnServer(SocketGuild server, SocketGuildUser user)
        {
            for (int i = 0; i <= 3; i++)
            {
                int delay = random.Next(1000, 3000);
                int choice = random.Next(0, serverMessage.Count);
                string message = serverMessage[choice];

               foreach (var textChannel in server.TextChannels)
                {
                    if (textChannel.Name == "general") //исправить на добавление из файла
                    {
                        switch (message)
                        {
                            case "{love}":
                                IReadOnlyCollection<SocketGuildUser> users = textChannel.Users;
                                int userChoices = random.Next(0, users.Count);
                                int couter = 0;
                                
                                foreach (SocketGuildUser guildUser in users)
                                {
                                    if (couter == userChoices)
                                    {
                                        await textChannel.SendMessageAsync($"{guildUser.Mention} слушай...тут это, просили передать, что в тебя без ума влюблен {user.Mention}");
                                    }
                                    couter++;
                                } 
                                break;
                            default:
                                await textChannel.SendMessageAsync($"{user.Mention} {message}");
                                break;
                        }
                    }
                }
                await Task.Delay(delay);
            }
        }

        private async static void SpamMentioned(SocketGuild server, SocketGuildUser user)
        {
            foreach (var textChannel in server.TextChannels)
            {
                if (textChannel.Name == "general") //исправить на добавление из файла
                {
                    await textChannel.SendMessageAsync($"{user.Mention}");
                    await Task.Delay(1000);
                    await textChannel.SendMessageAsync($"{user.Mention}");
                    await textChannel.SendMessageAsync($"{user.Mention}");
                    await Task.Delay(3000);
                    await textChannel.SendMessageAsync($"{user.Mention}, {user.Mention}, {user.Mention}, {user.Mention}");
                    await Task.Delay(5000);

                    for (int i = 0; i <= 10; i++)
                    {
                        await textChannel.SendMessageAsync($"{user.Mention}");
                    }

                    await Task.Delay(10000);
                    await textChannel.SendMessageAsync($"хе-хе...:upside_down:\nпрости)");
                }

            }
        }

        private async static void Kick(SocketGuild server, SocketGuildUser user)
        {
            string inviteUrl = string.Empty;

            foreach (var textChannel in server.TextChannels)
            {
                if (textChannel.Name == "general")
                {
                    inviteUrl = textChannel.CreateInviteAsync().ToString();
                }
            }

            await user.KickAsync();
            await Task.Delay(600000);
            await user.SendMessageAsync($"Ладно, ладно, мы тебя все любим <3, возвращайся если ещё не вернулся {inviteUrl}");
        }
    }
}