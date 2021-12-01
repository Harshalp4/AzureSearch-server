using AzureSearch.suites.Debugging;

namespace AzureSearch.suites
{
    public class suitesConsts
    {
        public const string LocalizationSourceName = "suites";

        public const string ConnectionStringName = "Default";

        public const bool MultiTenancyEnabled = true;


        /// <summary>
        /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
        /// </summary>
        public static readonly string DefaultPassPhrase =
            DebugHelper.IsDebug ? "gsKxGZ012HLL3MI5" : "827a50f141c8467aae400dfff976f110";
    }
}
