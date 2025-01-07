using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;

public class CustomAuthenticationProvider : SqlAuthenticationProvider
{
    private readonly IPublicClientApplication _pca;

    public CustomAuthenticationProvider(IPublicClientApplication pca)
    {
        _pca = pca;
    }

    public override async Task<SqlAuthenticationToken> AcquireTokenAsync(SqlAuthenticationParameters parameters)
    {
        var accounts = await _pca.GetAccountsAsync();
        AuthenticationResult result;

        try
        {
            result = await _pca.AcquireTokenSilent(new[] { "https://database.windows.net/.default" }, accounts.FirstOrDefault())
                               .ExecuteAsync();
        }
        catch (MsalUiRequiredException)
        {
            result = await _pca.AcquireTokenInteractive(new[] { "https://database.windows.net/.default" })
                               .WithPrompt(Prompt.SelectAccount)
                               .WithTenantId(parameters.Authority) // Use the authority from parameters
                               .ExecuteAsync();
        }

        return new SqlAuthenticationToken(result.AccessToken, result.ExpiresOn);
    }

    public override bool IsSupported(SqlAuthenticationMethod authenticationMethod)
    {
        return authenticationMethod == SqlAuthenticationMethod.ActiveDirectoryInteractive;
    }
}