using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace DiscordBot
{
    class RandomEventForUser
    {
        /* Этот класс отвечает за эвент для одного случайного пользователя с сервера.
           Смысл этого эвента в том, что бот каждый день выбирает случайного человека и начинает ему докучать разными
           способами, пишет в личку, кидает картинки, постоянно тегает человека и так далее */

        // Начало эвента для случайного пользователя с сервера
        public static void StartEvent(IReadOnlyCollection<SocketGuild> servers)
        {
            foreach (var server in servers)
            {
                FirstStage(server);
            }
        }

        private async static void FirstStage(SocketGuild server)
        {
            IEnumerable<SocketGuildUser> users = server.Users.Select(user => user);
            Random random = new Random();
            int userEvent = random.Next(0, users.Count() + 1);
            int counter = 0;

            foreach (SocketGuildUser user in users)
            {
                if (counter == userEvent)
                {
                    if (user.IsBot)
                    {
                        userEvent = random.Next(0, users.Count() + 1);
                        counter = 0;

                        continue;
                    }

                    var textChanels = server.TextChannels;

                    //Заглушка с текстовыми каналами, название канала должно браться из файла настроек пользователя *исправить*
                    foreach (var textChanel in textChanels)
                    {
                        if (textChanel.Name == "general")
                        {
                            await textChanel.SendMessageAsync("Ого, сегодня моей целью станет...");

                            await Task.Delay(5000);

                            await textChanel.SendMessageAsync("Кого же выбрать... :thinking:");

                            await Task.Delay(5000);

                            await textChanel.SendMessageAsync($"Привет, <@{user.Id}> сегодня ты мой раб! ♂Dungeon master♂ просил передать тебе привет:)");

                            // Список с действиями бота
                            List<Action> actions = new List<Action>();
                            actions.Add(() => MessageOnServer(server, user));
                            actions.Add(() => MessageOnDM(server, user));
                            actions.Add(() => Kick(server, user));
                            actions.Add(() => SpamMentioned(server, user));

                            OtherActions(actions);
                        }
                    }
                }
                counter++;
            }
        }

        private async static void OtherActions(List<Action> actions)
        {
            for (int i = 0; i < 10; i++)
            {
                Random random = new Random();
                int actionEvent = random.Next(0, actions.Count + 1);
                int delayBetweenEvent = random.Next(300000, 3600000);

                actions[actionEvent]();

                await Task.Delay(delayBetweenEvent);
            }
        }

        private async static void MessageOnDM(SocketGuild server, SocketGuildUser user)
        {
            
        }

        private async static void MessageOnServer(SocketGuild server, SocketGuildUser user)
        {
            throw new NotImplementedException();
        }

        private async static void Kick(SocketGuild server, SocketGuildUser user)
        {
            throw new NotImplementedException();
        }

        private async static void SpamMentioned(SocketGuild server, SocketGuildUser user)
        {
            throw new NotImplementedException();
        }
    }
}