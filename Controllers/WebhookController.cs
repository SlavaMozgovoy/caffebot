using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using CaffeBot.Services;
namespace CaffeBot.Controllers
{
    public class WebhookController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromServices] HandleUpdateService handleUpdateService,
                                           [FromBody] Update update)
        {
            await handleUpdateService.EchoAsync(update);
            return Ok();
        }
    }
}
