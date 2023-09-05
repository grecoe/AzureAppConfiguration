namespace AppConfigCoreUtil.Domain
{
    using AppConfigCoreUtil.Attributes;
    using AppConfigCoreUtil.Models;
    using System.Reflection;

    internal class AssemblyHelper
    {
        /// <summary>
        /// Internal map used to cache loaded type information.
        /// </summary>
        private static Dictionary<Type, ConfigurationSectionAttribute> Sections { get; set; }
            = new Dictionary<Type, ConfigurationSectionAttribute>();

        /// <summary>
        /// Get a list of objects from a library that have the ConfigurationSectionAttribute.
        /// 
        /// If the model library is not provided, the current library is used, this is likely 
        /// overkill since the current AppConfigCoreUtil library does NOT have any objects matching
        /// that pattern. 
        /// </summary>
        /// <param name="modelLibrary">Name of the library to search for objects that implement 
        /// the ConfigurationSectionAttribute.</param>
        /// <returns>A list of all of the objects that implement the ConfigurationSectionAttribute.</returns>
        /// <exception cref="Exception"></exception>
        internal static Dictionary<Type, ConfigurationSectionAttribute> GetSections(string modelLibrary)
        {
            if (AssemblyHelper.Sections.Count == 0)
            {
                Assembly? loadAssembly = null;

                if (string.IsNullOrEmpty(modelLibrary))
                {
                    loadAssembly = Assembly.GetExecutingAssembly();
                }
                else
                {
                    AssemblyName? assemblyName = Assembly.GetEntryAssembly().GetReferencedAssemblies()
                                .Where(a => a.FullName.Contains(modelLibrary))
                                .FirstOrDefault();

                    if( assemblyName != null)
                    {
                        loadAssembly = Assembly.Load(assemblyName);
                    }
                }

                if ( loadAssembly == null)
                {
                    throw new FileNotFoundException($"Unable to load any types from {modelLibrary}");
                }

                var results = from type in loadAssembly.GetTypes()
                               let attributes = AssemblyHelper.GetConfigurationSectionAttribute(type)
                               where attributes != null
                               select attributes; 

                if (results.Count() > 0)
                {
                    foreach (KeyValuePair<Type, ConfigurationSectionAttribute> map in results)
                    {
                        ConfigurationSectionAttribute? sectionAttribute = map.Value as ConfigurationSectionAttribute;
                        if (sectionAttribute != null)
                        {
                            AssemblyHelper.Sections.Add(map.Key, sectionAttribute);
                        }
                    }
                }
            }

            return AssemblyHelper.Sections;
        }

        /// <summary>
        /// Get a mapping of object type to ConfigurationSectionAttribute for a Type.
        /// </summary>
        /// <param name="type">The type of object to validate</param>
        /// <returns>A KeyVauluePair if found, null otherwise.</returns>
        internal static KeyValuePair<Type, ConfigurationSectionAttribute>? GetConfigurationSectionAttribute(Type type)
        {
            KeyValuePair<Type, ConfigurationSectionAttribute>? return_value = null;

            var attributes = type.GetCustomAttributes(typeof(ConfigurationSectionAttribute), true);
            if(attributes.Any())
            {
                return_value = new KeyValuePair<Type, ConfigurationSectionAttribute>(type, attributes.First() as ConfigurationSectionAttribute);
            }

            return return_value;
        }


        /// <summary>
        /// Get the configuration attributes for a given type. Model library is provided
        /// in cases where this is teh first attempt. 
        /// 
        /// Checks the pre-loaded sections from the modelLibrary. If not found, attempts to 
        /// find the information directly from the type. 
        /// 
        /// This is used in both cases where a model library IS provided and used in a Service 
        /// as well as a standalone when NOT running as a service. 
        /// </summary>
        /// <param name="sectionType">Object type to retrieve.</param>
        /// <param name="modelLibrary">Optional library to load data section information.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal static ConfigAttributeMapping GetConfigurationAttributes(Type sectionType, string modelLibrary)
        {
            ConfigAttributeMapping returnMapping = new ConfigAttributeMapping();

            // Will load objects from the library if not done already.
            AssemblyHelper.GetSections(modelLibrary);

            // Attempt to find the section in the pre-loaded. 
            List<KeyValuePair<Type, ConfigurationSectionAttribute>> section = AssemblyHelper.Sections.Where(x => x.Key == sectionType).ToList();

            KeyValuePair<Type, ConfigurationSectionAttribute>? selectedSection = null;
            if (section.Any())
            {
                selectedSection = section.First();
            }
            else
            {
                // Not found in pre-loaded so attempt to get the information directly from the type.
                selectedSection = AssemblyHelper.GetConfigurationSectionAttribute(sectionType);
            }

            if(selectedSection == null)
            {
                throw new FileNotFoundException($"{sectionType.Name} is not a valid AppConfig object.");
            }
            
            // Collect the section attributes
            returnMapping.SectionAttribute = selectedSection.Value.Value;

            // Get the PropertyConfigurationAttributes from teh class. 

            // If class itself has a PropertyConfigurationAttribute, then child properties do not
            object[] classAttrs = sectionType.GetCustomAttributes(true);
            var cust = classAttrs.Where(x => (x as PropertyConfigurationAttribute) != null).ToList();
            if (cust.Any())
            {
                // Class has the configuration attribute and PropertyConfigurationAttribute must
                // also be of content type application/json. 
                returnMapping.SectionConfiguration = cust.First() as PropertyConfigurationAttribute;
                if(returnMapping.SectionConfiguration.ContentType != AppConfigurationContentType.CONTENT_JSON)
                {
                    throw new Exception("ContentType does not work in this context");
                }
            }
            else
            {
                // Class is made up of properties, so collect those. 
                PropertyInfo[] props = sectionType.GetProperties();
                foreach (PropertyInfo prop in props)
                {
                    object[] attrs = prop.GetCustomAttributes(true);
                    foreach (object attr in attrs)
                    {
                        PropertyConfigurationAttribute? oepConfig = attr as PropertyConfigurationAttribute;
                        if (oepConfig != null)
                        {
                            returnMapping.AttributeMappings.Add(prop.Name, oepConfig);
                        }
                    }
                }
            }

            return returnMapping;
        }
    }
}
