using System.ComponentModel.DataAnnotations;

namespace Argent_Company.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }

        public LoginViewModel()
        {
            Email = string.Empty;
            Password = string.Empty;
        }
    }
}
