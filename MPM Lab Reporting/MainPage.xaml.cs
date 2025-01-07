using MPM_Lab_Reporting.ViewModels;

namespace MPM_Lab_Reporting
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
            var serviceProvider = App.Services;
            if (serviceProvider != null)
            {
                BindingContext = serviceProvider.GetRequiredService<MainPageViewModel>();
            }
            else
            {
                // Handle the case where ServiceProvider is null
                throw new InvalidOperationException("ServiceProvider is not initialized.");
            }
        }
    }

}
