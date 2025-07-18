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

        /// <summary>
        /// Task custom ID
        /// </summary>
        public string Tid { get; set; }

        /// <summary>
        /// Task type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Task title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Task start date 
        /// </summary>
        public DateTime Startdate { get; set; }

        /// <summary>
        /// Task due date 
        /// </summary>
        public DateTime Duedate { get; set; }

        /// <summary>
        /// Task dealine  
        /// </summary>
        public DateTime Deadline { get; set; }

        /// <summary>
        /// task priority 
        /// </summary>
        public TaskPriority Priority { get; set; }

        /// <summary>
        /// A short description about the task 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// To whom the task is assigned 
        /// </summary>
        public IdNameModel Assignee { get; set; }

        /// <summary>
        /// If there any attachment for the task
        /// </summary>
        public string Attachment { get; set; }

        /// <summary>
        /// Steps to complete a task
        /// </summary>
        public List<SubTask>? Subtask { get; set; }

        /// <summary>
        /// Under which business the task is assinged 
        /// </summary>
        public IdNameModel BusinessDetails { get; set; }

        /// <summary>
        /// Task created by which user
        /// </summary>
        public CreatedByModel CreatedBy { get; set; }

        /// <summary>
        /// A boolean field for soft delete
        /// </summary>
        public bool?  IsActive { get; set; } = true;

        /// <summary>
        /// A field to check if the task is comepletd or not 
        /// </summary>
        public bool IsCompleted { get; set; } = false;

        /// <summary>
        /// Date of completion of a task
        /// </summary>
        [BsonIgnoreIfDefault]
        public DateTime CompletionDate { get; set; }

        /// <summary>
        /// User who completed this task
        /// </summary>
        [BsonIgnoreIfDefault]
        public IdNameModel CompletedBy { get; set; }
    }

    public class SubTask
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>
        /// subtask title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// subtask status 
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// a field for subtask soft deletion
        /// </summary>
        public bool IsActive { get; set; } = true;
        
    }
}

