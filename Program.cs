using Telegram.Bot;
using Microsoft.EntityFrameworkCore;
using CaffeBot.Services;
using CaffeBot.Methods;
using System.Security.Claims;
using System.Text;
using NReco.Logging;
using NReco.Logging.File;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews().AddNewtonsoftJson();
builder.Services.AddLogging(i => i.AddFile("log.txt", options => {
    options.Append = false;
    options.MinLevel = LogLevel.Warning;
}));
var botToken = builder.Configuration.GetSection("BotOptions")["BotToken"];
builder.Services.AddHttpClient("tgwebhook")
    .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(botToken, httpClient));

builder.Services.AddTransient<NotifyService>();

var connectionString = builder.Configuration.GetConnectionString("Database");
builder.Services.AddDbContextPool<CaffeBot.ApplicationContext>(options =>
{
    options.UseSqlServer(connectionString);
    
});
builder.Services.AddScoped<DbService>();
builder.Services.AddScoped<CartMethod>();
builder.Services.AddScoped<ContactsMethod>();
builder.Services.AddScoped<MenuMethod>();
builder.Services.AddScoped<ProfileMethod>();
builder.Services.AddScoped<PromoMethod>();
builder.Services.AddScoped<StartMethod>();
builder.Services.AddScoped<NotifyFixalChange>();

bool isOn = Convert.ToBoolean(builder.Configuration.GetSection("TimeService")["IsOn"]);
int min = Convert.ToInt32(builder.Configuration.GetSection("TimeService")["Min"]);
int max = Convert.ToInt32(builder.Configuration.GetSection("TimeService")["Max"]);

builder.Services.AddSingleton<TimeService>(x => new TimeService(isOn, min, max, x.GetService<ITelegramBotClient>()));

builder.Services.AddHostedService<ConfigureWebhook>();
builder.Services.AddScoped<HandleUpdateService>();

builder.Services.AddAuthentication("Cookie").AddCookie("Cookie", options =>
{
    options.LoginPath = "/Login/Auth";
    options.AccessDeniedPath = "/Login/Denied";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Common", adminOptions =>
    {
        adminOptions.RequireAssertion(assert =>
        {
            return assert.User.HasClaim(ClaimTypes.Role, "Common");
        });
    });
    options.AddPolicy("Admin", adminOptions =>
    {
        adminOptions.RequireAssertion(assert =>
        {
            return assert.User.HasClaim(ClaimTypes.Role, "Administrator");
        });
    });
    options.AddPolicy("Head", headOptions =>
    {
        headOptions.RequireAssertion(assert =>
        {
            return assert.User.HasClaim(ClaimTypes.Role, "Head");
        });
    });
    options.AddPolicy("Developer", devOptions =>
    {
        devOptions.RequireAssertion(assert =>
        {
            return assert.User.HasClaim(ClaimTypes.Role, "Developer");
        });
    });
});


//application
var app = builder.Build();

app.UseStaticFiles();

app.UseExceptionHandler("/Home/Error");

app.UseHsts();
app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(name: "tgwebhook",
                                 pattern: $"bot/{botToken}",
                                 new { controller = "Webhook", action = "Post" });
    endpoints.MapControllerRoute(name: "default",
                                 pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();
