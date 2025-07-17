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

            if (lastTask != null && !string.IsNullOrEmpty(lastTask.Tid))
            {
                string lastNumberPart = lastTask.Tid.Replace("TID-", "");
                if (int.TryParse(lastNumberPart, out int parsedNumber))
                {
                    nextNumber = parsedNumber + 1;
                }
            }
            string newCustomId = $"TID-{nextNumber.ToString("D3")}";
            var userName = User.FindFirst("username")?.Value;
            Console.Write(".................");
            Console.WriteLine(dto.Assignee.Id);
            Console.WriteLine(userName);
            User assignee = await _userService.GetUSerById(dto.Assignee.Id);
            Console.WriteLine("Assignee ID: " + assignee.Id);  
            Console.WriteLine("Assignee Name: " + assignee.Username);  

            var task = new Models.Task
            {
                Tid= newCustomId,
                Type = dto.Type,
                Title = dto.Title,
                Startdate = dto.Startdate,
                Duedate = dto.Duedate,
                Deadline = dto.Deadline,
                Priority = dto.Priority,
                Description = dto.Description,
                Assignee = new IdNameModel
                {
                    Id = assignee.Id,
                    Name = assignee.Username,
                },
                Attachment = fileResult.SecureUrl.ToString(),
                BusinessDetails = new IdNameModel
                {
                    Id = dto.BusinessDetails.Id,
                    Name = dto.BusinessDetails.Name,
                },
                CreatedBy = new CreatedByModel
                {
                    Id = userId,
                    Name = userName,
                },
                Subtask = new List<SubTask>(),
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
                    Name = task.Title,
                },
                TargetBusiness = new IdNameModel
                {
                  Id = task.BusinessDetails.Id,
                  Name = task.BusinessDetails.Name,
                },
                CreatedBy = new CreatedByModel
                {
                    Id = userId,
                    Name = userName
                },
                ChangeType = ChangeTypeEnum.Add,
                Description = $"<strong>New task created</strong> — <span>Title: {dto.Title}</span>"
            };

            await _taskHistoryService.CreateTaskHistory(taskHistory);


            var subtask = new SubTask
            {
                Title = "Gather information from client",
                Status = "Waiting",
            };
            await _taskService.CreateSubTask(subtask, task.Id);

            return Ok("New task created");
        }


        [HttpGet("get-all-task")]
        public async Task<IActionResult> GetAllTasks([FromQuery] string? criteria, [FromQuery] string? value, [FromQuery] string? search)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized("Unauthorized");

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            List<Models.Task> tasks;

            if (userRole == "Admin")
            {
                tasks = await _taskService.GetAllTaskForAdmin();
            }
            else if (userRole == "Manager" || userRole == "Staff")
            {
                tasks = await _taskService.GetAllTaskAddedByCurrentUser(userId);
            }
            else
            {
                return BadRequest("Cannot Perform Get Task Action");
            }

            if (tasks == null || !tasks.Any())
                return NotFound("No tasks found");

            if (!string.IsNullOrWhiteSpace(criteria) && !string.IsNullOrWhiteSpace(value))
            {
                tasks = await _taskService.FilterTasksAsync(criteria, value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();
                tasks = tasks
                    .Where(t =>
                        (!string.IsNullOrEmpty(t.Title) && t.Title.ToLower().Contains(lower)) ||
                        (!string.IsNullOrEmpty(t.Tid) && t.Tid.ToLower().Contains(lower)))
                    .ToList();
            }

            return Ok(new { tasks });
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
            var userName = User.FindFirst("username")?.Value;
            if (userRole != "Admin" && userRole != "Manager")
                return Forbid("Only Admin & Manager have edit access");

            var task = await _taskService.GetTaskByTaskId(taskId);
            if (task == null)
                return NotFound("Task not found");

            if (task.Assignee.Id != userId && task.CreatedBy.Id != userId && userRole != "Admin")
                return Forbid($"{userName} doesnt have edit access on this !");
            User task_assignee = await _userService.GetUSerById(userId);
            var updatedTask = new Models.Task
            {
                Id = taskId,
                Type = dto.Type,
                Tid = task.Tid,
                Title = dto.Title,
                Startdate = dto.Startdate,
                Duedate = dto.Duedate,
                Deadline = dto.Deadline,
                Priority = dto.Priority,
                Description = dto.Description,
                Assignee = new IdNameModel
                {
                    Id = dto.Assignee.Id,
                    Name = dto.Assignee.Name,
                },
                Attachment = task.Attachment,
                Subtask = task.Subtask,
                BusinessDetails = new IdNameModel
                {
                    Id = businessId,
                    Name = task.BusinessDetails.Name
                },
                CreatedBy = new CreatedByModel
                {
                    Name= task.CreatedBy.Name,
                    Id = task.CreatedBy.Id
                }
            };

            var description = $"<strong>Task updated</strong><br/><ul style='padding-left: 16px;'>";

            if (task.Title != dto.Title)
                description += $"<li><strong>Title:</strong> <em>{task.Title}</em> → <em>{dto.Title}</em></li>";

            if (task.Description != dto.Description)
                description += $"<li><strong>Description:</strong> <em>{task.Description}</em> → <em>{dto.Description}</em></li>";

            if (task.Priority != dto.Priority)
                description += $"<li><strong>Priority:</strong> <em>{task.Priority}</em> → <em>{dto.Priority}</em></li>";

            if (task.Type != dto.Type)
                description += $"<li><strong>Type:</strong> <em>{task.Type}</em> → <em>{dto.Type}</em></li>";

            if (task.Assignee.Id != dto.Assignee.Id)
                description += $"<li><strong>Assignee:</strong> <em>{task.Assignee.Name}</em> → <em>{dto.Assignee.Name}</em></li>";

            if (task.Startdate != dto.Startdate)
                description += $"<li><strong>Start Date:</strong> <em>{task.Startdate:yyyy-MM-dd}</em> → <em>{dto.Startdate:yyyy-MM-dd}</em></li>";

            if (task.Duedate != dto.Duedate)
                description += $"<li><strong>Due Date:</strong> <em>{task.Duedate:yyyy-MM-dd}</em> → <em>{dto.Duedate:yyyy-MM-dd}</em></li>";

            if (task.Deadline != dto.Deadline)
                description += $"<li><strong>Deadline:</strong> <em>{task.Deadline:yyyy-MM-dd}</em> → <em>{dto.Deadline:yyyy-MM-dd}</em></li>";

            description += "</ul>";


            await _taskService.UpdateTask(taskId, updatedTask);
            Console.WriteLine(userName);
            var taskHistory = new TaskHistoryModel
            {
                TargetTask = new IdNameModel
                {
                    Id = taskId,
                    Name = dto.Title
                },
                TargetBusiness = new IdNameModel
                {
                    Id = task.BusinessDetails.Id,
                    Name = task.BusinessDetails.Name
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

        [HttpPost("delete-task/{taskId}")]
        public async Task<IActionResult> DeleteTaskById(string taskId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("Unauthorized");

            var task = await _taskService.GetTaskByTaskId(taskId);
            if (task == null)
                return NotFound("Task not found");

            await _taskService.DisableTask(taskId);

            var description = $"<strong>Task deleted:</strong> \"{task.Title}\"";
            var userName = User.FindFirst("username")?.Value;
            var taskHistory = new TaskHistoryModel
            {
                TargetTask = new IdNameModel
                {
                    Id = taskId,
                    Name = task.Title
                },
                TargetBusiness = new IdNameModel
                {
                    Id = task.BusinessDetails.Id,
                    Name = task.BusinessDetails.Name
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
            return Ok("Task deleted !");
        }

        [HttpPost("add-subtask/{taskId}")]
        public async Task<IActionResult> AddSubTask(AddSubTaskDto dto, string taskId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Models.Task task = await _taskService.GetTaskByTaskId(taskId);

            if (userId == null)
            {
                return Unauthorized("Unauthorized");
            }

            var subtask = new SubTask
            {
                Title = dto.Title,
                Status = dto.Status
            };

            if(task.Subtask.Count > 0)
            {
                if (task.IsCompleted)
                {
                    task.IsCompleted = false;
                    await _taskService.TaskCompletionToggle(task);
                }
            }

            await _taskService.CreateSubTask(subtask, taskId);



            var description = $"<strong>New subtask added:</strong> <span>{dto.Title}</span>";

            var userName = User.FindFirst("username")?.Value;
            var taskHistory = new TaskHistoryModel
            {
                TargetTask = new IdNameModel
                {
                    Id = taskId,
                    Name = task.Title
                },
                TargetBusiness = new IdNameModel
                {
                    Id = task.BusinessDetails.Id,
                    Name = task.BusinessDetails.Name
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
            var subtask = task.Subtask.FirstOrDefault(s => s.Id == subtaskId);
            if (subtask == null)
                return NotFound("Subtask not found");

            var previousStatus = subtask.Status;

  
            await _taskService.UpdateSubtaskStaus(subtaskId, status);
            task = await _taskService.GetTaskByTaskId(taskId);

            var activeSubtasks = task.Subtask.Where(st => st.IsActive).ToList();

            if (activeSubtasks.Count > 0 && activeSubtasks.All(st => st.Status == "Completed"))
            {
                if (!task.IsCompleted)
                {
                    task.IsCompleted = true;
                    await _taskService.TaskCompletionToggle(task);
                }
            }


            var description = $"<strong>Status of subtask</strong> \"{subtask.Title}\" changed from <em>{previousStatus}</em> to <em>{status}</em>";
            var userName = User.FindFirst("username")?.Value;
            var taskHistory = new TaskHistoryModel
            {
                TargetTask = new IdNameModel
                {
                    Id = taskId,
                    Name = task.Title
                },
                TargetBusiness = new IdNameModel
                {
                    Id = task.BusinessDetails.Id,
                    Name = task.BusinessDetails.Name
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

            var subtask = task.Subtask.FirstOrDefault(s => s.Id == subtaskId);
            if (subtask == null)
                return NotFound("Subtask not found");

            var deletedSubtaskTitle = subtask.Title;

            await _taskService.DisableSubTaskAsync(subtaskId);

            task = await _taskService.GetTaskByTaskId(taskId);

            var activeSubtasks = task.Subtask.Where(st => st.IsActive).ToList();

            if (activeSubtasks.Count > 0 && activeSubtasks.All(st => st.Status == "Completed"))
            {
                if (!task.IsCompleted)
                {
                    task.IsCompleted = true;
                    await _taskService.TaskCompletionToggle(task);
                }
            }

            var description = $"<strong>Subtask deleted:</strong> \"{deletedSubtaskTitle}\"";
            var userName = User.FindFirst("username")?.Value;
            var taskHistory = new TaskHistoryModel
            {
                TargetTask = new IdNameModel
                {
                    Id = taskId,
                    Name = task.Title
                },
                TargetBusiness = new IdNameModel
                {
                    Id = task.BusinessDetails.Id,
                    Name = task.BusinessDetails.Name
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

        [HttpGet("get-tasks-creation-by-date-stats")]
        public async Task<IActionResult> GetTaskCreationByDateStats()
        {
            var result = await _taskService.TaskCreationStats();
            return Ok(result);

        }


        [HttpGet("get-tasks-creation-by-user-stats")]
        public async Task<IActionResult> GetTaskCreationByUserStats()
        {
            var result = await _taskService.TaskCreatedCountByUserPeriods();
            return Ok(result);

        }

        //[HttpGet("analytics/completion-summary")]
        //public async Task<IActionResult> GetTaskCompletionSummary()
        //{
        //    var now = DateTime.UtcNow;
        //    var startOfCurrentMonth = new DateTime(now.Year, now.Month, 1);
        //    var startOfPreviousMonth = startOfCurrentMonth.AddMonths(-1);
        //    var endOfPreviousMonth = startOfCurrentMonth.AddDays(-1);

        //    var allTasks = await _taskService.GetAllTasks();

        //    var completedTasks = allTasks.Where(t => t.IsCompleted && t.CompletedDate != null).ToList();

        //    var incompleteTasks = allTasks.Where(t => !t.IsCompleted).ToList();


        //    var previousMonthCompleted = completedTasks
        //        .Count(t => t.CompletedDate >= startOfPreviousMonth && t.CompletedDate <= endOfPreviousMonth);

        //    var currentMonthCompleted = completedTasks
        //        .Count(t => t.CompletedDate >= startOfCurrentMonth && t.CompletedDate <= now);

        //    var totalCompleted = completedTasks.Count;
        //    var totalIncomplete = incompleteTasks.Count;

        //    return Ok(new
        //    {
        //        PreviousMonthCompleted = previousMonthCompleted,
        //        CurrentMonthCompleted = currentMonthCompleted,
        //        TotalCompleted = totalCompleted,
        //        TotalIncomplete = totalIncomplete
        //    });
        //}

    }
}
