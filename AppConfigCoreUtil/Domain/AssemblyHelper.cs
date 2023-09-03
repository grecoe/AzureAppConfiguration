namespace AppConfigCoreUtil.Domain
{
    using AppConfigCoreUtil.Attributes;
    using AppConfigCoreUtil.Models;
    using System.Reflection;

    internal class AssemblyHelper
    {
        private static Dictionary<Type, ConfigurationSectionAttribute> Sections { get; set; }
            = new Dictionary<Type, ConfigurationSectionAttribute>();

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
                    throw new Exception("Unable to load any types.");
                }

                var results = from type in loadAssembly.GetTypes()
                              let attributes = type.GetCustomAttributes(typeof(ConfigurationSectionAttribute), true)
                              where attributes != null && attributes.Length > 0
                              select new KeyValuePair<Type, object>(type, attributes.First());
                if (results.Count() > 0)
                {
                    foreach (KeyValuePair<Type, object> map in results)
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


        internal static ConfigAttributeMapping GetConfigurationAttributes(Type sectionType, string modelLibrary)
        {
            AssemblyHelper.GetSections(modelLibrary);

            ConfigAttributeMapping returnMapping = new ConfigAttributeMapping();

            List<KeyValuePair<Type, ConfigurationSectionAttribute>> section = AssemblyHelper.Sections.Where(x => x.Key == sectionType).ToList();


            KeyValuePair<Type, ConfigurationSectionAttribute>? selectedSection = null;
            if (section.Any())
            {
                selectedSection = section.First();
            }
            else
            {
                selectedSection = AssemblyHelper.GetConfigurationSectionAttribute(sectionType);
            }

            if(selectedSection == null)
            {
                throw new Exception($"{sectionType.Name} is not a valid AppConfig object.");
            }
            

            returnMapping.SectionAttribute = selectedSection.Value.Value;

            // If class itself has a PropertyConfigurationAttribute, then child properties do not
            object[] classAttrs = sectionType.GetCustomAttributes(true);
            var cust = classAttrs.Where(x => (x as PropertyConfigurationAttribute) != null).ToList();

            if (cust.Any())
            {
                // Class has teh configuration attribute, so ignore properties, but it MUST have a content
                // type of application/json
                returnMapping.SectionConfiguration = cust.First() as PropertyConfigurationAttribute;
                if(returnMapping.SectionConfiguration.ContentType != AppConfigurationContentType.CONTENT_JSON)
                {
                    throw new Exception("ContentType does not work in this context");
                }
            }
            else
            {
                // Class is made up of properties
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
