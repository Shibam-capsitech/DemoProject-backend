using DemoProject_backend.Enums;
using DemoProject_backend.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DemoProject_backend.Dtos
{
    public class CreateTaskDto
    {
        public string type { get; set; }
        public string title { get; set; }
        public DateTime startdate { get; set; }
        public DateTime duedate { get; set; }
        public DateTime deadline { get; set; }
        public TaskPriority priority { get; set; }
        public string description { get; set; }
        public IdNameDto assignee { get; set; }

        [BsonElement("businessdetails")]
        public IdNameDto businessDetails { get; set; }
    }

    public class UpdateTaskDto
    {
        public string type { get; set; }
        public string title { get; set; }
        public DateTime startdate { get; set; }
        public DateTime duedate { get; set; }
        public DateTime deadline { get; set; }
        public TaskPriority priority { get; set; }
        public string description { get; set; }
        public IdNameDto assignee { get; set; }
    }

    public class GetAllTaskDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string tid { get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public DateTime startdate { get; set; }
        public DateTime duedate { get; set; }
        public DateTime deadline { get; set; }
        public int priority { get; set; }
        public string description { get; set; }
        public IdNameDto assignee { get; set; }
        public string attachment { get; set; }
        public List<SubTask> subtask { get; set; }
        public IdNameDto businessdetails { get; set; }
        public CreatedByDto createdby { get; set; }
    }

    public class GetFilteredTaskDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string tid { get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public DateTime startdate { get; set; }
        public DateTime duedate { get; set; }
        public DateTime deadline { get; set; }
        public int priority { get; set; }
        public string description { get; set; }
        public IdNameModel assignee { get; set; }
        public string attachment { get; set; }
        public List<SubTask> subtask { get; set; }
        public IdNameDto businessdetails { get; set; }
        public CreatedByDto createdby { get; set; }
        public string username { get; set; }
        public string email { get; set; }
    }

    public class AddSubTaskDto
    {
        public string title { get; set; }
        public string status { get; set; }
    }
}
