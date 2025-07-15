using DemoProject_backend.Enums;
using DemoProject_backend.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DemoProject_backend.Dtos
{
    public class CreateTaskDto
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public DateTime Startdate { get; set; }
        public DateTime Duedate { get; set; }
        public DateTime Deadline { get; set; }
        public TaskPriority Priority { get; set; }
        public string Description { get; set; }
        public IdNameDto Assignee { get; set; }
        public IdNameDto BusinessDetails { get; set; }
    }

    public class UpdateTaskDto
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public DateTime Startdate { get; set; }
        public DateTime Duedate { get; set; }
        public DateTime Deadline { get; set; }
        public TaskPriority Priority { get; set; }
        public string Description { get; set; }
        public IdNameDto Assignee { get; set; }
    }

    public class GetAllTaskDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Tid { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public DateTime Startdate { get; set; }
        public DateTime Duedate { get; set; }
        public DateTime Deadline { get; set; }
        public int Priority { get; set; }
        public string Description { get; set; }
        public IdNameDto Assignee { get; set; }
        public string Attachment { get; set; }
        public List<SubTask> Subtask { get; set; }
        public IdNameDto Businessdetails { get; set; }
        public CreatedByDto CreatedBy { get; set; }
    }

    public class GetFilteredTaskDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Tid { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public DateTime Startdate { get; set; }
        public DateTime Duedate { get; set; }
        public DateTime Deadline { get; set; }
        public int Priority { get; set; }
        public string Description { get; set; }
        public IdNameModel Assignee { get; set; }
        public string Attachment { get; set; }
        public List<SubTask> Subtask { get; set; }
        public IdNameDto Businessdetails { get; set; }
        public CreatedByDto Createdby { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }

    public class AddSubTaskDto
    {
        public string Title { get; set; }
        public string Status { get; set; }
    }
}
