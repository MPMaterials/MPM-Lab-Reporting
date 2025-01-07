using Microsoft.Maui.Controls;

namespace MPM_Lab_Reporting
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;

        public App(IServiceProvider serviceProvider)
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NMaF5cXmBCfEx1WmFZfVtgc19EZ1ZRQWY/P1ZhSXxWdkRjXH9ecHZQRGRaVkY=");
            InitializeComponent();

            Services = serviceProvider;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Microsoft.Maui.Controls.Window(new AppShell());
        }
    }
}