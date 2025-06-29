using System.ComponentModel.DataAnnotations;
using RestAPI.Enums;

namespace RestAPI.Dtos
{
    public class UserDto
    {
        public class UpdateUserDto
        {
            [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
            public string? Name { get; set; }

            [EmailAddress(ErrorMessage = "Invalid email format")]
            [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
            public string? Email { get; set; }
        }

        public class CreateUserDto
        {
            [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
            public string? Name { get; set; }

            [EmailAddress(ErrorMessage = "Invalid email format")]
            [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
            public string? Email { get; set; }

            [StringLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
            public string? Password { get; set; }

            [EnumDataType(typeof(RoleEnum), ErrorMessage = "Invalid role")]
            public string? Role { get; set; }
        }
    }
}
