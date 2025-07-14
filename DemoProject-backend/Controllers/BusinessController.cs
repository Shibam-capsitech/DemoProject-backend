using DemoProject_backend.Dtos;
using DemoProject_backend.Models;
using DemoProject_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using System.Runtime.InteropServices;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace DemoProject_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BusinessController : Controller
    {
        private readonly BusinessService _businessService;
        private readonly JwtService _jwtService;
        private readonly CloudinaryService _cloudinaryService;
        public BusinessController(BusinessService businessService, JwtService jwtService, CloudinaryService cloudinaryService)
        {
            _businessService = businessService;
            _jwtService = jwtService;
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost("create-business")]
        public async Task<IActionResult> CreateBusiness([FromForm] CreateBusinessDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Unauthorized");
            }
            var lastBusiness = await _businessService.GetLastBusinessAsync();
            int nextNumber = 1;

            if (lastBusiness != null && !string.IsNullOrEmpty(lastBusiness.bId))
            {
                string lastNumberPart = lastBusiness.bId.Replace("CID-", "");
                if (int.TryParse(lastNumberPart, out int parsedNumber))
                {
                    nextNumber = parsedNumber + 1;
                }
            }

            string newCustomId = $"CID-{nextNumber.ToString("D3")}";
            var userName = User.FindFirst("username")?.Value;
            var business = new Business
            {
                bId = newCustomId,
                type = dto.type,
                name = dto.name,
                address = new AddressModel
                {
                    city = dto.city,
                    country = dto.country,
                    state = dto.state,
                    building = dto.building,
                    postcode = dto.postcode
                },
                createdBy = new CreatedByModel
                {
                    Id = userId,
                    Name = userName
                }
            };

            await _businessService.CreateBusiness(business);
            return Ok("Business Created Successfully");
        }


        [HttpGet("get-all-businesses")]
        public async Task<IActionResult> GetAllBusinesses()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Unauthorized");
            }

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if(userRole == "Admin")
            {
                var businesses = await _businessService.GetAllBusinessesForAdmin();
                if (businesses == null)
                {
                    return NotFound("No businesses found");
                }
                return Ok(new { businesses });
            }
            else
            {
                var businesses = await _businessService.GetAllBusinessesAddedByCurrentUser(userId);
                if (businesses == null)
                {
                    return NotFound("No businesses found");
                }
                return Ok(new { businesses });
            }
        }


        [HttpGet("delete-businesses/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBusinessById(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Unauthorized");
            }

            await _businessService.DeleteBusiness(id);

            return Ok("Business deleted successfully !");
        }

        [HttpGet("get-business-by-id/{id}")]
        public async Task<IActionResult> GetBusinessById(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Unauthorized");
            }

            var business = await _businessService.GetBusinessById(id);

            return Ok(new {business});
        }

        [HttpPost("update-business-by-id/{id}")]
        public async Task<IActionResult> UpdateBusiness([FromForm] UpdateBusinessDto dto, string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Unauthorized");
            }
            GetAllBusinessesDto business = await _businessService.GetBusinessById(id);
            var updatedBusiness = new Business
            {
                bId = business.bid,
                type = dto.type,
                name = dto.name,
                address = new AddressModel
                {
                    city = dto.city,
                    country = dto.country,
                    state = dto.state,
                    building = dto.building,
                    postcode = dto.postcode
                }
            };
            await _businessService.UpdateBusiness(updatedBusiness, id);
            return Ok("Business updated succesfully");
        }

        [HttpPost("filter")]
        public async Task<IActionResult> FilterBusinesses([FromBody] FilterReqDto filter)
        {
            if (string.IsNullOrWhiteSpace(filter.Criteria) || string.IsNullOrWhiteSpace(filter.Value))
            {
                return BadRequest("Both 'criteria' and 'value' are required.");
            }

            var result = await _businessService.FilterBusinessesAsync(filter.Criteria, filter.Value);
            return Ok(new { filteredData = result });
        }

    }
}
