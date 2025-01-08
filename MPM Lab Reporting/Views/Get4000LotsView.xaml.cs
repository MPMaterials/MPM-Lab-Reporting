using MPM_Lab_Reporting;
using MPM_Lab_Reporting.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace MPM_Lab_Reporting.Views;

public partial class Get4000LotsView : ContentPage
{
	public Get4000LotsView()
	{
		InitializeComponent();
        var serviceProvider = App.Services;
        if (serviceProvider != null)
        {
            BindingContext = serviceProvider.GetRequiredService<Get4000LotsViewModel>();
        }
        else
        {
            // Handle the case where ServiceProvider is null
            throw new InvalidOperationException("ServiceProvider is not initialized.");
        }
    }
}