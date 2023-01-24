using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using CaffeBot.Entities;

namespace CaffeBot.Services
{
    public class DbService
    {
        private readonly ApplicationContext _c;

        public DbService(ApplicationContext c)
        {
            _c = c;
        }
        
        public async Task InitializeProfileAsync(Message Message)
        {
            var chatId = Message.Chat.Id;
            var profile = await GetProfileAsync(chatId);
            if (profile == null)
            {
                string name = $"{(Message.Chat.FirstName ?? string.Empty)} {(Message.Chat.LastName ?? string.Empty)}";
                profile = new Profile()
                {
                    ChatId = chatId,
                    Name = name,
                };
                await _c.Profiles.AddAsync(profile);
                await _c.SaveChangesAsync();
                Cart cart = new Cart()
                {
                    ProfileId = profile.ProfileId,
                };
                await _c.Carts.AddAsync(cart);
                await _c.SaveChangesAsync();
            }
        }
        public async Task<Profile?> GetProfileAsync(long ChatId)
        {
            return await _c.Profiles.FirstOrDefaultAsync(p => p.ChatId == ChatId);
        }

        public async Task ResetIndexesAsync(long ChatId)
        {
            var profile = await GetProfileAsync(ChatId);
            var cart = await GetCartAsync(ChatId);

            profile.PromoIndex = 0;
            cart.Index = 0;

            await _c.SaveChangesAsync();
        }

        public async Task SetProfileSubscribedAsync(long ChatId)
        {
            var profile = await GetProfileAsync(ChatId);
            if (profile == null)
                return;
            profile.Subscribed = true;
            await _c.SaveChangesAsync();
        }

        public async Task SetProfileUnsubscribedAsync(long ChatId)
        {
            var profile = await GetProfileAsync(ChatId);
            if (profile == null)
                return;
            profile.Subscribed = false;
            await _c.SaveChangesAsync();
        }

        public async Task SetProfileAddressAsync(long ChatId, string Address)
        {
            var profile = await GetProfileAsync(ChatId);
            if (profile == null)
                return;
            profile.Address = Address;
            await _c.SaveChangesAsync();  
        }

        public async Task SetProfilePhoneNumberAsync(long ChatId, string PhoneNumber)
        {
            var profile = await GetProfileAsync(ChatId);
            if (profile == null)
                return;
            profile.PhoneNumber = PhoneNumber;
            await _c.SaveChangesAsync();
        }

        public async Task SetProfileStatusAsync(long ChatId, ProfileStatus status)
        {
            var profile = await GetProfileAsync(ChatId);
            if (profile == null)
                return;
            profile.ProfileStatus = status;
            await _c.SaveChangesAsync();
        }
        
        public async Task<Cart?> GetCartAsync(long ChatId)
        {
            var profile = await GetProfileAsync(ChatId);
            if (profile == null)
                return null;
            return await _c.Carts.Include(c=>c.Positions).FirstOrDefaultAsync(c => c.ProfileId == profile.ProfileId && !c.Confirmed);   
        }
        public async Task ClearCartAsync(long ChatId)
        {
            var cart = await GetCartAsync(ChatId);
            cart.Index = 0;

            await _c.Positions.Where(c=>c.CartId == cart.CartId).ForEachAsync(p=>_c.Remove(p));
            await _c.SaveChangesAsync();
        }

        public async Task IncrementCartIndexAsync(long ChatId)
        {
            var cart = await GetCartAsync(ChatId);
            if (cart == null)
            {
                return;
            }
            var countPositions = await _c.Positions.CountAsync(p => p.CartId == cart.CartId);
            cart.Index += cart.Index < countPositions - 1 ? 1 : 0; 

            await _c.SaveChangesAsync();
        }
        public async Task DecrementCartIndexAsync(long ChatId)
        {
            var cart = await GetCartAsync(ChatId);
            if (cart == null)
            {
                return;
            }
        
            cart.Index -= cart.Index > 0 ? 1 : 0;

            await _c.SaveChangesAsync();
        }

        public async Task SetCartDescriptionAsync(long ChatId, string Description)
        {
            var cart = await GetCartAsync(ChatId);
            if (cart == null)
                return;
            cart.Desctiption = Description;
            await _c.SaveChangesAsync();
        }
        public async Task<List<Position>?> GetPositionsAsync(long ChatId)
        {
            var cart = await GetCartAsync(ChatId);
            if (cart == null)
                return null;
            return await _c.Positions.Include(p=>p.Dish).Where(p => p.CartId == cart.CartId).ToListAsync(); 
        }

        public async Task<Category?> GetCategoryAsync(long CategoryId)
        {
            return await _c.Categories.FirstOrDefaultAsync(c => c.CategoryId == CategoryId);
        }
        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _c.Categories.Include(c=>c.Dishes).ToListAsync();
        }

        public async Task<Dish?> GetDishAsync(long DishId)
        {
            return await _c.Dishes.FirstOrDefaultAsync(d => d.DishId == DishId);
        }

        public async Task<List<Dish>?> GetDishesByCategoryAsync(long CategoryId, int @from = 0, int @to = 5)
        {
            var category = await GetCategoryAsync(CategoryId);
            if (category == null)
                return null;

            var items = (await _c.Dishes.Include(d => d.Category)
                    .Where(d => d.CategoryId == CategoryId && d.Available)
                    .ToListAsync());

            to = items.Count - from  >= to ? to : items.Count - from;

            return items.GetRange(@from, to);
        }

