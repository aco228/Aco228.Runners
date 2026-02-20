using Aco228.Runners.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Aco228.Runners.Services;

public static class HostedServiceRunner
{
    public static async Task RunAsync(IServiceProvider provider)
    {
        var hostedServices = provider.GetServices<IHostedService>().ToList();

        var cts = new CancellationTokenSource();
        var stopping = 0;

        void RequestStop()
        {
            if (Interlocked.Exchange(ref stopping, 1) == 0)
                cts.Cancel();
        }

        // Observe background crashes
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Console.WriteLine("Unobserved task exception:");
            Console.WriteLine(e.Exception);
        };

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Console.WriteLine("Unhandled exception:");
            Console.WriteLine(e.ExceptionObject);
            RequestStop();
        };

        //  Ctrl+C
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            RequestStop();
        };

        // Docker / SIGTERM / service shutdown
        AppDomain.CurrentDomain.ProcessExit += (_, __) =>
        {
            RequestStop();
        };

        Console.WriteLine($"Starting {hostedServices.Count} hosted services...");

        foreach (var service in hostedServices)
        {
            if (service is HostServiceBase hostServiceBase)
                await hostServiceBase.Initialize();
            
            await service.StartAsync(cts.Token);
        }

        try
        {
            await Task.Delay(Timeout.Infinite, cts.Token);
        }
        catch (TaskCanceledException)
        {
            // expected on shutdown
        }

        Console.WriteLine("Stopping hosted services...");

        foreach (var service in hostedServices.AsEnumerable().Reverse())
        {
            try
            {
                await service.StopAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping service {service.GetType().Name}");
                Console.WriteLine(ex);
            }
        }

        Console.WriteLine("Disposing service provider...");

        if (provider is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();

        Console.WriteLine("Shutdown complete.");
    }
}