using System.Configuration;
using System.Globalization;
using System.IO;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MPM_Lab_Reporting.Helpers
{
    public class ErrorCatch
    {
        private static Tools? _tools;
        private static IMessenger? _messenger;

        public static void Initialize(Tools tools, IMessenger messenger)
        {
            _tools = tools;
            _messenger = messenger;
        }

        public static async void Error(Exception e)
        {
            string? appDataPath = FileSystem.AppDataDirectory;
            if (string.IsNullOrEmpty(appDataPath))
            {
                // Handle the case where appDataPath is null or empty
                _messenger?.Send(new ValueChangedMessage<string>("AlertTrue"));
                if (_tools != null)
                {
                    await _tools.ShowMessageAsync("Error", "AppDataDirectory is not available.");
                }
                return;
            }
            string errorFile = Path.Combine(appDataPath, "error_log.txt");
            string now = DateTime.Now.ToString(CultureInfo.InvariantCulture);

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            if (!File.Exists(errorFile))
            {
                using (FileStream fs = File.Create(errorFile)) { }
            }

            if (!e.ToString().Contains("There is no row at position 0."))
            {
                _messenger?.Send(new ValueChangedMessage<string>("AlertTrue"));
                bool success = false;
                int retries = 3;
                while (!success && retries > 0)
                {
                    try
                    {
                        File.AppendAllText(errorFile, $"{now} {e}{Environment.NewLine}");
                        success = true;
                    }
                    catch (IOException)
                    {
                        retries--;
                        System.Threading.Thread.Sleep(100); // Wait for 100ms before retrying
                    }
                }
                // Show the error message using _tools.ShowMessageAsync
                if (_tools != null)
                {
                    _messenger?.Send(new ValueChangedMessage<string>("AlertTrue"));
                    await _tools.ShowMessageAsync("Error", e.ToString());
                }
            }
        }
    }
}
