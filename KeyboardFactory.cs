using Telegram.Bot.Types.ReplyMarkups;

namespace CaffeBot
{
    public class KeyboardFactory
    {
        public static ReplyKeyboardRemove GetEmpty()
        {
            var markup = new ReplyKeyboardRemove();
            return markup;
        }
        
        public static ReplyKeyboardMarkup LilKeyboard()
        {
            var markup = new ReplyKeyboardMarkup(new List<KeyboardButton>()
            {
                new KeyboardButton("🏠 Домой"),
                new KeyboardButton("🍽 Меню"),
                new KeyboardButton("🛒 Корзина")
            });
            markup.ResizeKeyboard = true;
            markup.OneTimeKeyboard = false;
            return markup;
        }

        public static ReplyKeyboardMarkup GetStart(bool isNotified, bool isAdm)
        {
            var buttons = new List<List<KeyboardButton>>(){
                new List<KeyboardButton>()
                {
                    new KeyboardButton("🙂 Профиль"),
                    new KeyboardButton("🍽 Меню")
                },
                new List<KeyboardButton>()
                {
                    new KeyboardButton("💵 Акции"),
                    new KeyboardButton("🛒 Корзина")
                },
                new List<KeyboardButton>()
                {
                    new KeyboardButton("📱 Контакты"),
                },
            };

            if (isAdm || isNotified)
            {
                buttons[2].Add(new KeyboardButton("🖥 Панель управления"));
            }


            var markup = new ReplyKeyboardMarkup(buttons);
            markup.ResizeKeyboard= true;
            return markup;
        }
    }
}
