using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPM_Lab_Reporting.Properties;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;

namespace MPM_Lab_Reporting.Helpers
{
    public class Tools
    {
        private readonly string _sqldbName = Settings.SqlDB;
        private readonly string _sqlServer = Settings.SqlSvr;

        public SqlConnection? Conn { get; set; } = null;

        public async Task EnsureConnectionOpenAsync()
        {
            if (Conn == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }

            try
            {
                if (Conn.State != ConnectionState.Open)
                {
                    Conn.Close();
                    await Conn.OpenAsync();
                }
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine($"Task was canceled: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in EnsureConnectionOpenAsync: {ex.Message}");
                throw;
            }
        }

        public async Task SqlSetupAsync(IPublicClientApplication pca)
        {
            Debug.WriteLine("SqlSetupAsync Called");
            try
            {
                var tenantId = Settings.TenantID; // Get TenantID from settings
                var accounts = await pca.GetAccountsAsync();
                AuthenticationResult result;

                try
                {
                    // Attempt to acquire token silently using cached credentials
                    result = await pca.AcquireTokenSilent(new[] { "https://database.windows.net/.default", "openid", "profile", "offline_access" }, accounts.FirstOrDefault())
                                      .ExecuteAsync();
                }
                catch (MsalUiRequiredException)
                {
                    // If silent acquisition fails, fall back to interactive login
                    result = await pca.AcquireTokenInteractive(new[] { "https://database.windows.net/.default", "openid", "profile", "offline_access" })
                                      .WithPrompt(Prompt.SelectAccount)
                                      .WithTenantId(tenantId) // Use WithTenantId instead of WithAuthority
                                      .ExecuteAsync();
                }

                // Set the authentication provider with the acquired token
                SqlAuthenticationProvider.SetProvider(SqlAuthenticationMethod.ActiveDirectoryInteractive, new CustomAuthenticationProvider(pca));
                Conn = new SqlConnection(
                    new SqlConnectionStringBuilder
                    {
                        DataSource = _sqlServer,
                        InitialCatalog = _sqldbName,
                        IntegratedSecurity = false,
                        TrustServerCertificate = true,
                        Authentication = SqlAuthenticationMethod.ActiveDirectoryInteractive,
                        MultipleActiveResultSets = true
                    }.ConnectionString);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in SqlSetupAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> TrySilentLoginAsync(IPublicClientApplication pca)
        {
            Debug.WriteLine("TrySilentLoginAsync Called");
            try
            {
                var tenantId = Settings.TenantID; // Get TenantID from settings
                var accounts = await pca.GetAccountsAsync();

                try
                {
                    // Attempt to acquire token silently using cached credentials
                    var result = await pca.AcquireTokenSilent(new[] { "https://database.windows.net/.default", "openid", "profile", "offline_access" }, accounts.FirstOrDefault())
                                          .ExecuteAsync();
                    return true; // Silent login succeeded
                }
                catch (MsalUiRequiredException)
                {
                    // Silent login failed
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in TrySilentLoginAsync: {ex.Message}");
                return false;
            }
        }

        public async Task ShowMessageAsync(string title, string message)
        {
            var currentApp = Application.Current;
            if (currentApp != null && currentApp.Windows != null && currentApp.Windows.Count > 0 && currentApp.Windows[0]?.Page != null)
            {
                var page = currentApp.Windows[0].Page;
                if (page != null)
                {
                    await page.DisplayAlert(title, message, "OK");
                }
                else
                {
                    Debug.WriteLine("Unable to display alert. Page is null.");
                }
            }
            else
            {
                Debug.WriteLine("Unable to display alert. Application or Page is null.");
            }
        }

        public string GetErrorLogFilePath()
        {
            string appDataPath = FileSystem.AppDataDirectory;
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }
            return Path.Combine(appDataPath, "error_log.txt");
        }

        public static string GetDirectoryName(string filePath)
        {
            return string.IsNullOrEmpty(filePath) ? string.Empty : Path.GetDirectoryName(filePath) ?? string.Empty;
        }
    }
}
