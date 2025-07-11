using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DemoProject_backend.Dtos
{
    public class TaskHistoryDto
    {
        public string Id { get; set; }
        public string TaskId { get; set; }
        public string BusinessId { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime TimeStamp { get; set; }
        public List<FieldChangeDto> Changes { get; set; }
        public string? ChangeType { get; set; }
    }

    public class FieldChangeDto
    {
        public string? field { get; set; }
        public string? previous
        { get; set; }

        [BsonElement("new")]
        public string? newval { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? subtaskId { get; set; }

        [BsonElement("isChangeRegardingTask")]
        public bool? IsChangeRegardingTask { get; set; }

        [BsonElement("isChangeRegardingSubTask")]
        public bool? IsChangeRegardingSubTask { get; set; }
    }

        public class GetTaskHistoryDto
        {

        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public DateTime timestamp { get; set; }
        public string BusinessId { get; set; }
        public List<FieldChangeDto> changes { get; set; }
        public TaskInfo taskdetails { get; set; }
        public UserInfo userdetails { get; set; }

        public string? ChangeType { get; set; }

    }

        public class TaskInfo
        {
            public string title { get; set; }
            public string description { get; set; }
            public string type { get; set; }
        }

        public class UserInfo
        {
            public string name { get; set; }
            public string email { get; set; }
            public string role { get; set; }
        }

        public class TaskHistory
        {
            public string Id { get; set; }
            public IdNameModel Target { get; set; }
            public CreatedBy CreatedBy { get; set; }
            public string Description{ get; set; }
            public NoteType ChangeType { get; set; }
        }

        public class IdNameModel
        {
            /// <summary>
            /// Client/Task Id
            /// </summary>
            [BsonId]
            [BsonRepresentation(BsonType.ObjectId)]
            public string Id { get; set; }
            /// <summary>
            /// Client/Task Name
            /// </summary>
            public string Name { get; set; }
        }
        public class CreatedBy
        {
            /// <summary>
            /// User Id
            /// </summary>
            [BsonId]
            [BsonRepresentation(BsonType.ObjectId)]
            public string Id { get; set; }
            /// <summary>
            /// User Name
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// Created Date
            /// </summary>
            public DateTime Date { get; set; }
        }

        public enum NoteType
        {
        [Display(Name = "AJDgxaSJ")]
            Unknown,
            Add,
            Edit,
            Delete,
        }

}
