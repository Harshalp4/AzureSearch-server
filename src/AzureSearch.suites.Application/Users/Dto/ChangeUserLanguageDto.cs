using System.ComponentModel.DataAnnotations;

namespace AzureSearch.suites.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}