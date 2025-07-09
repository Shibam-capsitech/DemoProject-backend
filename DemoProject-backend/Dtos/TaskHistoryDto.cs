using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

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
    }

    public class FieldChangeDto
    {
        public string field { get; set; }
        public string previous
        { get; set; }

        [BsonElement("new")]
        public string newval { get; set; }
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
   

}
