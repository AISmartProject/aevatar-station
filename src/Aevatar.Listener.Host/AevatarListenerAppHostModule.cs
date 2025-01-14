using System;
using System.Linq;
using Aevatar.Core;
using Aevatar.Listener.Extensions;
using Aevatar.Listener.SDK.Handler;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace Aevatar.Listener;

[DependsOn(typeof(AbpAutofacModule),
    typeof(AbpAutoMapperModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AevatarListenerTemplatetModule)
)]
public class AevatarListenerHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        ConfigureCors(context, configuration);
        context.Services.AddSingleton<IGAgentFactory,GAgentFactory>();
    }

    private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(
                        configuration["App:CorsOrigins"]
                            .Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(o => o.RemovePostFix("/"))
                            .ToArray()
                    )
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
    }
    
    public void ConfigureServices(IServiceCollection services)
    {
       
    }

    public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
    {

    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var handlers = context.ServiceProvider.GetServices<IWebhookHandler>();
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapWebhookHandlers(handlers);
        });
       
        app.UseCors();
    }
       
    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {

    }
}