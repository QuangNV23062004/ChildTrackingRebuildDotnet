using System.ComponentModel.DataAnnotations;

namespace RestAPI.Dtos
{
    public class UserDto
    {
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string? Name { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string? Email { get; set; }
    }
}