using MPM_Lab_Reporting;
using MPM_Lab_Reporting.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace MPM_Lab_Reporting.Views
{
    /// <summary>
    /// Interaction logic for GridView.xaml
    /// </summary>
    public partial class GridDataView : ContentPage
    {
        public GridDataView()
        {
            InitializeComponent();
            var serviceProvider = App.Services;
            if (serviceProvider != null)
            {
                BindingContext = serviceProvider.GetRequiredService<GridDataViewModel>();
            }
            else
            {
                // Handle the case where ServiceProvider is null
                throw new InvalidOperationException("ServiceProvider is not initialized.");
            }
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();

            Shell.SetBackButtonBehavior(this, new BackButtonBehavior
            {
                IsVisible = false
            });
        }
    }
}
