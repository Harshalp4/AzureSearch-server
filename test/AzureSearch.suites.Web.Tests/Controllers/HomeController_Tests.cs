using System.Threading.Tasks;
using AzureSearch.suites.Models.TokenAuth;
using AzureSearch.suites.Web.Controllers;
using Shouldly;
using Xunit;

namespace AzureSearch.suites.Web.Tests.Controllers
{
    public class HomeController_Tests: suitesWebTestBase
    {
        [Fact]
        public async Task Index_Test()
        {
            await AuthenticateAsync(null, new AuthenticateModel
            {
                UserNameOrEmailAddress = "admin",
                Password = "123qwe"
            });

            //Act
            var response = await GetResponseAsStringAsync(
                GetUrl<HomeController>(nameof(HomeController.Index))
            );

            //Assert
            response.ShouldNotBeNullOrEmpty();
        }
    }
}