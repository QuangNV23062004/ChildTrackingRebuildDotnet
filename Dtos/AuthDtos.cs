using System.ComponentModel.DataAnnotations;

namespace RestAPI.Dtos
{
    public class AuthDto
    {
        public class RegisterDto
        {
            [Required(ErrorMessage = "Name is required")]
            [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
            public string Name { get; set; }

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email format")]
            [StringLength(255, ErrorMessage = "Email must not exceed 255 characters")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Password is required")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
            public string Password { get; set; }
        }

        public class LoginDto
        {
            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email format")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Password is required")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
            public string Password { get; set; }
        }
    }
}