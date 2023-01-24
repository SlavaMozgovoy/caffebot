using CaffeBot;
using CaffeBot.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace CaffeBot.Controllers
{
    [Authorize("Admin")]
    public class DishController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<DishController> _logger;
        private readonly IConfiguration _configuration;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _host;
        public DishController(ApplicationContext context, ILogger<DishController> logger, Microsoft.AspNetCore.Hosting.IHostingEnvironment host, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _host = host;
            _configuration = configuration;
        }
        public async Task<IActionResult> Stop(long id)
        {
            var dish = await _context.Dishes.FirstOrDefaultAsync(d => d.DishId == id);
            if (dish != null)
            {
                _logger.LogWarning($"Dish {dish.DishId} is now stopped");
                dish.Available = false;
                await _context.SaveChangesAsync();
                return RedirectToAction("Dishes", "Dish");
            }
            return NotFound();
        }

        [Authorize("Developer")]
        public async Task<IActionResult> AddCategory(string CategoryName)
        {
            Category category = new Category();
            category.Name = CategoryName;
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction("Dishes", "Dish");
        }

        [Authorize("Developer")]
        public async Task<IActionResult> DeleteDish(long Id)
        {
            var dish = _context.Dishes.FirstOrDefault(d => d.DishId == Id);
            _context.Dishes.Remove(dish);
            await _context.SaveChangesAsync();  
            return RedirectToAction("Dishes", "Dish");
        }

        [HttpPost]
        [Authorize("Developer")]
        public async Task<IActionResult> EditDish(long Id, string Name, string Description, decimal Price, long CategoryId, IFormFile File)
        {
            Dish dish = null;
            if (Id == 0)
            {
                dish = new Dish();
            }
            else 
            { 
                dish = await _context.Dishes.FirstOrDefaultAsync(d => d.DishId == Id);
            }
            if (dish != null) { 
                dish.Name = Name;
                dish.Description = Description;
                dish.Price = Price;
                dish.CategoryId = CategoryId;
                if (File != null)
                {
                    string path = "/PicturesNk/" + File.GetHashCode() + ".jpg";
                    using FileStream fs = new FileStream(_host.WebRootPath + path, FileMode.Create);
                    dish.ImgPath = path;
                    File.CopyTo(fs);
                }
                _context.Update(dish);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Dishes", "Dish");
        }

        public async Task<IActionResult> MakeAvailable(long id)
        {
            var dish = await _context.Dishes.FirstOrDefaultAsync(d => d.DishId == id);
            if (dish != null)
            {
                _logger.LogWarning($"Dish {dish.DishId} is now available");
                dish.Available = true;
                await _context.SaveChangesAsync();
                return RedirectToAction("Dishes", "Dish");
            }
            return NotFound();
        }

        public async Task<IActionResult> MakeAllDishesAvailable()
        {
            await _context.Dishes.Where(d => !d.Available).ForEachAsync(d=>d.Available = true);
            await _context.SaveChangesAsync();
            return RedirectToAction("Dishes", "Dish");
        }

        public async Task<IActionResult> Dishes()
        {
            var dishes = await _context.Dishes.Include(d => d.Category).ToListAsync();
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Bonus = _configuration["BonusPercent"];
            return View(dishes);
        }
    }
}
