using MPM_Lab_Reporting.ViewModels;
using MPM_Lab_Reporting.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace MPM_Lab_Reporting
{
    [XamlCompilation(XamlCompilationOptions.Compile)]

    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(SettingsView), typeof(SettingsView));
            Routing.RegisterRoute(nameof(ErrorLogView), typeof(ErrorLogView));
            Routing.RegisterRoute(nameof(GridDataView), typeof(GridDataView));


            // Bind the FlyoutBehavior to the MainPageViewModel's FlyoutBehavior property
            this.SetBinding(Shell.FlyoutBehaviorProperty, new Binding("FlyoutBehavior", source: App.Services.GetRequiredService<MainPageViewModel>()));

            // Set the BindingContext for the ImageButton
            var mainPageViewModel = App.Services.GetRequiredService<MainPageViewModel>();
            ErrorLogButton.SetBinding(IsVisibleProperty, new Binding("HasError", source: mainPageViewModel));

            BindingContext = this;

        }
        public Command<string> NavigateCommand => new Command<string>(async (route) =>
        {
            await Shell.Current.GoToAsync(route);
        });
    }
}
