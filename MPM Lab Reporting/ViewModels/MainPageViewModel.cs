using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MPM_Lab_Reporting.Helpers;
using Microsoft.Maui.Controls;

namespace MPM_Lab_Reporting.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly Tools _tools;
        private readonly IPublicClientApplication _pca;

        public MainPageViewModel(Tools tools, IPublicClientApplication pca)
        {
            _tools = tools;
            _pca = pca;
            LogInCommand = new AsyncRelayCommand(LogInAsync);
            LogOutCommand = new AsyncRelayCommand(LogOutAsync);
            ButtonText = "Login To Entra";
            IsButtonEnabled = true;
            IsLogoutButtonEnabled = false;
            FlyoutBehavior = FlyoutBehavior.Disabled;
            HasError = false;

            // Attempt silent login on initialization
            _ = AttemptSilentLoginAsync();
        }

        private string? _buttonText;
        public string? ButtonText
        {
            get => _buttonText;
            set => SetProperty(ref _buttonText, value);
        }

        private bool _isButtonEnabled;
        public bool IsButtonEnabled
        {
            get => _isButtonEnabled;
            set => SetProperty(ref _isButtonEnabled, value);
        }

        private bool _isLogoutButtonEnabled;
        public bool IsLogoutButtonEnabled
        {
            get => _isLogoutButtonEnabled;
            set => SetProperty(ref _isLogoutButtonEnabled, value);
        }

        private bool _isLoggedIn;
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
        }

        private FlyoutBehavior _flyoutBehavior;
        public FlyoutBehavior FlyoutBehavior
        {
            get => _flyoutBehavior;
            set => SetProperty(ref _flyoutBehavior, value);
        }

        private bool _hasError;
        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        public IAsyncRelayCommand LogInCommand { get; }
        public IAsyncRelayCommand LogOutCommand { get; }

        private async Task AttemptSilentLoginAsync()
        {
            var silentLoginSucceeded = await _tools.TrySilentLoginAsync(_pca);

            if (silentLoginSucceeded)
            {
                var accounts = await _pca.GetAccountsAsync();
                var firstAccount = accounts.FirstOrDefault();
                var userName = firstAccount?.Username ?? "Unknown User";

                ButtonText = $"Logged into Entra as {userName}";
                IsLoggedIn = true;
                IsLogoutButtonEnabled = true;
                FlyoutBehavior = FlyoutBehavior.Flyout;
                IsButtonEnabled = false;
            }
            else
            {
                // Silent login failed, enable the login button
                ButtonText = "Login To Entra";
                IsButtonEnabled = true;
            }
        }

        private async Task LogInAsync()
        {
            try
            {
                ButtonText = "Loading Entra Connection";
                IsButtonEnabled = false;

                await _tools.SqlSetupAsync(_pca);

                var accounts = await _pca.GetAccountsAsync();
                var firstAccount = accounts.FirstOrDefault();
                var userName = firstAccount?.Username ?? "Unknown User";

                ButtonText = $"Logged into Entra as {userName}";
                IsLoggedIn = true;
                IsLogoutButtonEnabled = true;
                FlyoutBehavior = FlyoutBehavior.Flyout;
            }
            catch (Exception ex)
            {
                var mainPage = Application.Current?.Windows[0]?.Page;
                if (mainPage != null)
                {
                    await mainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                }
                ButtonText = "Login To Entra";
                IsButtonEnabled = true;
                HasError = true;
            }
        }

        private async Task LogOutAsync()
        {
            try
            {
                var accounts = await _pca.GetAccountsAsync();
                foreach (var account in accounts)
                {
                    await _pca.RemoveAsync(account);
                }

                IsLoggedIn = false;
                ButtonText = "Login To Entra";
                IsButtonEnabled = true;
                IsLogoutButtonEnabled = false;
                FlyoutBehavior = FlyoutBehavior.Disabled;
            }
            catch (Exception ex)
            {
                var mainPage = Application.Current?.Windows[0]?.Page;
                if (mainPage != null)
                {
                    await mainPage.DisplayAlert("Error", $"An error occurred during logout: {ex.Message}", "OK");
                }
                HasError = true;
            }
        }
    }
}
