using Abp.AutoMapper;
using AzureSearch.suites.Authentication.External;

namespace AzureSearch.suites.Models.TokenAuth
{
    [AutoMapFrom(typeof(ExternalLoginProviderInfo))]
    public class ExternalLoginProviderInfoModel
    {
        public string Name { get; set; }

        public string ClientId { get; set; }
    }
}
