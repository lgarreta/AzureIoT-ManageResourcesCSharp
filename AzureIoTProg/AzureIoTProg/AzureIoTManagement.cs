// Install de Nuget packages ResourceManager, IdentityModel, Azure.Management
using System;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.ResourceManager.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Microsoft.Azure.Management.Storage;
using Microsoft.Azure;
using Microsoft.Azure.Management.Storage.Models;

namespace AzureIoTProgTest {
	class AzureIoTProgClass {
		// Replace de corresponding values taken from the autentication process
		static string tmpApplicationId = "4c1f4d33-e87b-4395-b758-dbc0d4afe25f";
		static string tmpSubscriptionId = "933a5bc9-18cb-4cd3-800a-2a2fdd692fb6";
		static string tmpTenantId = "2707f6c8-6127-4a28-858a-1bad498d78b8";
		static string tmpAppPassword = "iothub2017A";

		// Temporal names for resources
		static string tmpResourceGroupName = "progrg1";
		static string tmpResourceGroupLocation = "East US";
		static string tmpIoTHubName = "progiothub1";
		static string tmpDeployName = "progdeploy1";
		static string tmpStorageAccountName = "progstrgacc1";
		static string tmpStorageAddress = "https://progstrgacc1.blob.core.windows.net/";

		static void Main(string[] args) {
			// Retrieve a token from Azure AD using the application id and password
			var accessToken = GetAuthorizationToken (tmpApplicationId, tmpSubscriptionId, tmpTenantId, tmpAppPassword);

			// Create, or obtain a reference to, the resource group you are using
			var rgResponse = CreateUpdateResourceGroup (accessToken, tmpSubscriptionId, tmpResourceGroupName, tmpResourceGroupLocation);

			// Create a storage account
			CreateStorageAccount(accessToken, tmpSubscriptionId, tmpStorageAccountName,	tmpResourceGroupName, tmpResourceGroupLocation);

			// Create the IoTHub (it uses the templates uploaded to the account storage
			//CreateIoTHub(accessToken, tmpSubscriptionId, tmpResourceGroupName, tmpDeployName);

			Console.WriteLine("End, press some key");
			Console.ReadLine();
		}

		//----------------------------------------------------------------------
		// Return the resource management client by retrieve a token from Azure AD
		//----------------------------------------------------------------------
		private static string	GetAuthorizationToken (string applicationId, string subscriptionId, string tenantId, string appPassword)	{
			Console.WriteLine("Getting authorization token credentials" + "...");
			var credential = new ClientCredential(applicationId, appPassword);
			var authContext = new AuthenticationContext(string.Format("https://login.windows.net/{0}", tenantId));
			AuthenticationResult token = authContext.AcquireTokenAsync("https://management.core.windows.net/", credential).Result;
			if (token == null) {
				throw new InvalidOperationException("Failed to obtain the token");
			}
			return token.AccessToken;
		}

		//----------------------------------------------------------------------
		// Create, or obtain a reference to, the resource group you are using
		//----------------------------------------------------------------------
		private static ResourceGroup CreateUpdateResourceGroup(string accessToken, string subscriptionId, string rgName, string rgLocalization) {
			Console.WriteLine("Creating/Updating resource group: " + rgName + "...");

			var tokenCredentials = new TokenCredentials(accessToken);
			var rmClient = new ResourceManagementClient(tokenCredentials) {SubscriptionId = subscriptionId};

			var rgResponse = rmClient.ResourceGroups.CreateOrUpdate(rgName, new ResourceGroup(rgLocalization));
			if (rgResponse.Properties.ProvisioningState != "Succeeded") {
				throw new InvalidOperationException("Failed to creating/updating resource group");
			}
			Console.WriteLine("Resource Group:\n" + rgResponse.ToString());
			return rgResponse;
		}

		//---------------------------------------------------------
		// Create an Storage Account
		//---------------------------------------------------------
		private static void CreateStorageAccount(string accessToken, string subscriptionId, string accountName, string rgName, string rgLocalization)
		{
			Console.WriteLine("Creating an Storage Account: " + "...");
			var tokenCredentials = new TokenCredentials(accessToken);
			var rmClient = new ResourceManagementClient(tokenCredentials) { SubscriptionId = subscriptionId };
			// Add a Microsoft.Storage provider to the subscription.
			var storageProvider = rmClient.Providers.Register("Microsoft.Storage");

			// Auth..
			var cloudTokenCredencials = new TokenCloudCredentials(subscriptionId, accessToken);
			var smClient = new StorageManagementClient(cloudTokenCredencials);

			// Your new storage account.
			var storageAccount = smClient.StorageAccounts.Create(rgName, accountName,
					new StorageAccountCreateParameters()
					{
						Location = rgLocalization,
						AccountType = AccountType.StandardLRS
					}
			);
			Console.WriteLine(">>> Storage Account: " + storageAccount.ToString());
		}

		//---------------------------------------------------------
		// Create an IoT Hub
		//---------------------------------------------------------
		static void CreateIoTHub (string accessToken, string subscriptionId, string rgName, string deploymentName) {
			Console.WriteLine("Creating IoT Hub " + "...");
			var tokenCredentials = new TokenCredentials(accessToken);
			var resourceManagementClient = new ResourceManagementClient(tokenCredentials) { SubscriptionId = subscriptionId };

			// Submit the template and parameter files to the Azure Resource Manager
			var createResponse = resourceManagementClient.Deployments.CreateOrUpdate(
				rgName, deploymentName, new Deployment() {
					Properties = new DeploymentProperties {
						Mode = DeploymentMode.Incremental,
						TemplateLink = new TemplateLink { Uri = tmpStorageAddress + "templates/esquema_iothub.json" },
						ParametersLink = new ParametersLink { Uri = tmpStorageAddress + "templates/parametros_iothub.json" },
					} });

			// Displays the status and the keys for the new IoT hub:
			string state = createResponse.Properties.ProvisioningState;
			Console.WriteLine("Deployment state: {0}", state);
			if (state != "Succeeded") 	{
				throw new InvalidOperationException("Failed to create the IoT Hub");
			}
			Console.WriteLine(createResponse.Properties.Outputs);
		}
	}
}
