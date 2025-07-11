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

        [BsonElement("tid")]
        public string TId { get; set; }

        [BsonElement("type")]
        public string Type { get; set; }

        [BsonElement("businessname")]
        public string BusinessName { get; set; }

        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("startdate")]
        public DateTime StartDate { get; set; }

        [BsonElement("duedate")]
        public DateTime DueDate { get; set; }

        [BsonElement("deadline")]
        public DateTime Deadline { get; set; }

        [BsonElement("priority")]
        public TaskPriority Priority { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("assignee")]
        public string Assignee { get; set; }

        [BsonElement("attachment")]
        public string Attachment { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string BusinessId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        [BsonElement("subtask")]
        public List<SubTask>? SubTask { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public IdNameModel Business { get; set; }
    }

    public class SubTask
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("Status")]
        public string Status { get; set; } //do this as enum
    }
}

