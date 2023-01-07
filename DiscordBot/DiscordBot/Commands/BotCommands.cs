using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace DiscordBot.Commands
{
    public class BotCommands : ModuleBase<SocketCommandContext>
    {
        [Command("уровень")]
        public async Task GetMyId()
        {
            var user = Context.User;
            string servername = Context.Guild.Name;
            int level = Convert.ToInt32(DataBase.GetUser(servername, user.Id)[3]);

            await ReplyAsync($"Твой уровень - {level}, не плохо {user.Username}");
        }

        [Command("привет")]
        public async Task SayHello()
        {
            await ReplyAsync("https://tenor.com/view/gifts-gif-19920977");
        }   

        [Command("инфа о")]
        public async Task GetInfoUser(IUser user)
        {
            if (user.IsBot)
            {
                await ReplyAsync("А не пойти ли бы тебе :thinking: ");
                return;
            }

            string activity = string.Empty;

            if (user.Activities.Count == 0)
            {
                activity = "Да ничем эта падаль не занята, вот и все...";
            }
            else
            {
                foreach (var active in user.Activities)
                {
                    if (user.Activities.Count == 1)
                    {
                        activity = $"Зависает в {active}";
                        break;
                    }

                    activity += $"{active}, ";
                }
            }

            await ReplyAsync($"Вот значит как, хочешь знать все о <@{user.Id}>");
            await Task.Delay(5000);
            await ReplyAsync($"Ну значит так....\nДанное существо сейчас в {user.Status}, в народе кличат как {user.Username}, " +
                $"секретный айди - {user.Id} на пару с датой когда начался срач в дискорд - {user.CreatedAt.DateTime.ToShortDateString()}"+
                $"\nИ последнее что я могу поведать, это то, в чем зависает наш юзверь - {activity}");          
        }

        [Command("магический шар")]
        public async Task MagicBall(params string[] args)
        {
            int messageidex = args.Length - 1;
            List<string> answer = new List<string> { "Я так не думаю", "Конечно :sunglasses:", "НЕТ", "ДА", "Отрицаю",
                                                   "Полностью с тобой согласен", "Да что ты говоришь... https://media.tenor.com/ajfPIbGLpa0AAAAM/interesting-charlie-and-the-chocolate-factory.gif",
                                                   ":clown:", "Хм...:thinking:", "https://media.tenor.com/Z0A3V-aC7T8AAAAM/gachi.gif",
                                                   "https://media.tenor.com/WAzoidSydHoAAAAM/%D0%B7%D0%B5%D0%BB%D0%B5%D0%BD%D1%81%D0%BA%D0%B8%D0%B9-%D0%B7%D0%B5.gif",
                                                   "https://media.tenor.com/BSYeNq2POsQAAAAM/gachimuchi.gif", 
                                                   "Пойдем выйдем за такие вопросы... https://media.tenor.com/Dx8XeOcQDpUAAAAM/gachi-gachi-muchi.gif",
                                                   "https://media.tenor.com/zXKPbuZpW0IAAAAM/memeblog-gachi.gif"};
            Random random = new Random();
            int choiceAnswer = random.Next(0, answer.Count);

            if (args[messageidex].EndsWith('?'))
            {
                await ReplyAsync(answer[choiceAnswer]);
            }
            else
            {
                await ReplyAsync("Слушай, я не понял твоего вопроса....повтори если не падла");
            }
        }
    }
}
