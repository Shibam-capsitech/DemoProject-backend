using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace DemoProject_backend.Models
{
    public class TaskHistoryModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string TaskId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string BusinessId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UpdatedBy { get; set; }

        [BsonElement("timestamp")]
        public DateTime TimeStamp { get; set; }

        [BsonElement("changes")]
        public String Changes { get; set; }

        [BsonElement("ChangeType")]
        public string? ChangeType { get; set; }
    }

    public class FieldChange
    {
        
        [BsonElement("field")]
        public string? Field { get; set; }

        [BsonElement("previous")]
        public string? PreviousValue { get; set; }

        [BsonElement("new")]
        public string? NewValue { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("subtaskId")]
        public string? SubTaskId { get; set; }

        [BsonElement("isChangeRegardingTask")]
        public bool? IsChangeRegardingTask { get; set; }

        [BsonElement("isChangeRegardingSubTask")]
        public bool? IsChangeRegardingSubTask { get; set; }

    }
}
