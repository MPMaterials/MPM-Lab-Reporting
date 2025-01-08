using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MPM_Lab_Reporting.Messages;
using System.Threading.Tasks;

namespace MPM_Lab_Reporting.ViewModels
{
    public partial class AppShellViewModel : ObservableObject
    {
        public AppShellViewModel()
        {
            NavigateCommand = new AsyncRelayCommand<string?>(NavigateAsync);
            WeakReferenceMessenger.Default.Register<FlyoutBehaviorMessage>(this, (r, m) =>
            {
                FlyoutBehavior = m.Value;
            });
            WeakReferenceMessenger.Default.Register<ErrorMessage>(this, (r, m) =>
            {
                HasError = m.Value;
            });
            FlyoutBehavior = FlyoutBehavior.Disabled;
            HasError = false;

        }

        public IAsyncRelayCommand<string?> NavigateCommand { get; }

        private async Task NavigateAsync(string? route)
        {
            if (route != null)
            {
                await Shell.Current.GoToAsync(route);
            }
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
    }
}
