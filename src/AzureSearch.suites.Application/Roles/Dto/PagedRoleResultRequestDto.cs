using Abp.Application.Services.Dto;

namespace AzureSearch.suites.Roles.Dto
{
    public class PagedRoleResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
    }
}

