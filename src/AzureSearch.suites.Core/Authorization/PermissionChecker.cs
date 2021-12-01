using Abp.Authorization;
using AzureSearch.suites.Authorization.Roles;
using AzureSearch.suites.Authorization.Users;

namespace AzureSearch.suites.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}
