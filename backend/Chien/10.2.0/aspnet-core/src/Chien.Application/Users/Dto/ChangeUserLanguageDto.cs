using System.ComponentModel.DataAnnotations;

namespace Chien.Users.Dto;

public class ChangeUserLanguageDto
{
    [Required]
    public string LanguageName { get; set; }
}