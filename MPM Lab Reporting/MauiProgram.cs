using Syncfusion.Maui.Core.Hosting;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Identity.Client;
using MPM_Lab_Reporting.Properties;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;
using CommunityToolkit.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using MPM_Lab_Reporting.Helpers;
using MPM_Lab_Reporting.ViewModels;


namespace MPM_Lab_Reporting
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                //.UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                })
                .ConfigureLifecycleEvents(events =>
                {
#if WINDOWS

#endif
                });

            // Register services and view models
            ConfigureServices(builder.Services);


#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.ConfigureSyncfusionCore();
            return builder.Build();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            var tenantId = Settings.TenantID;
            var clientID = Settings.ClientID;
            //services.AddSingleton<MainPageViewModel>();
            services.AddSingleton<IMessenger, WeakReferenceMessenger>();
            services.AddSingleton<Tools>();
            services.AddSingleton<ErrorCatch>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<ErrorLogViewModel>();
            services.AddSingleton<MainPageViewModel>();
            services.AddSingleton<MainPage>();

            // Register IPublicClientApplication
            services.AddSingleton<IPublicClientApplication>(provider =>
                PublicClientApplicationBuilder.Create(clientID)
                    .WithTenantId(tenantId)
                    .WithRedirectUri("http://localhost")
                    .Build());
        }
    }
}
