using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using MPM_Lab_Reporting.Helpers;
using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace MPM_Lab_Reporting.ViewModels
{
    public class ErrorLogViewModel : ObservableObject
    {
        private readonly IMessenger _messenger;
        private ICommand? _deleteErrorLogCommand;
        private ICommand? _loadErrorLogCommand;
        private string _errorText = string.Empty;
        private int _errorCounter = 0;
        private string _errorMessage = string.Empty;

        public ErrorLogViewModel(IMessenger messenger)
        {
            Debug.WriteLine("ErrorLogViewModel Instantiated");
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _messenger.Register<ValueChangedMessage<string>>(this, HandleNotificationMessage);
        }

        public string ErrorText
        {
            get => _errorText;
            set => SetProperty(ref _errorText, value);
        }

        public ICommand DeleteErrorLogCommand =>
            _deleteErrorLogCommand ?? (_deleteErrorLogCommand = new AsyncRelayCommand(DeleteErrorLogClick));

        public ICommand LoadErrorLogCommand =>
            _loadErrorLogCommand ?? (_loadErrorLogCommand = new RelayCommand(LoadLogClick));

        private void HandleNotificationMessage(object recipient, ValueChangedMessage<string> message)
        {
            string errorFile = GetErrorLogFilePath();
            switch (message.Value)
            {
                case "ErrorYes":
                    ErrorText = File.Exists(errorFile) ? File.ReadAllText(errorFile) : "No Data";
                    break;
                case "ErrorNo":
                    ErrorText = string.Empty;
                    break;
            }
        }

        private string GetErrorLogFilePath()
        {
            string appDataPath = FileSystem.AppDataDirectory;
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }
            return Path.Combine(appDataPath, "error_log.txt");
        }

        private void LoadLogClick()
        {
            string errorFile = GetErrorLogFilePath();
            ErrorText = File.Exists(errorFile) ? File.ReadAllText(errorFile) : "No Data";
        }

        private async Task DeleteErrorLogClick()
        {
            await Task.Run(DeleteLog);

            Tools tools = new Tools();
            var mainPage = Application.Current?.Windows[0]?.Page;

            if (_errorCounter > 0)
            {
                if (mainPage != null)
                {
                    await mainPage.DisplayAlert("Error", _errorMessage, "OK");
                }
                _errorCounter = 0;
            }
            else
            {
                if (mainPage != null)
                {
                    await mainPage.DisplayAlert("Completed", "Log File Deleted", "OK");
                }
                Debug.WriteLine($"Sending message: AlertFalse");
                _messenger.Send(new ValueChangedMessage<string>("AlertFalse"));
            }
        }

        private void DeleteLog()
        {
            _errorCounter = 0;
            _errorMessage = string.Empty;

            string errorFile = GetErrorLogFilePath();

            if (File.Exists(errorFile))
            {
                try
                {
                    File.Delete(errorFile);
                    ErrorText = string.Empty;
                }
                catch (Exception er)
                {
                    _errorCounter++;
                    _errorMessage = "Unable to delete log file with error: " + er.Message;
                }
            }
            _messenger.Send(new ValueChangedMessage<string>("AlertFalse"));
        }
    }
}

