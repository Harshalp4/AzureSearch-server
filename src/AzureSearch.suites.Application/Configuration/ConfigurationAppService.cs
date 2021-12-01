using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Session;
using AzureSearch.suites.Configuration.Dto;

namespace AzureSearch.suites.Configuration
{
    [AbpAuthorize]
    public class ConfigurationAppService : suitesAppServiceBase, IConfigurationAppService
    {
        public async Task ChangeUiTheme(ChangeUiThemeInput input)
        {
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
        }
    }
}
