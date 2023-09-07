# App Configuration Core

This project is the backbone to using Azure AppConfiguration in an application. See how to use it for specific [patterns](#patterns).

## Domain/AzureAppConfiguration.cs

This is the main file to be used when using Azure AppConfiguration.

In a BackgrounService, WebAPI or WebApp the main use of the class is to provide the IConfigurationBuilder:

- Section names to Select, along with whatever appropriate labels should be used.
- Individual fields to be notified on with the IConfigurationRefresher interface.
- Class definitions/declarations of objects which are backed by the different sections in the configuration.

The class can also be used standalone to load and update configuration setting in an Azure AppConfiguration following the pattern of the Models/Sections/TestSection.cs class. Of note:

- A class property name must match the leaf name of the key for it to be used in IConfigurationBuilder.
- A class property that is backed by an Azure Key Vault secret can be read, but not updated.

## Property backing classes

See the class Models/Sections/TestSection to see how to layout a class to be used in various applications.

A class defines custom attributes on the class and class properties (Attributes/) which are then used, via reflection, to load/write settings to/from the Azure AppConfiguration.

### Important Property Notes

- The IConfigurationRefresher class is not triggered on a configuration setting backed by an Azure KeyVault secret.
- The default content type of a property in Azure AppConfiguration is String, which applies to bool, int, float etc. The code will correctly convert the string property to the correct property type.
- Storing objects you must use the content type of application/json for the configuration to successfully serialize and de-serialize to it's underlying class property. 
- Use the content type of application/vnd.microsoft.appconfig.keyvaultref for class properties that are backed by Azure KeyVault values to ensure that the property is NOT overwritten in the Azure AppConfiguration store.  

# Patterns

Use the configuration in either service based applications or standalone applications. 

## WebApp/WebAPI/Worker Service

See the RPAppConfig project for example usage.

- Provide the endpoint to the AppConfiguration
- Provide a default label for settings
- Library provides the sections to Select() on as well as the fields to be used for refresh notification.
- User need only know the specific configuration/settings classes they want to access in their workers.

## Standalone Application

The code does not solely rely on the AppConfiguration framework to read/write data to the AppConfiguration service.

```csharp
        // Provide the endpoint and a label to be used, can be null if no label is needed
        string endpoint = "https://appconfigbillingtest.azconfig.io";
        string label = "Development";

        AzureAppConfiguration azureAppConfiguration = new AzureAppConfiguration(
            endpoint,
            new DefaultAzureCredential(),
            modelLibraryForDefinitions);

        // Read settings from the app configuration into well known objects based on label. 
        TestSection? testSection = await azureAppConfiguration.LoadSection<TestSection>(label);
        
        // Make changes and either save back with the same label OR create a new version with 
        // a new label. 
        azureAppConfiguration.SaveSection(testSection, "Backup");
``````

## Documentation

Ensure docfx is up to date

> dotnet tool install --global docfx --version 2.70.3

```bash
PS C:\gitrepogrecoe\AzureAppConfiguration> cd .\docfx_project\
PS C:\gitrepogrecoe\AzureAppConfiguration\docfx_project> docfx
PS C:\gitrepogrecoe\AzureAppConfiguration\docfx_project> cd ..
PS C:\gitrepogrecoe\AzureAppConfiguration> docfx serve docfx_project/_site
```