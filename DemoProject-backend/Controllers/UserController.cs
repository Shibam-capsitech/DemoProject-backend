using DemoProject_backend.Dtos;
using DemoProject_backend.Models;
using DemoProject_backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DemoProject_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly UserService _userService;
        private readonly JwtService _jwtService;
        public UserController(UserService userService, JwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup(UserSignUpDto dto)
        {
            var user = await _userService.GetUSerByEmail(dto.Email);
            if (user != null)
            {
                return BadRequest("User Already Exists");
            }
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var newUser = new User
            {
                Name = dto.Name,
                Username = dto.Username,
                Email = dto.Email,
                Password = hashedPassword,
                City = dto.City,
                Country = dto.Country,
                Role = dto.Role,
                State = dto.State,
                Postcode = dto.Postcode,
            };
            await _userService.CreateUser(newUser);
            return Ok("User Created Successfully");

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            var user = await _userService.GetUSerByEmail(dto.Email);
            if (user == null)
                return Unauthorized("No such user exists");
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                return Unauthorized("Invalid cred");
            }

            var token = _jwtService.GenerateToken(user);
            return Ok(new { Token = token });
        }

        [HttpGet("get-all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("Unauthorized");
            var users = _userService.GetAllUsers();
            return Ok(new { users });
        }
    }
}
