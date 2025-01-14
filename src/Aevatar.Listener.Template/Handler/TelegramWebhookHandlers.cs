using System.IO;
using System.Threading.Tasks;
using Aevatar.Listener.SDK.Handler;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Aevatar.Listener.Handler;

public class TelegramWebhookHandler : IWebhookHandler
{
    private readonly ILogger<TelegramWebhookHandler> _logger;

    public TelegramWebhookHandler(ILogger<TelegramWebhookHandler> logger)
    {
        _logger = logger;
    }

    public string Path => "/api/webhooks/telegram";
    
    public string HttpMethod => "POST";

    public async Task HandleAsync(HttpRequest request)
    {
        var headers = request.Headers;
        var token = headers["X-Telegram-Bot-Api-Secret-Token"].ToString();
        _logger.LogInformation("Receive update message from telegram.{specificHeader}", token);
        using var reader = new StreamReader(request.Body);
        var bodyString = await reader.ReadToEndAsync();
        _logger.LogInformation("Receive update message from telegram.{message}", bodyString);
    }

   
}