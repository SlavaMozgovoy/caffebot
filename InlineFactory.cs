using CaffeBot.Entities;
using Telegram.Bot.Types.ReplyMarkups;

namespace CaffeBot
{
    public static class InlineFactory
    {
        public static readonly string AddData = "/cart?add?";
        public static readonly string DeleteData = "/cart?delete?";
        public static readonly string CountData = "/count?";
        public static readonly string TotalPrice = "/totalPrice?";

        public static readonly string RemovePosition = "/position?remove?";
        //CART
        public static readonly string IncrementPosition = "/position?add?"; //id
        public static readonly string DecrementPosition = "/position?delete?"; //id
        public static readonly string PositionPrevious = "/position?previous?";
        public static readonly string PositionNext = "/position?next?";
        public static readonly string ConfirmData = "/menuCart?confirm?";
        public static readonly string ClearData = "/menuCart?clear?";

        public static readonly string PromoNext = "/promo?next";
        public static readonly string PromoPrevious = "/promo?previous";

        public static readonly string OrderDescription = "/order?desctption";
        public static readonly string OrderConfirm = "/order?confirm";
        public static readonly string OrderBonusConfirm = "/order?bonusConfirm";
        public static InlineKeyboardMarkup GetConfirmMarkup(bool bonuses = false)
        {
            InlineKeyboardMarkup markup;
            if (bonuses)
            {
                markup = new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>()
                {
                    new List<InlineKeyboardButton>(){
                        new InlineKeyboardButton("Добавить примечание"){CallbackData = OrderDescription},
                        new InlineKeyboardButton("Подтвердить заказ"){CallbackData = OrderConfirm},
                    },
                     new List<InlineKeyboardButton>(){
                        new InlineKeyboardButton("Оплатить бонусами"){CallbackData=OrderBonusConfirm},
                    },
                });
              
            }
            else
            {
                markup = new InlineKeyboardMarkup(new List<InlineKeyboardButton>()
                {
                    new InlineKeyboardButton("Добавить примечание"){CallbackData = OrderDescription},
                    new InlineKeyboardButton("Подтвердить заказ"){CallbackData = OrderConfirm}
                });
            }
            
            return markup;
        }

