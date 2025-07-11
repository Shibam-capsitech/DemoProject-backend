using DemoProject_backend.Dtos;
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

            if (lastTask != null && !string.IsNullOrEmpty(lastTask.TId))
            {
                string lastNumberPart = lastTask.TId.Replace("TID-", "");
                if (int.TryParse(lastNumberPart, out int parsedNumber))
                {
                    nextNumber = parsedNumber + 1;
                }
            }
            string newCustomId = $"TID-{nextNumber.ToString("D3")}";
            var task = new Models.Task
            {
                TId= newCustomId,
                Type = dto.type,
                Title = dto.title,
                BusinessName = dto.businessName,
                StartDate = dto.startDate,
                DueDate = dto.dueDate,
                Deadline = dto.deadline,
                Description = dto.description,
                Assignee = dto.assignee,
                Attachment = fileResult.SecureUrl.ToString(),
                UserId = userId,
                BusinessId = dto.businessId,
                SubTask = new List<SubTask>(),
            };
            await _taskService.CreateTask(task);

            var changes = new List<FieldChange>
                {
                 new FieldChange
                 {
                    Field = "Task Created",
                    PreviousValue = null,
                    NewValue = $"New task created: {dto.title}",
                    IsChangeRegardingSubTask = false,
                    IsChangeRegardingTask= true
                  }
                  };
            var taskHistory = new TaskHistoryModel
            {
                TaskId = task.Id,
                BusinessId = dto.businessId,
                UpdatedBy = userId,
                TimeStamp = DateTime.UtcNow,
                Changes = changes,
                ChangeType = "Create"
            };
            await _taskHistoryService.CreateTaskHistory(taskHistory);

            var subtask = new SubTask
            {
                Title ="Gather information from client",
                Status= "Waiting",
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

            if (userRole == "Manager" && task.UserId != userId)
                return Forbid("Managers can only edit their own tasks.");

            var updatedTask = new Models.Task
            {
                Id = taskId,
                Type = dto.type,
                TId = task.TId,
                BusinessName = task.BusinessName,
                Title = dto.title,
                StartDate = dto.startDate,
                DueDate = dto.dueDate,
                Deadline = dto.deadline,
                Priority = dto.priority,
                Description = dto.description,
                Assignee = dto.assignee,
                BusinessId = task.BusinessId,
                UserId = task.UserId,
                Attachment = task.Attachment,
                SubTask = task.SubTask,
            };
            var changes = new List<FieldChange>();
            var description = $"Details updated :- <br/>";
            if (task.Title != dto.title)
                description += $"<strong>Name</strong> : Updated from {task.Title} to {dto.title}<br/>";
                changes.Add(new FieldChange { Field = "title", PreviousValue = task.Title, NewValue = dto.title });

            if (task.Description != dto.description)
                description += $"Description updated from {task.Description} to {dto.description}<br/>";
                changes.Add(new FieldChange { Field = "description", PreviousValue = task.Description, NewValue = dto.description });

            if (task.Priority != dto.priority)
                changes.Add(new FieldChange { Field = "priority", PreviousValue = task.Priority.ToString(), NewValue = dto.priority.ToString() });

            if (task.Type != dto.type)
                changes.Add(new FieldChange { Field = "type", PreviousValue = task.Type, NewValue = dto.type });

            if (task.Assignee != dto.assignee)
                changes.Add(new FieldChange { Field = "assignee", PreviousValue = task.Assignee, NewValue = dto.assignee });

            if (task.StartDate != dto.startDate)
                changes.Add(new FieldChange { Field = "startDate", PreviousValue = task.StartDate.ToString("s"), NewValue = dto.startDate.ToString("s") });

            if (task.DueDate != dto.dueDate)
                changes.Add(new FieldChange { Field = "dueDate", PreviousValue = task.DueDate.ToString("s"), NewValue = dto.dueDate.ToString("s") });

            if (task.Deadline != dto.deadline)
                changes.Add(new FieldChange { Field = "deadline", PreviousValue = task.Deadline.ToString("s"), NewValue = dto.deadline.ToString("s") });

            await _taskService.UpdateTask(taskId, updatedTask);
            if (changes.Count > 0)
            {
                var taskHistory = new TaskHistoryModel
                {
                    TaskId = taskId,
                    BusinessId= businessId,
                    UpdatedBy = userId,
                    TimeStamp = DateTime.UtcNow,
                    Changes = description,
                     ChangeType = "Update"
                };

                await _taskHistoryService.CreateTaskHistory(taskHistory);
            }

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
                Title = dto.title,
                Status = dto.status
            };
            await _taskService.CreateSubTask(subtask, taskId);

            Models.Task task = await _taskService.GetTaskByTaskId(taskId);
            var changes = new List<FieldChange>
                {
                 new FieldChange
                 {
                    Field = "SubTask Created",
                     PreviousValue = null,
                      NewValue = $"New sub task created: {dto.title}",
                      SubTaskId = subtask.Id,
                      IsChangeRegardingSubTask = true,
                      IsChangeRegardingTask= false
                  }
                  };
            var taskHistory = new TaskHistoryModel
            {
                TaskId = taskId,
                BusinessId = task.BusinessId,
                UpdatedBy = userId,
                TimeStamp = DateTime.UtcNow,
                Changes = changes,
                ChangeType = "Create"
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
            var subtask = task.SubTask.FirstOrDefault(s => s.Id == subtaskId);
            if (subtask == null)
                return NotFound("Subtask not found");

            var previousStatus = subtask.Status;

  
            await _taskService.UpdateSubtaskStaus(subtaskId, status);

            var changes = new List<FieldChange>
            {
             new FieldChange
             {
            Field = "Subtask Status",
            PreviousValue = previousStatus,
            NewValue = status,
            SubTaskId = subtaskId,
            IsChangeRegardingSubTask = true,
            IsChangeRegardingTask = false
             }
             };

            var taskHistory = new TaskHistoryModel
            {
                TaskId = taskId,
                BusinessId = task.BusinessId,
                UpdatedBy = userId,
                TimeStamp = DateTime.UtcNow,
                Changes = changes,
                 ChangeType = "Update"
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

            var subtask = task.SubTask.FirstOrDefault(s => s.Id == subtaskId);
            if (subtask == null)
                return NotFound("Subtask not found");

            var deletedSubtaskTitle = subtask.Title;

            await _taskService.DeleteSubTask(subtaskId);
            var changes = new List<FieldChange>
            {
              new FieldChange
               {
                Field = "Subtask Deleted",
                PreviousValue = $"Subtask: {deletedSubtaskTitle}",
                NewValue = "Deleted",
                SubTaskId = subtaskId,
                IsChangeRegardingSubTask = true,
                IsChangeRegardingTask = false,
                }
               };

            var taskHistory = new TaskHistoryModel
            {
                TaskId = taskId,
                BusinessId = task.BusinessId,
                UpdatedBy = userId,
                TimeStamp = DateTime.UtcNow,
                Changes = changes,
                ChangeType = "Delete"
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
