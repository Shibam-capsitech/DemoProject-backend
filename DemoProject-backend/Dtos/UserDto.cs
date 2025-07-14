using DemoProject_backend.Enums;
using DemoProject_backend.Models;

namespace DemoProject_backend.Dtos
{
    public class UserSignUpDto
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Name { get; set; }
        public required UserRole Role { get; set; }
        public required AddressModel Address { get; set; }
    }

    public class UserLoginDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }

    }
}
