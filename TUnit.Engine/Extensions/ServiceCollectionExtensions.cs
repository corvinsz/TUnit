﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Services;
using TUnit.Engine.Helpers;
using TUnit.Engine.Logging;
using TUnit.Engine.Services;

namespace TUnit.Engine.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFromFrameworkServiceProvider(this IServiceCollection services,
        IServiceProvider serviceProvider, IExtension extension)
    {
        return services
            .AddSingleton(extension)
            .AddTransient(_ => serviceProvider.GetCommandLineOptions());
    }
        
    public static IServiceCollection AddTestEngineServices(this IServiceCollection services, IServiceProvider serviceProvider)
    {
        return services
            .AddSingleton(EngineCancellationToken.CancellationTokenSource)
            .AddSingleton<Disposer>()
            .AddSingleton<StandardOutConsoleInterceptor>()
            .AddSingleton<StandardErrorConsoleInterceptor>()
            .AddSingleton<TestsLoader>()
            .AddSingleton<TestsExecutor>()
            .AddSingleton<TestGrouper>()
            .AddSingleton<SingleTestExecutor>()
            .AddSingleton<TestInvoker>()
            .AddSingleton<TUnitTestDiscoverer>()
            .AddSingleton<TestFilterService>()
            .AddSingleton<ExplicitFilterService>()
            .AddSingleton<TUnitOnEndExecutor>()
            .AddSingleton<TUnitLogger>(sp => ActivatorUtilities.CreateInstance<TUnitLogger>(sp, serviceProvider.GetOutputDevice(), serviceProvider.GetLoggerFactory()))
            .AddSingleton<TUnitInitializer>();
    }
}