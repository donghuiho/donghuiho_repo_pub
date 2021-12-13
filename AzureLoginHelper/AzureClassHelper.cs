using System;
using System.Linq;
using Microsoft.Identity.Client;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace AzureLoginHelper
{
    public class AzureClassHelper
	{

		public static string ClientId = "your client ID";
		public static string Tenant = "your tenant ID";
		public static string LastError = "";
		public static string Status = "";
		private static IPublicClientApplication clientApp;
		public static string[] scopes = new string[] { "https://graph.microsoft.com/.default" };

		public static IPublicClientApplication PubClientApp
		{
			get
			{
				return clientApp;
			}
		}

		public static void InitializeAzureAuth(string clientID="", string tenant="")
		{
			if (string.IsNullOrEmpty(clientID) == false)
				ClientId = clientID;

			if (string.IsNullOrEmpty(tenant) == false)
				Tenant = tenant;

			clientApp = PublicClientApplicationBuilder.Create(ClientId)
					.WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
					.WithAuthority(AzureCloudInstance.AzurePublic, Tenant)
					.Build();

		}


		public static async Task<AuthenticationResult> Login()
		{
			AuthenticationResult authResult = null;
			var accounts = await AzureClassHelper.PubClientApp.GetAccountsAsync();
			var firstAccount = accounts.FirstOrDefault();

			Status = "";
			try
			{

				authResult = await AzureClassHelper.PubClientApp.AcquireTokenInteractive(AzureClassHelper.scopes)
				.WithAccount(accounts.FirstOrDefault())
				.WithPrompt(Prompt.SelectAccount)
				.WithPrompt(Prompt.ForceLogin)
				.ExecuteAsync();
			}
			catch (MsalUiRequiredException ex1)
			{

				//try again
				try
				{
					authResult = await AzureClassHelper.PubClientApp.AcquireTokenInteractive(AzureClassHelper.scopes)
						.WithAccount(accounts.FirstOrDefault())
						.WithPrompt(Prompt.SelectAccount)
						.WithPrompt(Prompt.ForceLogin)
						.ExecuteAsync();
				}
				catch (MsalException ex2)
				{

					if (ex2.ErrorCode.ToLower() == "access_denied")
					{
						Status = "access_denied";
						AddToLog(Status);
					}
					else
					{
						Status = "Login failure.";
						AddToLog(LastError);
					}

				}
			}
			catch (Exception ex3)
			{
				LastError = $"Error Acquiring Token Silently:{System.Environment.NewLine}{ex3}";
				AddToLog(LastError);
				Status = "Exception";
			}
			return authResult;  //that has authResult.Account.Username
		}

		public static async Task Logout()
		{

			var accounts = await AzureClassHelper.PubClientApp.GetAccountsAsync();
			if (accounts.Any())
			{
				try
				{
					await AzureClassHelper.PubClientApp.RemoveAsync(accounts.FirstOrDefault());
					Status= "User has signed-out";
					AddToLog(LastError);
				}
				catch (MsalException ex)
				{
					Status = "Exception";
					LastError = $"Error signing-out user: {ex.Message}";
					throw new Exception($"Error signing-out user: {ex.Message}");
				}
			}
		}

		public static string GetStatus()
		{
			return Status;
		}

		public static string GetLastError()
		{
			return LastError;
		}

		private static void AddToLog(string msg)
		{
			// LogToSql.bDoDBUpdate(msg, DateTime.Now, ADID);
		}

	}
}

