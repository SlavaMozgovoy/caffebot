using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace CaffeBot.Services
{
    public class ConfigureWebhook : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ConfigureWebhook> _logger;
        public ConfigureWebhook(
                                IServiceProvider serviceProvider,
                                IConfiguration configuration,
                                ILogger<ConfigureWebhook> logger)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var botClient = scope.ServiceProvider.GetService<ITelegramBotClient>();
            var botToken = _configuration.GetRequiredSection("BotOptions")["BotToken"];
            string host = Environment.GetEnvironmentVariable("HOST_ADDRESS") ?? _configuration["Host"];
            _logger.LogInformation("Host webhook: " + host);
            var webhookAddress = @$"{host}/bot/{botToken}";
            try { 
                await botClient.SetWebhookAsync
                    (
                        url: webhookAddress,
                        allowedUpdates: new UpdateType[] { UpdateType.Message, UpdateType.CallbackQuery },
                        cancellationToken: cancellationToken
                    );
            }catch( Exception ex )
            {
                await botClient.SendTextMessageAsync(1000055102, "Webhook is dead :(");
                await botClient.SendTextMessageAsync(1012613112, "Webhook is dead :(");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var botClient = scope.ServiceProvider.GetService<ITelegramBotClient>();
            await botClient.DeleteWebhookAsync(cancellationToken: cancellationToken);
        }
    }
}