﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServerApp.ChatBot;
using ServerApp.DB;

namespace ServerApp;

/// <summary>
/// Program entry class.
/// </summary>
internal class Program
{
    /// <summary>
    /// Main entry point.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    private static async Task Main(string[] args)
    {
        using var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, configuration) =>
            {
                configuration.Sources.Clear();
                var env = hostingContext.HostingEnvironment;

                // Set DOTNET_ENVIRONMENT environment variable to use particular settings file (via value of env.EnvironmentName).
                configuration
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((hostingContext, services) =>
            {
                services.AddHostedService<TeleBotService>();
                services.AddTransient<IMessageHandler, MessageHandler>();
                services.AddSingleton<IDatabase, Database>();
            })
            .Build();

        await host.RunAsync();
    }
}