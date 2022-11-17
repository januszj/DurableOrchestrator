using System;
using System.Reflection;
using DurableOrchestrator;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

[assembly: FunctionsStartup(typeof(Startup))]
namespace DurableOrchestrator;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {

    }

    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
        builder.ConfigurationBuilder
           .SetBasePath(Environment.CurrentDirectory)
           .AddJsonFile("local.settings.json", true)
           .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
           .AddEnvironmentVariables()
           .Build();
    }
}

