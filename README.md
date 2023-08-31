# Azure App Configuration Made Easy

Azure AppConfiguration makes it simple to store application settings for your application and services. It seamlessly provides access to labelled data making it trivial to switch between Prod/Dev/Test environments while also seamlessly providing access to values in Azure KeyVault. To learn more about Azure AppConfiguration [read the docs](https://learn.microsoft.com/en-us/azure/azure-app-configuration/overview).

This project is made up of 3 assemblies

|Assebly|Purpose|
|---|---|
|AppConfigDriver|A sample application as a worker service that requires settings, some secret, to perform it's work.|
|AppConfigCoreUtil|The workhorse of the solution. This provides the wrappers around the Azure AppConfiguration service.|
|AppConfigModelLib|A class library of the class definitions that map to the individual key/value pairs within the underlying Azure AppConfiguration service.|

## Pre-Requisites

- You must have an Azure subscription to follow along with this code. You can get a [free](https://azure.microsoft.com/en-us/free) if you need/want one. Be aware, there is limited funds on a free account and you will NOT run over that amount testing this project. 
- Install the [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) on your machine/environment.
    - Once installed, perform the `az login` command and ensure you are logged into Azure locally.
- Login to the [Azure Portal](https://portal.azure.com), find your subscription and create a Resource Group for the following resources. 
- Create an [Azure KeyVault](#azure-keyvault) and follow the directions in that section. 
- Create an [Azure AppConfiguration](#azure-appconfiguration) and follow the directions in that section.
- From the blade of your Azure AppConfiguration, copy the Endpoint value from the Essentials section. 
    - Update AppConfigDriver/appsettings.json on line 9 with this information.
- Open Visual Studio.
- Set AppConfigDriver as the Startup Project
- Hit F5 to start the program.

#### Azure KeyVault

- In the Azure Portal create an Azure KeyVault in the resource group you created.
- On the KeyVault blade, select Access Control (IAM)
- Add yourself with the role assignment *Key Vault Secrets Officer*
- Select Secrets from the KeyVault blade.
- Create a new secret, which we will use later. The content of the secret doesn't matter for the example.

#### Azure AppConfiguration

- In the Azure Portal create an *App Configuration* in the resource group you created.
- On the KeyVault blade, select Access Control (IAM)
- Add yourself with the role assignment *App Configuration Data Owner*
- Add the following properties in *Configuration explorer* for the App Configuration:

```bash
Create: Key-value
Key: SingleProp:Data
Label: Development
ContentType: application.json
Value:
{
	"Name" : "WestUSRULES",
	"Region" : "WestUS"
}
```

```bash
Create: Key-value
Key: Cosmos:Enabled
Label: Development
ContentType: string
Value: True
```

```bash
Create: Key-value
Key: Cosmos:Database
Label: Development
ContentType: application/json
Value:
{
	"Database" : "MyCosmosDB",
	"Collections" : [
		"customers",
		"address"
	]
}
```

```bash
Create: Key Vault reference
Key: Cosmos:ConnectionString
Label: Development

Choose the KeyVault and Secret you created in your Key Vault.
```
