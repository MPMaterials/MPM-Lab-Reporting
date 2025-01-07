using MPM_Lab_Reporting.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace MPM_Lab_Reporting.Views
{

    public partial class ErrorLogView : ContentPage
    {
        public ErrorLogView()
        {
            InitializeComponent();
            var serviceProvider = App.Services;
            if (serviceProvider != null)
            {
                BindingContext = serviceProvider.GetRequiredService<ErrorLogViewModel>();
            }
            else
            {
                // Handle the case where ServiceProvider is null
                throw new InvalidOperationException("ServiceProvider is not initialized.");
            }
        }
    }
}
