using DemoProject_backend.Enums;
using DemoProject_backend.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DemoProject_backend.Dtos
{
    public class CreateTaskDto
    {
        public string type { get; set; }
        public string businessName { get; set; }
        public string title { get; set; }
        public DateTime startDate { get; set; }
        public DateTime dueDate { get; set; }
        public DateTime deadline { get; set; }
        public TaskPriority priority { get; set; }
        public string description { get; set; }
        public string assignee { get; set; }
        public string businessId { get; set; }
    }
    public class UpdateTaskDto
    {
        public string type { get; set; }
        public string title { get; set; }
        public DateTime startDate { get; set; }
        public DateTime dueDate { get; set; }
        public DateTime deadline { get; set; }
        public TaskPriority priority { get; set; }
        public string description { get; set; }
        public string assignee { get; set; }
    }

    public class GetAllTaskDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        public string type { get; set; }

        public string businessname { get; set; }

        public string title { get; set; }

        public DateTime startdate { get; set; }

        public DateTime duedate { get; set; }

        public DateTime deadline { get; set; }

        public int priority { get; set; }

        public string description { get; set; }

        public string assignee { get; set; }

        public string attachment { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string BusinessId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        public User userDetails { get; set; }
        public Business businessDetails { get; set;}
        public DateTime createdAt { get; set; }

        public List<SubTask>  subtask  { get; set; }
    }

    public class AddSubTaskDto
    {
        public string title { get; set;}
        public string status { get; set;}
    }

}
