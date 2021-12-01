using Abp.Configuration.Startup;
using Abp.Localization.Dictionaries;
using Abp.Localization.Dictionaries.Xml;
using Abp.Reflection.Extensions;

namespace AzureSearch.suites.Localization
{
    public static class suitesLocalizationConfigurer
    {
        public static void Configure(ILocalizationConfiguration localizationConfiguration)
        {
            localizationConfiguration.Sources.Add(
                new DictionaryBasedLocalizationSource(suitesConsts.LocalizationSourceName,
                    new XmlEmbeddedFileLocalizationDictionaryProvider(
                        typeof(suitesLocalizationConfigurer).GetAssembly(),
                        "AzureSearch.suites.Localization.SourceFiles"
                    )
                )
            );
        }
    }
}
