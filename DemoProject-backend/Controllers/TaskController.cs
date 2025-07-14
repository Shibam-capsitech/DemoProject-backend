using DemoProject_backend.Dtos;
using DemoProject_backend.Enums;
using DemoProject_backend.Models;
using DemoProject_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.FileProviders;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Claims;
using System.Threading.Channels;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DemoProject_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : Controller
    {
        private readonly TaskService _taskService;
        private readonly JwtService _jwtService;
        private readonly CloudinaryService _cloudinaryService;
        private readonly UserService _userService;
        private readonly TaskHistoryService _taskHistoryService;
        public TaskController(TaskService taskService, JwtService jwtService, CloudinaryService cloudinaryService, UserService userService, TaskHistoryService taskHistoryService)
        {
            _taskService = taskService;
            _jwtService = jwtService;
            _cloudinaryService = cloudinaryService;
            _userService = userService;
            _taskHistoryService = taskHistoryService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"> The DTO for Task Creation </param>
        /// <param name="file"> Attachment File</param>
        /// <returns></returns>
        [HttpPost("create-task")]
        public async Task<IActionResult> CreateTask([FromForm] CreateTaskDto dto, IFormFile file)
        {
            var fileResult = await _cloudinaryService.UploadImageAsync(file);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Unauthorized");
            }
            if (fileResult.Error != null)
            {
                return BadRequest(fileResult.Error.Message);
            }
            var lastTask = await _taskService.GetLastTask();
            int nextNumber = 1;

            if (lastTask != null && !string.IsNullOrEmpty(lastTask.tid))
            {
                string lastNumberPart = lastTask.tid.Replace("TID-", "");
                if (int.TryParse(lastNumberPart, out int parsedNumber))
                {
                    nextNumber = parsedNumber + 1;
                }
            }
            string newCustomId = $"TID-{nextNumber.ToString("D3")}";
            var userName = User.FindFirst("username")?.Value;
            Console.Write(".................");
            Console.WriteLine(dto.assignee.Id);
            Console.WriteLine(userName);
            User assignee = await _userService.GetUSerById(dto.assignee.Id);
            Console.WriteLine("Assignee ID: " + assignee.Id);  
            Console.WriteLine("Assignee Name: " + assignee.username);  

            var task = new Models.Task
            {
                tid= newCustomId,
                type = dto.type,
                title = dto.title,
                startdate = dto.startdate,
                duedate = dto.duedate,
                deadline = dto.deadline,
                description = dto.description,
                assignee = new IdNameModel
                {
                    Id = assignee.Id,
                    Name = assignee.username,
                },
                attachment = fileResult.SecureUrl.ToString(),
                businessDetails = new IdNameModel
                {
                    Id = dto.businessDetails.Id,
                    Name = dto.businessDetails.Name,
                },
                createdBy = new CreatedByModel
                {
                    Id = userId,
                    Name = userName,
                },
                subtask = new List<SubTask>(),
            };
            await _taskService.CreateTask(task);

            //var changes = new List<FieldChange>
            //    {
            //     new FieldChange
            //     {
            //        Field = "Task Created",
            //        PreviousValue = null,
            //        NewValue = $"New task created: {dto.title}",
            //        IsChangeRegardingSubTask = false,
            //        IsChangeRegardingTask= true
            //      }
            //      };
            
            var taskHistory = new TaskHistoryModel
            {
                TargetTask = new IdNameModel
                {
                    Id = task.Id, 
                    Name = task.title,
                },
                TargetBusiness = new IdNameModel
                {
                  Id = task.businessDetails.Id,
                  Name = task.businessDetails.Name,
                },
                CreatedBy = new CreatedByModel
                {
                    Id = userId,
                    Name = userName
                },
                ChangeType = ChangeTypeEnum.Add,
                Description = $"<strong>New task created</strong> — <span>Title: {dto.title}</span>"
            };

            await _taskHistoryService.CreateTaskHistory(taskHistory);


            var subtask = new SubTask
            {
                title = "Gather information from client",
                status = "Waiting",
            };
            await _taskService.CreateSubTask(subtask, task.Id);

            return Ok("New task created");
        }


        [HttpGet("get-all-task")]
        public async Task<IActionResult> GetAllTasks()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Unauthorized");
            }
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            Console.WriteLine(userRole);
            if (userRole == "Admin")
            {
                var tasks = await _taskService.GetAllTaskForAmin();
                if (tasks == null)
                {
                    return NotFound("No tasks found");
                }
                return Ok(new { tasks });

            }
            else if (userRole == "Manager" || userRole == "Staff")
            {
                var tasks = await _taskService.GetAllTaskAddedByCurrentUser(userId);
                if (tasks == null)
                {
                    return NotFound("No tasks found");
                }
                return Ok(new { tasks });
            }
            else
            {
                return BadRequest("Cannot Perform Get Task Action");
            }
        }

        //[HttpGet("get-tasks")]
        //public async Task<IActionResult> GetTaskAddedByCurrentUser()
        //{
        //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    if (userId == null)
        //    {
        //        return Unauthorized("Unauthorized");
        //    }
        //    var tasks = await _taskService.GetAllTaskAddedByCurrentUser(userId);
        //    if (tasks == null)
        //    {
        //        return NotFound("No tasks found");
        //    }
        //    return Ok(new { tasks });
        //}
        [HttpGet("get-task-by-id/{id}")]
        public async Task<IActionResult> GetTaskById(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Unauthorized");
            }
            var task = await _taskService.GetTaskByTaskId(id);
            if(task == null)
            {
                return NotFound("Task not found");
            }
            return Ok(new { task });
        } 

        [HttpPost("update-task")]
        public async Task<IActionResult> EditTaskById([FromBody] UpdateTaskDto dto,[FromQuery] string taskId,[FromQuery] string businessId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("Unauthorized");

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Admin" && userRole != "Manager")
                return Forbid("Only Admin & Manager have edit access");

            var task = await _taskService.GetTaskByTaskId(taskId);
            if (task == null)
                return NotFound("Task not found");

            if (userRole == "Manager" && task.createdBy.Id != userId)
                return Forbid("Managers can only edit their own tasks.");
            User task_assignee = await _userService.GetUSerById(userId);
            var updatedTask = new Models.Task
            {
                Id = taskId,
                type = dto.type,
                tid = task.tid,
                title = dto.title,
                startdate = dto.startdate,
                duedate = dto.duedate,
                deadline = dto.deadline,
                priority = dto.priority,
                description = dto.description,
                assignee = new IdNameModel
                {
                    Id = task_assignee.Id,
                    Name = task_assignee.name,
                },
                attachment = task.attachment,
                subtask = task.subtask,
                businessDetails = new IdNameModel
                {
                    Id = businessId,
                    Name = task.businessDetails.Id
                },
                createdBy = new CreatedByModel
                {
                    Name= task.createdBy.Name,
                    Id = task.createdBy.Id
                }
            };

            var description = $"<strong>Task updated</strong><br/><ul style='padding-left: 16px;'>";

            if (task.title != dto.title)
                description += $"<li><strong>Title:</strong> <em>{task.title}</em> → <em>{dto.title}</em></li>";

            if (task.description != dto.description)
                description += $"<li><strong>Description:</strong> <em>{task.description}</em> → <em>{dto.description}</em></li>";

            if (task.priority != dto.priority)
                description += $"<li><strong>Priority:</strong> <em>{task.priority}</em> → <em>{dto.priority}</em></li>";

            if (task.type != dto.type)
                description += $"<li><strong>Type:</strong> <em>{task.type}</em> → <em>{dto.type}</em></li>";

            if (task.assignee.Id != dto.assignee.Id)
                description += $"<li><strong>Assignee:</strong> <em>{task.assignee.Name}</em> → <em>{dto.assignee.Name}</em></li>";

            if (task.startdate != dto.startdate)
                description += $"<li><strong>Start Date:</strong> <em>{task.startdate:yyyy-MM-dd}</em> → <em>{dto.startdate:yyyy-MM-dd}</em></li>";

            if (task.duedate != dto.duedate)
                description += $"<li><strong>Due Date:</strong> <em>{task.duedate:yyyy-MM-dd}</em> → <em>{dto.duedate:yyyy-MM-dd}</em></li>";

            if (task.deadline != dto.deadline)
                description += $"<li><strong>Deadline:</strong> <em>{task.deadline:yyyy-MM-dd}</em> → <em>{dto.deadline:yyyy-MM-dd}</em></li>";

            description += "</ul>";


            await _taskService.UpdateTask(taskId, updatedTask);

            var userName = User.FindFirst("username")?.Value;
            Console.WriteLine(userName);
            var taskHistory = new TaskHistoryModel
            {
                TargetTask = new IdNameModel
                {
                    Id = taskId,
                    Name = dto.title
                },
                TargetBusiness = new IdNameModel
                {
                    Id = task.businessDetails.Id,
                    Name = task.businessDetails.Name
                },
                CreatedBy = new CreatedByModel
                {
                    Id = userId,
                    Name = userName
                },
                ChangeType = ChangeTypeEnum.Edit,
                Description = description,

            };

            await _taskHistoryService.CreateTaskHistory(taskHistory);

            return Ok("Task updated successfully");
        }

        [HttpPost("add-subtask/{taskId}")]
        public async Task<IActionResult> AddSubTask(AddSubTaskDto dto, string taskId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Unauthorized");
            }

            var subtask = new SubTask
            {
                title = dto.title,
                status = dto.status
            };
            await _taskService.CreateSubTask(subtask, taskId);

            Models.Task task = await _taskService.GetTaskByTaskId(taskId);

            var description = $"<strong>New subtask added:</strong> <span>{dto.title}</span>";

            var userName = User.FindFirst("username")?.Value;
            var taskHistory = new TaskHistoryModel
            {
                TargetTask = new IdNameModel
                {
                    Id = taskId,
                    Name = task.title
                },
                TargetBusiness = new IdNameModel
                {
                    Id = task.businessDetails.Id,
                    Name = task.businessDetails.Name
                },
                CreatedBy = new CreatedByModel
                {
                    Id = userId,
                    Name = userName
                },
                ChangeType = ChangeTypeEnum.Add,
                Description = description,

            };
            await _taskHistoryService.CreateTaskHistory(taskHistory);
            return Ok("Subtask created !");
        }

        [HttpPost("change-subtask-status/{subtaskId}")]
        public async Task<IActionResult> UpdateSubtaskStaus(string subtaskId, [FromQuery] string taskId,[FromQuery] string status)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("Unauthorized");


            Models.Task task = await _taskService.GetTaskByTaskId(taskId);
            if (task == null)
                return NotFound("Task not found");
            var subtask = task.subtask.FirstOrDefault(s => s.Id == subtaskId);
            if (subtask == null)
                return NotFound("Subtask not found");

            var previousStatus = subtask.status;

  
            await _taskService.UpdateSubtaskStaus(subtaskId, status);
            var description = $"<strong>Status of subtask</strong> \"{subtask.title}\" changed from <em>{previousStatus}</em> to <em>{status}</em>";
            var userName = User.FindFirst("username")?.Value;
            var taskHistory = new TaskHistoryModel
            {
                TargetTask = new IdNameModel
                {
                    Id = taskId,
                    Name = task.title
                },
                TargetBusiness = new IdNameModel
                {
                    Id = task.businessDetails.Id,
                    Name = task.businessDetails.Name
                },
                CreatedBy = new CreatedByModel
                {
                    Id = userId,
                    Name = userName
                },
                ChangeType = ChangeTypeEnum.Edit,
                Description = description,
            };

            await _taskHistoryService.CreateTaskHistory(taskHistory);
            return Ok("Subtask status updated");
        }


        [HttpPost("delete-subtask/{subtaskId}")]
        public async Task<IActionResult> DeleteSubtask([FromRoute] string subtaskId, [FromQuery] string taskId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("Unauthorized");

            var task = await _taskService.GetTaskByTaskId(taskId);
            if (task == null)
                return NotFound("Task not found");

            var subtask = task.subtask.FirstOrDefault(s => s.Id == subtaskId);
            if (subtask == null)
                return NotFound("Subtask not found");

            var deletedSubtaskTitle = subtask.title;

            await _taskService.DeleteSubTask(subtaskId);
            var description = $"<strong>Subtask deleted:</strong> \"{deletedSubtaskTitle}\"";
            var userName = User.FindFirst("username")?.Value;
            var taskHistory = new TaskHistoryModel
            {
                TargetTask = new IdNameModel
                {
                    Id = taskId,
                    Name = task.title
                },
                TargetBusiness = new IdNameModel
                {
                    Id = task.businessDetails.Id,
                    Name = task.businessDetails.Name
                },
                CreatedBy = new CreatedByModel
                {
                    Id = userId,
                    Name = userName
                },
                ChangeType = ChangeTypeEnum.Delete,
                Description = description,
            };

            await _taskHistoryService.CreateTaskHistory(taskHistory);

            return Ok("Subtask deleted and history logged");
        }

        [HttpGet("get-tasks-by-business-id/{businessId}")]
        public async Task<IActionResult> GetTasksByBusinessId(string businessId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Unauthorized");
            }
            var tasks = await _taskService.GetTasksByBusinessId(businessId);
            var isCompleted = true;
            return Ok(new { tasks, isCompleted });
        }

        [HttpPost("filter")]
        public async Task<IActionResult> FilterTasks([FromBody] FilterReqDto filter)
        {
            if (string.IsNullOrWhiteSpace(filter.Criteria) || string.IsNullOrWhiteSpace(filter.Value))
            {
                return BadRequest("Both 'criteria' and 'value' are required.");
            }

            var result = await _taskService.FilterTasksAsync(filter.Criteria, filter.Value);
            return Ok(new { filteredData = result });
        }


    }
}
