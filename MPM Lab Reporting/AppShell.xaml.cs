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
            Routing.RegisterRoute(nameof(Get4000LotsView), typeof(Get4000LotsView));


            // Bind the FlyoutBehavior to the MainPageViewModel's FlyoutBehavior property
            this.SetBinding(Shell.FlyoutBehaviorProperty, new Binding("FlyoutBehavior", source: App.Services.GetRequiredService<AppShellViewModel>()));

            // Set the BindingContext for the ImageButton
            // Set the BindingContext for the ImageButton to AppShellViewModel
            var appShellViewModel = App.Services.GetRequiredService<AppShellViewModel>();
            ErrorLogButton.SetBinding(IsVisibleProperty, new Binding("HasError", source: appShellViewModel));

            BindingContext = appShellViewModel;

        }

    }
}
