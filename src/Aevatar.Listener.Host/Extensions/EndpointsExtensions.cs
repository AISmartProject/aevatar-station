using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aevatar.Listener.SDK.Handler;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Aevatar.Listener.Extensions;

public static class EndpointsExtensions
{
    /// <param name="endpoints">Endpoints </param>
    /// <param name="webhookHandlers">webhookHandlers </param>
    public static void MapWebhookHandlers(this IEndpointRouteBuilder endpoints, IEnumerable<IWebhookHandler> webhookHandlers)
    {
        foreach (var webhook in webhookHandlers)
        {
            switch (webhook.HttpMethod.ToUpperInvariant())
            {
                case "POST":
                    endpoints.MapPost(webhook.Path, async context =>
                    {
                        await ExecuteWebhookHandler(webhook, context);
                    });
                    break;
                case "GET":
                    endpoints.MapGet(webhook.Path, async context =>
                    {
                        await ExecuteWebhookHandler(webhook, context);
                    });
                    break;
                default:
                    throw new NotSupportedException($"HTTP method {webhook.HttpMethod} is not supported for webhook {webhook.Path}");
            }
        }
    }
    
    private static async Task ExecuteWebhookHandler(IWebhookHandler webhook, HttpContext context)
    {
        try
        {
            await webhook.HandleAsync(context.Request);

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync($"Webhook {webhook.Path} successfully processed.");
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync($"Error processing Webhook {webhook.Path}: {ex.Message}");
        }
    }
}