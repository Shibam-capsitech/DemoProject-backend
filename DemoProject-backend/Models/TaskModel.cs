using DemoProject_backend.Dtos;
using DemoProject_backend.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace DemoProject_backend.Models
{
    public class Task
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
        public TaskPriority Priority { get; set; }
        public string Description { get; set; }
        public IdNameModel Assignee { get; set; }
        public string Attachment { get; set; }
        public List<SubTask>? Subtask { get; set; }
        public IdNameModel BusinessDetails { get; set; }
        public CreatedByModel CreatedBy { get; set; }
        public bool? IsActive { get; set; } = true;
    }

    public class SubTask
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
    }
}

