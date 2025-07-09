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
        public List<FieldChange> Changes { get; set; }
    }

    public class FieldChange
    {
        [BsonElement("field")]
        public string? Field { get; set; }

        [BsonElement("previous")]
        public string? PreviousValue { get; set; }

        [BsonElement("new")]
        public string? NewValue { get; set; }
    }
}
