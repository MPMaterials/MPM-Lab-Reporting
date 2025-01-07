using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using MPM_Lab_Reporting.Properties;

namespace MPM_Lab_Reporting.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private ICommand? _saveSettings;

        private string _sqlServer;
        private string _sqldbName;

        public SettingsViewModel()
        {
            _sqlServer = Settings.SqlSvr;
            _sqldbName = Settings.SqlDB;
        }

        public string SqlServer
        {
            get => _sqlServer;
            set
            {
                SetProperty(ref _sqlServer, value);
            }
        }

        public string SqlDbName
        {
            get => _sqldbName;
            set
            {
                SetProperty(ref _sqldbName, value);
            }
        }

        public ICommand SaveSettingsCommand => _saveSettings ??= new RelayCommand(SaveSettingsClick);

        private void SaveSettingsClick()
        {
            Settings.SqlSvr = SqlServer;
            Settings.SqlDB = SqlDbName;

            // Restart the application
            var currentApp = Application.Current;
            var mainPage = currentApp?.Windows[0].Page;
            if (mainPage != null && currentApp != null && currentApp.Windows.Count > 0)
            {
                currentApp.Windows[0].Page = new NavigationPage(new MainPage());
                mainPage.Navigation.PopToRootAsync();
            }
        }
    }
}
