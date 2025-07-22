using CsvHelper;
using DemoProject_backend.Dtos;
using DemoProject_backend.Enums;
using DemoProject_backend.Models;
using DemoProject_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using System.Formats.Asn1;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
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

            if (lastBusiness != null && !string.IsNullOrEmpty(lastBusiness.BId))
            {
                string lastNumberPart = lastBusiness.BId.Replace("CID-", "");
                if (int.TryParse(lastNumberPart, out int parsedNumber))
                {
                    nextNumber = parsedNumber + 1;
                }
            }

            string newCustomId = $"CID-{nextNumber.ToString("D3")}";
            var userName = User.FindFirst("username")?.Value;
            var business = new Business
            {
                BId = newCustomId,
                Type = dto.Type,
                Name = dto.Name,
                Address = new AddressModel
                {
                    City = dto.City,
                    Country = dto.Country,
                    State = dto.State,
                    Building = dto.Building,
                    Postcode = dto.Postcode
                },
                CreatedBy = new CreatedByModel
                {
                    Id = userId,
                    Name = userName
                }
            };

            await _businessService.CreateBusiness(business);
            return Ok("Business Created Successfully");
        }

        /// <summary>
        /// get all particular businesses both for admin and other user admin can see all but the other user can see which are they created
        /// </summary>
        /// <param name="id"> Business id</param>
        /// <returns></returns>
        [HttpGet("get-all-businesses")]
        public async Task<IActionResult> GetOrFilterBusinesses(
        [FromQuery] string? criteria,
        [FromQuery] string? value,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("Unauthorized");

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!string.IsNullOrWhiteSpace(criteria) && !string.IsNullOrWhiteSpace(value))
            {
                var filtered = await _businessService.FilterBusinessesAsync(criteria, value);

                var totalCount = filtered.Count;
                var pagedFiltered = filtered
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Ok(new
                {
                    businesses = pagedFiltered,
                    pagination = new
                    {
                        currentPage = page,
                        pageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                });
            }

            IEnumerable<Business> businesses;
            if (userRole == "Admin")
                businesses = (IEnumerable<Business>) await _businessService.GetAllBusinessesForAdmin();
            else
                businesses = (IEnumerable<Business>) await _businessService.GetAllBusinessesAddedByCurrentUser(userId);

            if (businesses == null || !businesses.Any())
                return NotFound("No businesses found");

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                businesses = businesses.Where(b =>
                    !string.IsNullOrEmpty(b.Name) && b.Name.ToLower().Contains(lowerSearch)
                );
            }

            var total = businesses.Count();
            var pagedBusinesses = businesses
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                businesses = pagedBusinesses,
                pagination = new
                {
                    currentPage = page,
                    pageSize,
                    totalCount = total,
                    totalPages = (int)Math.Ceiling((double)total / pageSize)
                }
            });
        }


        /// <summary>
        /// Download all list
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("download-business-list")]
        public async Task<IActionResult> DownloadBusinessList()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("Unauthorized");

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            IEnumerable<Business> businesses;

            if (userRole == "Admin")
                businesses = (IEnumerable<Business>) await _businessService.GetAllBusinessesForAdmin();
            else
                businesses = (IEnumerable<Business>)  await _businessService.GetAllBusinessesAddedByCurrentUser(userId);

            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream);
            using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

            await csvWriter.WriteRecordsAsync(businesses);
            await streamWriter.FlushAsync();
            memoryStream.Position = 0;

            return File(memoryStream.ToArray(), "text/csv", "businesses.csv");
        }



        /// <summary>
        /// get a particular business by id
        /// </summary>
        /// <param name="id"> Business id</param>
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


        /// <summary>
        /// update a particular business by id
        /// </summary>
        /// <param name="id"> Business id</param>
        /// <returns></returns>
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
                BId = business.BId,
                Type = dto.Type,
                Name = dto.Name,
                Address = new AddressModel
                {
                    City = dto.City,
                    Country = dto.Country,
                    State = dto.State,
                    Building = dto.Building,
                    Postcode = dto.Postcode
                },
                CreatedBy = new CreatedByModel
                {
                    Name = business.CreatedBy.Name,
                    Id = business.CreatedBy.Id,
                    Date = business.CreatedBy.Date
                }
            };
            await _businessService.UpdateBusiness(updatedBusiness, id);
            return Ok("Business updated succesfully");
        }


        /// <summary>
        /// Soft delete of a busienss by isActive false
        /// </summary>
        /// <param name="businessId">Business Id</param>
        /// <returns></returns>
        [HttpPost("delete-business-by-id/{businessId}")]
        public async Task<IActionResult> DeleteBusiness(string businessId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("Unauthorized");

            var business = await _businessService.GetBusinessById(businessId);
            if (business == null)
                return NotFound("Task not found");

            await _businessService.DisableBusiness(businessId);

            return Ok("Business deleted !");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("get-all-businesses-name")]
        public async Task<IActionResult> GetAllBusinessNames()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("Unauthorized");

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            IEnumerable<Business> businesses;
            if (userRole == "Admin")
                businesses = (IEnumerable<Business>)await _businessService.GetAllBusinessesForAdmin();
            else
                businesses = (IEnumerable<Business>)await _businessService.GetAllBusinessesAddedByCurrentUser(userId);

            return Ok(new { businesses });
        }



        //[HttpPost("filter")]
        //public async Task<IActionResult> FilterBusinesses([FromBody] FilterReqDto filter)
        //{
        //    if (string.IsNullOrWhiteSpace(filter.Criteria) || string.IsNullOrWhiteSpace(filter.Value))
        //    {
        //        return BadRequest("Both 'criteria' and 'value' are required.");
        //    }

        //    var result = await _businessService.FilterBusinessesAsync(filter.Criteria, filter.Value);
        //    return Ok(new { filteredData = result });
        //}

    }
}