        public static InlineKeyboardMarkup GetNotifyMarkup()
        {
            var markup = new InlineKeyboardMarkup(new InlineKeyboardButton("Панель управления")
            {
                Url = "https://nashecaffe-bot.site/"
            });
            return markup;
        }
        public static InlineKeyboardMarkup GetCartMarkup(long chatId, ICollection<Position> positions, int index)
        {
            var position = positions.ToList()[index];
            var markup = new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>()
            {
                new List<InlineKeyboardButton>()
                {
                    new InlineKeyboardButton("❌"){CallbackData = RemovePosition + position.PositionId},
                    new InlineKeyboardButton("-"){CallbackData = DecrementPosition + position.PositionId},
                    new InlineKeyboardButton(position.Count.ToString()){CallbackData = "/empty"},
                    new InlineKeyboardButton("+"){CallbackData = IncrementPosition + position.PositionId}
                },
                new List<InlineKeyboardButton>()
                {
                    new InlineKeyboardButton("⇐"){CallbackData = PositionPrevious},
                    new InlineKeyboardButton($"{index + 1}/{positions.Count}"){CallbackData = "/empty"},
                    new InlineKeyboardButton("⇒"){CallbackData = PositionNext}
                },
                new List<InlineKeyboardButton>()
                {
                    new InlineKeyboardButton("Очистить корзину"){CallbackData = ClearData + chatId }
                },
                new List<InlineKeyboardButton>()
                {
                    new InlineKeyboardButton($"Проверить заказ - {positions.Sum(i=>i.Count * i.Dish.Price).ToString()}₽"){CallbackData = ConfirmData + chatId }
                }
            });
            return markup;
        }

        public static InlineKeyboardMarkup GetDishCountMarkup(long dishId, int count, decimal price)
        {
            return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>()
            {
                new List<InlineKeyboardButton>(){
                    new InlineKeyboardButton("-")
                    {
                        CallbackData = DeleteData + dishId,
                    },
                    new InlineKeyboardButton(String.Format("{0} шт.", count))
                    {
                        CallbackData = "/empty"
                    },
                    new InlineKeyboardButton("+")
                    {
                        CallbackData = AddData + dishId,
                    }
                },
                new List<InlineKeyboardButton>()
                {
                    new InlineKeyboardButton(String.Format("{0} ₽", price))
                    {
                        CallbackData = "/empty"
                    }
                }
            });
        }

        public static InlineKeyboardMarkup GetProfile()
        {
            var markup = new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>()
            {
                new List<InlineKeyboardButton>(){
                    new InlineKeyboardButton("1. Изменить номер")
                    {
                        CallbackData = "/phoneNumber"
                    }
                },
                new List<InlineKeyboardButton>(){
                    new InlineKeyboardButton("2. Изменить адрес")
                    {
                        CallbackData = "/address"
                    }
                }
            });

            return markup;
        }

        public static InlineKeyboardMarkup GetMenuNext(long categoryId, int @from, int @to, int total)
        {
            var markup = new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>()
            {
                new List<InlineKeyboardButton>(){
                    new InlineKeyboardButton($"Следующие блюда {@from + 1} -  {from + to} из {total}")
                    {
                         CallbackData = categoryId.ToString() + "?" + from + "?" + to
                    }
                },
            });

            return markup;
        }


        public static InlineKeyboardMarkup GetMenu(List<CaffeBot.Entities.Category> categories)
        {
            List<List<InlineKeyboardButton>> btns = new List<List<InlineKeyboardButton>>();
            //int i = 0;

            foreach (var category in categories)
            {
                List<InlineKeyboardButton> buttons = new List<InlineKeyboardButton>();
                buttons.Add(new InlineKeyboardButton(category.Name)
                {
                    CallbackData = category.CategoryId.ToString() + "?" + "0" + "?" + "5"
                });
                btns.Add(buttons);
            }

            // Размещаем кнопки по три штуки в одном ряду
           /* foreach (var category in categories)
            {
                // Добавляем новый ряд
                if (i++ % 3 == 0)
                {
                    btns.Add(new List<InlineKeyboardButton>());
                }
                // Добавляем кнопку категории
                btns[btns.Count - 1].Add(new InlineKeyboardButton(category.Name)
                {
                    CallbackData = category.CategoryId.ToString(),
                });

            }*/
            return new InlineKeyboardMarkup(btns);
        }

        public static InlineKeyboardMarkup GetContacts()
        {
            InlineKeyboardMarkup markup = new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>()
            {
                new List<InlineKeyboardButton>()
                {
                    new InlineKeyboardButton("Лугаком")
                    {
                        CallbackData = "/lugacom"
                    },
                    new InlineKeyboardButton("МТС")
                    {
                        CallbackData = "/vodafone"
                    }
                },
                new List<InlineKeyboardButton>()
                {
                    new InlineKeyboardButton("ВК")
                    {
                        CallbackData = "/vkurl",
                        Url = "https://vk.com/club103440750"
                    },
                    new InlineKeyboardButton("Инстаграм")
                    {
                        CallbackData = "/insturl",
                        Url = "https://www.instagram.com/nashe_cafe_bulvar/"
                    }
                },
                new List<InlineKeyboardButton>()
                {
                    new InlineKeyboardButton("Телеграм")
                    {
                        CallbackData = "/tgurl",
                        Url = "https://t.me/nashebulvar"
                    },
                    new InlineKeyboardButton("Наш сайт")
                    {
                        CallbackData = "/siteurl",
                        Url = "http://xn----7sbbsc2ar5d5a.com/"
                    }
                }
            });
            return markup;
        }

        public static InlineKeyboardMarkup GetPromo(int index, int all)
        {
            InlineKeyboardMarkup markup = new InlineKeyboardMarkup(new List<InlineKeyboardButton>()
            {
                new InlineKeyboardButton("⇐"){CallbackData = PromoPrevious},
                new InlineKeyboardButton($"{index + 1}/{all}"){CallbackData = "/Empty"},
                new InlineKeyboardButton("⇒"){CallbackData = PromoNext},
            });
            return markup;
        }
    }
}
