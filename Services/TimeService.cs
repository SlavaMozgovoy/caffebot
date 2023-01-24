using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;

namespace CaffeBot.Services
{
    public class TimeService
    {
        public bool IsOn { get; set; }  
        public int MinHour { get; set; }    
        public int MaxHour { get; set; }

        private readonly ITelegramBotClient _botClient;
        public TimeService(bool isOn, int minHour, int maxHour, [FromServices] ITelegramBotClient bot)
        {
            IsOn = isOn;
            MinHour = minHour;
            MaxHour = maxHour;
            _botClient = bot;
        }

        public bool IsAllowed(long ChatId, DateTime now)
        {
            if (!IsOn)
                return false;

            int hours = now.Hour;
            bool allowed = !(hours > MinHour && hours < MaxHour);
            if (allowed)
            {
                _botClient.SendTextMessageAsync(ChatId, $"В данный момент кафе не работает. Время работы: с {MinHour} утра до {MaxHour} вечера");
            }
            return allowed;
        }
    }
}
