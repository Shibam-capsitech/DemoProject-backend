using DemoProject_backend.Models;
using DemoProject_backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DemoProject_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskHistoryController : Controller
    {
        private readonly TaskHistoryService _taskHistoryService;
        public TaskHistoryController(TaskHistoryService taskHistoryService)
        {
            _taskHistoryService = taskHistoryService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("get-task-history/{id}")]
        public async Task<IActionResult> GetTaskHistory(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("Unauthorized");
            var taskHistories = await _taskHistoryService.GetTaskHistoryById(id);
            return Ok(new { taskHistories });
        }

        [HttpGet("get-task-history-by-business-id/{businessId}")]
        public async Task<IActionResult> GetTaskHistoryByBusinessId(string businessId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("Unauthorized");
            var taskHistories = await _taskHistoryService.GetTaskHistoryByBusinessId(businessId);
            return Ok(new { taskHistories });
        }

    }
}
