using MPM_Lab_Reporting.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace MPM_Lab_Reporting.Views
{
    public partial class SettingsView : ContentPage
    {
        public SettingsView()
        {
            InitializeComponent();
            var serviceProvider = App.Services;
            if (serviceProvider != null)
            {
                BindingContext = serviceProvider.GetRequiredService<SettingsViewModel>();
            }
            else
            {
                // Handle the case where ServiceProvider is null
                throw new InvalidOperationException("ServiceProvider is not initialized.");
            }
        }
    }
}