        public async Task<int> CountDishesByCategoryAsync(long CategoryId)
        {
            var category = await GetCategoryAsync(CategoryId);
            if (category == null)
                return -1;
            return await _c.Dishes.Where(d => d.CategoryId == category.CategoryId).CountAsync();
        }

        public async Task<List<Position>> GetNotAvailableDishesAsync(long ChatId)
        {
            var cart = await GetCartAsync(ChatId);
            
            var positions = await _c.Positions
                .Include(p => p.Dish)
                .Where(p => p.CartId == cart.CartId && !p.Dish.Available)
                .ToListAsync();

            return positions;
        }
        public async Task<Position?> GetPositionAsync(long PositionId)
        {
            return await _c.Positions.Include(d=> d.Dish).FirstOrDefaultAsync(p => p.PositionId == PositionId);
        }

        public async Task<Position?> GetPositionAsync(long ChatId, long DishId)
        {
            var cart = await GetCartAsync(ChatId);
            if (cart == null)
                return null;
            return await _c.Positions.Include(d => d.Dish).FirstOrDefaultAsync(d => d.CartId == cart.CartId && d.DishId == DishId);
        }
        public async Task ChangeCartIndexAsync(long ChatId)
        {
            var cart = await GetCartAsync(ChatId);
            if (cart.Index >= cart.Positions.Count && cart.Index >= 1)
            {
                cart.Index--;
                await _c.SaveChangesAsync();
            }
        }
        public async Task RemovePositionAsync(long PositionId)
        {
            var position = await GetPositionAsync(PositionId);
            if (position == null)
                return;
            _c.Remove(position);
            await _c.SaveChangesAsync();
        }

        public async Task IncrementPositionAsync(long PositionId)
        {
            var position = await GetPositionAsync(PositionId);
            if (position == null)
                return;
            position.Count++;
            await _c.SaveChangesAsync();
        }

        public async Task DecrementPositionAsync(long PositionId)
        {
            var position = await GetPositionAsync(PositionId);
            if (position == null)
                return;
            position.Count -= position.Count > 0 ? 1 : 0;
            if (position.Count == 0)
            {
                await RemovePositionAsync(PositionId);
            }
            await _c.SaveChangesAsync();
        }

        public async Task IncrementPositionAsync(long ChatId, long DishId)
        {
            var cart = await GetCartAsync(ChatId);
            var dish = await GetDishAsync(DishId);

            var position = await GetPositionAsync(ChatId, DishId);

            if (dish == null || cart == null)
                return;
            if (position == null)
            {
                position = new Position
                {
                    CartId = cart.CartId,
                    DishId = dish.DishId,
                    Count = 1
                };
            }
            else
            {
                position.Count++;
            }
            _c.Positions.Update(position);
            await _c.SaveChangesAsync();
        }

        public async Task DecrementPositionAsync(long ChatId, long DishId)
        {
            var cart = await GetCartAsync(ChatId);
            var dish = await GetDishAsync(DishId);

            var position = await GetPositionAsync(ChatId, DishId);

            if (dish == null || cart == null || position == null)
                return;
            if (position.Count == 1)
            {
                _c.Remove(position);    
            }
            else
            {
                position.Count--;
            }
            await _c.SaveChangesAsync();
        }

        public async Task<List<Promotion>> GetPromotionsAsync()
        {
            return await _c.Promotions.ToListAsync();
        }

        public async Task<int> CountPromotionsAsync()
        {
            return await _c.Promotions.CountAsync();
        } 

        public async Task IncrementPromoIndexAsync(long ChatId)
        {
            var profile = await GetProfileAsync(ChatId);
            if (profile == null)
            {
                return;     
            }
            var countPromotions = await CountPromotionsAsync();

            profile.PromoIndex += profile.PromoIndex < countPromotions - 1 ? 1 : 0;

            await _c.SaveChangesAsync();
        }
        public async Task DecrementPromoIndexAsync(long ChatId)
        {
            var profile = await GetProfileAsync(ChatId);
            if (profile == null)
            {
                return;
            }
            var countPromotions = await CountPromotionsAsync();

            profile.PromoIndex -= profile.PromoIndex >= 0 ? 1 : 0;

            await _c.SaveChangesAsync();
        }
        public async Task<bool> ConfirmOrderAsync(long ChatId, bool bonusPaid)
        {
            var profile = await GetProfileAsync(ChatId);
            var cart = await GetCartAsync(ChatId);
            cart.Confirmed = true;
            if(cart.Positions.Count == 0)
            {
                return false;
            }
            var order = new Order()
            {
                Address = profile.Address,
                PhoneNumber = profile.PhoneNumber,
                ProfileId = profile.ProfileId,
                CartId = cart.CartId,
                OrderTime = DateTime.Now,
                IsPaidByBonuses = bonusPaid,
            };

            var newCart = new Cart()
            {
                ProfileId = profile.ProfileId,
            };

            _c.Carts.Update(newCart);
            _c.Orders.Update(order);
            await _c.SaveChangesAsync();
            return true;
        }
    }
}
