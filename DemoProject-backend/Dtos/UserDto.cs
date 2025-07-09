using DemoProject_backend.Enums;

namespace DemoProject_backend.Dtos
{
    public class UserSignUpDto
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Name { get; set; }
        public required UserRole Role { get; set; }
        public required string? City { get; set; }
        public required string? Country { get; set; }
        public required string? State { get; set; }
        public required string? Postcode { get; set; }
    }

    public class UserLoginDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }

    }
}
