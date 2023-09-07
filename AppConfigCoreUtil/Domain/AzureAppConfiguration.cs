namespace AppConfigCoreUtil.Domain
{
    using AppConfigCoreUtil.Attributes;
    using AppConfigCoreUtil.Models;
    using Azure.Core;
    using Azure.Data.AppConfiguration;
    using Azure.Identity;
    using Azure.Security.KeyVault.Secrets;
    using Microsoft.Extensions.Configuration.AzureAppConfiguration;
    using Newtonsoft.Json;
    using System;
    using System.Data;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Reflection;
    using System.Reflection.Metadata;

    public class AzureAppConfiguration
    {
        /// <summary>
        /// The AppConfiguration endpoint
        /// </summary>
        public string Endpoint { get; private set; }
        /// <summary>
        /// The token credential to use on the endpoint. This can be the current 
        /// or default credential or any TokenCredential that has been given rights
        /// to the AppConfiguration and optionl KeyVault. 
        /// </summary>
        public TokenCredential Credential { get; private set; }
        /// <summary>
        /// The AppConfiguration ConfigurationClient.
        /// </summary>
        public ConfigurationClient ConfigurationClient { get; private set; }
        /// <summary>
        /// Optional library to load objects from if using in a service for auto detection
        /// of fields/properties. 
        /// </summary>
        public string ModelLibrary { get; private set; }
        /// <summary>
        /// Internal cache for secret clients needed to load data from KeyVault. 
        /// </summary>
        private Dictionary<Uri, SecretClient> SecretClientCache { get; set; } = new Dictionary<Uri, SecretClient>();

        public AzureAppConfiguration(string endpoint, TokenCredential credential, string? modelLibraryAssembly = null)
        {
            this.Endpoint = endpoint;
            this.Credential = credential;
            this.ConfigurationClient = new ConfigurationClient(new Uri(endpoint), new DefaultAzureCredential());
            this.ModelLibrary = modelLibraryAssembly != null ? modelLibraryAssembly : string.Empty;

            // Secret client..... https://github.com/Azure/azure-sdk-for-net/issues/13342
            // Cannot search for labels https://github.com/Azure/AppConfiguration/issues/647
        }

        /// <summary>
        /// Acquire the configuraiton mapping for all loaded objects. This data is used by 
        /// the service code to automatically find all sections in which to load and which 
        /// fields to be notified on.
        /// </summary>
        /// <returns>Mapping object for a WorkerService to configure AppConfiguration.</returns>
        public ConfigurationMapping GetConfigurationMapping()
        {
            ConfigurationMapping returnMapping = new ConfigurationMapping();

            Dictionary<Type, ConfigurationSectionAttribute> sectionMaps = 
                AssemblyHelper.GetSections(this.ModelLibrary);

            foreach (KeyValuePair<Type, ConfigurationSectionAttribute> section in sectionMaps)
            {
                ConfigAttributeMapping attributeMap = 
                    AssemblyHelper.GetConfigurationAttributes(section.Key, this.ModelLibrary);

                if (attributeMap != null)
                {
                    List<string> fields =
                        attributeMap.AttributeMappings.Values
                        .Where(x => x.Notify)
                        .Select(x => x.Key).Distinct().ToList();

                    if(attributeMap.SectionConfiguration != null)
                    {
                        fields.Add(attributeMap.SectionConfiguration.Key);
                    }

                    returnMapping.SectionMappings.Add(section.Value.SectionName, new SectionConfiguration());
                    returnMapping.SectionMappings[section.Value.SectionName].NotificationFields = fields;
                }
            }

            return returnMapping;
        }

        /// <summary>
        /// Create or update a configuration value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="content">Content to write</param>
        /// <param name="contentType">Content type of the value</param>
        /// <param name="label">Label if given.</param>
        public void CreateOrUpdateConfigurationSetting(
            string key, 
            string content, 
            string contentType,
            string? label = null)
        {
            ConfigurationSetting settingData = new ConfigurationSetting(key, content, label)
            {
                ContentType = contentType
            };

            try
            {
                var response = this.ConfigurationClient.AddConfigurationSetting(settingData);
            }
            catch (Azure.RequestFailedException exists)
            {
                var response = this.ConfigurationClient.SetConfigurationSetting(settingData);
            }

        }

        /// <summary>
        /// Gets a single property from the app configuration. Can be a part of a larger object
        /// or a standalone property.
        /// </summary>
        /// <typeparam name="T">Type of object to return</typeparam>
        /// <param name="key">Property key</param>
        /// <param name="label">Property label, if any</param>
        /// <returns>Instance of T with values if found, false otherwise.</returns>
        public async Task<T?> GetConfigurationSetting<T>(string key, string? label = null)
            where T: class
        {
            T? returnValue = null;

            string usableLabel = string.IsNullOrEmpty(label) ? LabelFilter.Null : label;
            ConfigurationSetting setting = new ConfigurationSetting(key, string.Empty, usableLabel);
            Azure.Response<ConfigurationSetting> searchSetting = this.ConfigurationClient.GetConfigurationSetting(setting);

            if(searchSetting.GetRawResponse().Status == (int)HttpStatusCode.OK)
            {
                if (!String.IsNullOrEmpty(searchSetting.Value.Value))
                {
                    object? value = await GetConfigurationValue(
                                                        searchSetting.Value.ContentType.ToLower(),
                                                        searchSetting.Value.Value,
                                                        typeof(T));

                    returnValue = value != null ? (T)value : null;
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Delete a single property
        /// </summary>
        /// <param name="key">Property Key</param>
        /// <param name="label">Property Label (null == none)</param>
        /// <returns>True if deleted</returns>
        public bool DeleteConfigurationSetting(string key, string? label = null)
        {
            bool deleteSuccess = false;
            Azure.Response response = this.ConfigurationClient.DeleteConfigurationSetting(key, label);
            if (response.IsError)
            {
                deleteSuccess = false;
            }
            return deleteSuccess;
        }

        /// <summary>
        /// Loads and instance of T with the given label from the AppConfiguration service. 
        /// 
        /// T must be class with the ConfigurationSectionAttribute, have a default constructor, 
        /// and have one or more PropertyConfigurationAttribute attributes.
        /// </summary>
        /// <typeparam name="T">Type of object to load from AppConfiguration</typeparam>
        /// <param name="label">Desired Label, if left null uses the defined LabelFilter.Null, i.e.
        /// no label.</param>
        /// <returns>An instance of T with the given label. If T data does exist but the label is 
        /// incorrect, null returned.</returns>
        public async Task<T?> GetSection<T>(string? label = null)
            where T : class, new()
        {
            T? returnValue = null;
            bool loadedData = false;

            ConfigAttributeMapping mapping = 
                AssemblyHelper.GetConfigurationAttributes(typeof(T), this.ModelLibrary);

            if (mapping.SectionAttribute != null)
            {
                string usableLabel = string.IsNullOrEmpty(label) ? LabelFilter.Null : label;
                SettingSelector selector = new SettingSelector()
                {
                    KeyFilter = string.Format("{0}*", mapping.SectionAttribute.SectionName),
                    LabelFilter = usableLabel
                };

                var settings = this.ConfigurationClient.GetConfigurationSettings(selector);

                if (mapping.SectionConfiguration != null)
                {
                    // There really can be only one setting here...
                    foreach (var setting in settings)
                    {
                        // Keys have to align
                        if (setting.Key == mapping.SectionConfiguration.Key &&
                            mapping.SectionConfiguration.ContentType == AppConfigurationContentType.CONTENT_JSON)
                        {
                            object? value = await GetConfigurationValue(
                                                                setting.ContentType.ToLower(),
                                                                setting.Value,
                                                                typeof(T));

                            returnValue = value == null? null : (T)value;
                            loadedData = true;
                        }
                    }
                }
                else if (mapping.AttributeMappings.Count > 0)
                {
                    returnValue = (T)Activator.CreateInstance(typeof(T));

                    foreach (var setting in settings)
                    {
                        loadedData = true;

                        IEnumerable<KeyValuePair<string, PropertyConfigurationAttribute>> attributes =
                                   from entry in mapping.AttributeMappings
                                   where (entry.Value.Key == setting.Key)
                                   select entry;

                        if (attributes.Any())
                        {
                            KeyValuePair<string, PropertyConfigurationAttribute> target = attributes.First();

                            // Have to look at type here to make sure we unroll it if required. 
                            PropertyInfo? pInfo = returnValue.GetType().GetProperty(target.Key);

                            if (pInfo != null)
                            {
                                object? value = await GetConfigurationValue(
                                    setting.ContentType.ToLower(),
                                    setting.Value,
                                    pInfo.PropertyType);

                                pInfo.SetValue(returnValue, value);
                            }
                        }
                    }
                }

            }
            return loadedData ? returnValue : null;
        }

        /// <summary>
        /// Saves an instance of T to AppConfiguration with the given label. 
        /// 
        /// T must be class with the ConfigurationSectionAttribute, have a default constructor, 
        /// and have one or more PropertyConfigurationAttribute attributes.
        /// 
        /// NOTE: Values from a KeyVault cannot be saved in this fashion. 
        /// </summary>
        /// <typeparam name="T">Type of object to save to AppConfiguration.</typeparam>
        /// <param name="sectionContent">Instance of T</param>
        /// <param name="optionalLabel">Label in which to save</param>
        public void CreateOrUpdateSection<T>(T sectionContent, string? optionalLabel = null)
            where T : class, new()
        {
            ConfigurtionIdentities identities = GetIdentities<T>(false, optionalLabel);

            foreach (ConfigurationIdentity id in identities.IdentityList)
            {
                string content = String.Empty;

                if (identities.SectionAsProperty)
                {
                    content = JsonConvert.SerializeObject(sectionContent, Formatting.Indented);
                }
                else
                {
                    PropertyInfo? pInfo = sectionContent.GetType().GetProperty(id.UnderlyingProperty);
                    if (pInfo != null)
                    {
                        object? value = pInfo.GetValue(sectionContent, null);
                        if (value != null)
                        {
                            if (id.ContentType == AppConfigurationContentType.CONTENT_JSON)
                            {
                                content = JsonConvert.SerializeObject(value, Formatting.Indented);
                            }
                            else
                            {
                                content = value.ToString();
                            }
                        }
                    }
                }

                this.CreateOrUpdateConfigurationSetting(id.Key, content, id.ContentType, id.Label);
            }
        }

        /// <summary>
        /// Delete a section from AppConfiguration with the given label. 
        /// </summary>
        /// <typeparam name="T">Type of object to save to AppConfiguration.</typeparam>
        /// <param name="optionalLabel">Label in which to save</param>
        /// <returns>True if deleted.</returns>
        public bool DeleteSection<T>(string? label = null)
            where T : class, new()
        {
            bool deleteSuccess = true;

            ConfigurtionIdentities identities = GetIdentities<T>(includeSecrets: true, optionalLabel: label);
            foreach (ConfigurationIdentity id in identities.IdentityList)
            {
                if (this.DeleteConfigurationSetting(id.Key, id.Label))
                {
                    deleteSuccess = false;
                }
            }

            return deleteSuccess;
        }



        /// <summary>
        /// Get the identity map between the AppConfiguration and the underlying class that is supporting
        /// the type.
        /// 
        /// This call excludes, by default, the KeyVault based properties because they cannot be saved.
        /// </summary>
        /// <typeparam name="T">Type of object to get the identity mapping from.</typeparam>
        /// <param name="includeSecrets">If true, returns the KV based properties.</param>
        /// <param name="optionalLabel">Optional label information.</param>
        /// <returns> Instance of ConfigurtionIdentities</returns>
        private ConfigurtionIdentities GetIdentities<T>(bool includeSecrets = false, string? optionalLabel = null)
            where T : class, new()
        {
            ConfigurtionIdentities returnIdentities = new ConfigurtionIdentities();

            ConfigAttributeMapping mapping = 
                AssemblyHelper.GetConfigurationAttributes(typeof(T),this.ModelLibrary);


            string settingLabel = string.IsNullOrEmpty(optionalLabel) ? LabelFilter.Null : optionalLabel;

            if (mapping.SectionAttribute != null)
            {
                if(mapping.SectionConfiguration != null && 
                    mapping.SectionConfiguration.ContentType == AppConfigurationContentType.CONTENT_JSON)
                {
                    returnIdentities.SectionAsProperty = true;
                    returnIdentities.IdentityList.Add(new ConfigurationIdentity()
                    {
                        Key = mapping.SectionConfiguration.Key,
                        Label = settingLabel,
                        ContentType = mapping.SectionConfiguration.ContentType
                    });
                }
                else if (mapping.AttributeMappings.Count > 0)
                {
                    foreach (KeyValuePair<string, PropertyConfigurationAttribute> prop in mapping.AttributeMappings)
                    {
                        // We do NOT save out keyvault information
                        if (prop.Value.ContentType == AppConfigurationContentType.CONTENT_KV && includeSecrets == false)
                        {
                            continue;
                        }

                        returnIdentities.IdentityList.Add(new ConfigurationIdentity()
                        {
                            Key = prop.Value.Key,
                            Label = settingLabel,
                            ContentType = prop.Value.ContentType,
                            UnderlyingProperty = prop.Key
                        });
                    }
                }
            }
            return returnIdentities;
        }

        /// <summary>
        /// Get a single configuration value.
        /// </summary>
        /// <param name="contentType">Type of the value</param>
        /// <param name="value">The stored value</param>
        /// <param name="propertyType">Type it should be.</param>
        /// <returns></returns>
        private async Task<object?> GetConfigurationValue(string contentType, string value, Type propertyType)
        {
            object? returnObject = null;

            if (!string.IsNullOrEmpty(contentType) && contentType.StartsWith(AppConfigurationContentType.CONTENT_KV))
            {
                returnObject = await this.GetKVSecretValue(value);
            }
            else
            {
                switch (contentType)
                {
                    case AppConfigurationContentType.CONTENT_JSON:
                        returnObject = DeserializeObject(propertyType, value);
                        break;
                    default:
                        returnObject = Convert.ChangeType(value, propertyType);
                        break;
                }
            }

            return returnObject;
        }

        /// <summary>
        /// Retrieve a secret from keyvault with the payload provided for the property
        /// in the form of a keyvault reference.
        /// </summary>
        /// <param name="kvPayload">Value stored in AppConfiguration when type indicates that
        /// it comes from keyvault. Payload has enough information that with the token provider
        /// we can retrieve the underlying value.</param>
        /// <returns>Secret value if found, null otherwise. </returns>
        private async Task<string?> GetKVSecretValue(string kvPayload)
        {
            string? secretValue = null;
            Dictionary<string, object>? keyValuePairs = JsonConvert.DeserializeObject<Dictionary<string, object>>(kvPayload);

            if (keyValuePairs != null && keyValuePairs.ContainsKey("uri"))
            {
                if (keyValuePairs["uri"] != null)
                {
                    string secretURIValue = keyValuePairs["uri"].ToString();
                    Uri secretUri = new Uri(secretURIValue, UriKind.Absolute);

                    string secretName = secretUri.Segments.ElementAtOrDefault(2).TrimEnd('/');
                    string secretVersion = string.Empty;

                    if (secretUri.Segments.ElementAtOrDefault(3) != null)
                    {
                        secretVersion = secretUri.Segments.ElementAtOrDefault(3).TrimEnd('/');
                    }
                    string keyVaultId = secretUri.Host;

                    SecretClient? client = null;
                    Uri secretVaultUri = new Uri(secretUri.GetLeftPart(UriPartial.Authority));

                    if (this.SecretClientCache.ContainsKey(secretVaultUri))
                    {
                        client = this.SecretClientCache[secretVaultUri];
                    }
                    else
                    {
                        client = new SecretClient(secretVaultUri, this.Credential);
                        this.SecretClientCache.Add(secretVaultUri, client);
                    }

                    KeyVaultSecret secret = await client.GetSecretAsync(secretName, secretVersion).ConfigureAwait(false);

                    secretValue = secret?.Value;
                }
            }
            return secretValue;

        }

        /// <summary>
        /// Deserialize some generic object from application/json content
        /// </summary>
        /// <param name="T">Object type to create</param>
        /// <param name="content">JSON Content</param>
        /// <returns>Object, if valid.</returns>
        private static object? DeserializeObject(Type T, string content)
        {
            object? returnObject = null;
            if (!string.IsNullOrEmpty(content))
            {
                MethodInfo genericDeserialize = GetGenericObjectDeserialize();
                MethodInfo genericMethod = genericDeserialize.MakeGenericMethod(T);
                returnObject = genericMethod.Invoke(null, new object[] { content });
            }
            return returnObject;
        }

        /// <summary>
        /// Generalize object deserialization using Newtonsoft.JsonConvert.DeserializeObject
        /// for a given type. 
        /// </summary>
        /// <returns>MethodInfo object to be used to deserialize a specific type of object.</returns>
        /// <exception cref="Exception">If generic deserializer cannot be created.</exception>
        private static MethodInfo GetGenericObjectDeserialize()
        {
            Type? stringType = Type.GetType("System.String");
            MethodInfo? genericDeserialize = typeof(JsonConvert)
                .GetMethod("DeserializeObject", 1, new Type[] { stringType });

            if (genericDeserialize == null)
            {
                throw new Exception("Cannot generage generic JsonConvert.DeserializeObject");
            }
            return genericDeserialize;
        }
    }
}
