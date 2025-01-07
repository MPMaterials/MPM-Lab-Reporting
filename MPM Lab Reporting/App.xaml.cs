using Microsoft.Maui.Controls;

namespace MPM_Lab_Reporting
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;

        public App(IServiceProvider serviceProvider)
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzY2MDM4NUAzMjM4MmUzMDJlMzBRK3k2L3ZRVUQzZEZ0MnRpTklKM0c3L2JJVDgwR0F0N0V0Q3ZxejdCTUk4PQ==");
            InitializeComponent();

            Services = serviceProvider;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Microsoft.Maui.Controls.Window(new AppShell());
        }
    }
}