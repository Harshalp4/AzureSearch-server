using System.Threading.Tasks;
using AzureSearch.suites.Configuration.Dto;

namespace AzureSearch.suites.Configuration
{
    public interface IConfigurationAppService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}
