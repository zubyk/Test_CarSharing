using CarSharing.Abstractions;
using CarSharing.Data;
using CarSharing.Services;
using CarSharing.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace CarSharing
{
    class Startup
    {
        [STAThread]
        public static void Main()
        {
            var host = Host
                .CreateDefaultBuilder()
                .UseSerilog((context, logConfig) => logConfig.ReadFrom.Configuration(context.Configuration), true, false)
                .ConfigureServices((context, services) =>
                {
                    services.Configure<TimeSignalConfiguration>(context.Configuration.GetSection("Timer"));
                    services.Configure<DriversDataGeneratorConfiguration>(context.Configuration.GetSection("Drivers"));
                    services.Configure<CarsDataGeneratorConfiguration>(context.Configuration.GetSection("Cars"));

                    services.AddSingleton((provider) => provider.GetService<IOptions<TimeSignalConfiguration>>()!.Value);
                    services.AddSingleton((provider) => provider.GetService<IOptions<DriversDataGeneratorConfiguration>>()!.Value);
                    services.AddSingleton((provider) => provider.GetService<IOptions<CarsDataGeneratorConfiguration>>()!.Value);

                    services.AddDbContext<DbDataContext>(options =>
                    {
                        options.UseSqlite(context.Configuration.GetConnectionString("Default"));
                        
                        if (context.HostingEnvironment.IsDevelopment())
                        {
                            options.EnableSensitiveDataLogging(true);
                            options.EnableDetailedErrors(true);
                        }
                    });

                    services.AddSingleton<ITimeSignal, TimerService>();

                    services.AddScoped<DriversDataStorage>();
                    services.AddScoped<CarsDataStorage>();

                    services.AddScoped<DataGeneratorService<DriversDataGeneratorConfiguration, DriversDataStorage>>();
                    services.AddScoped<DataGeneratorService<CarsDataGeneratorConfiguration, CarsDataStorage>>();

                    services.AddScoped(provider =>
                    {
                        return new MainViewModel(
                            provider.GetService<DataGeneratorService<CarsDataGeneratorConfiguration, CarsDataStorage>>()!,
                            provider.GetService<DataGeneratorService<DriversDataGeneratorConfiguration, DriversDataStorage>>()!,
                            provider.GetService<ILogger<MainViewModel>>()!
                        );
                    });

                    services.AddScoped<DbViewModel>();

                    services.AddSingleton<App>();
                })
            .Build();
            
            var app = host.Services.GetService<App>();

            app!.Run();
        }
    }
}
