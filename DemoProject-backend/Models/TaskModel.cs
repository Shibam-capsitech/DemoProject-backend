using DemoProject_backend.Dtos;
using DemoProject_backend.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace DemoProject_backend.Models
{
    public class Task
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
        public TaskPriority priority { get; set; }
        public string description { get; set; }
        public IdNameModel assignee { get; set; }
        public string attachment { get; set; }
        public List<SubTask>? subtask { get; set; }

        [BsonElement("businessdetails")]
        public IdNameModel businessDetails { get; set; }

        [BsonElement("createdby")]
        public CreatedByModel createdBy { get; set; } 

    }

    public class SubTask
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string title { get; set; }
        public string status { get; set; } 
    }
}

