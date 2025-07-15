using DemoProject_backend.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DemoProject_backend.Dtos
{
    //public class TaskHistoryDto
    //{
    //    public string Id { get; set; }
    //    public string TaskId { get; set; }
    //    public string BusinessId { get; set; }
    //    public string UpdatedBy { get; set; }
    //    public DateTime TimeStamp { get; set; }
    //    public List<FieldChangeDto> Changes { get; set; }
    //    public string? ChangeType { get; set; }
    //}

    //public class FieldChangeDto
    //{
    //    public string? field { get; set; }
    //    public string? previous
    //    { get; set; }

    //    [BsonElement("new")]
    //    public string? newval { get; set; }

    //    [BsonRepresentation(BsonType.ObjectId)]
    //    public string? subtaskId { get; set; }

    //    [BsonElement("isChangeRegardingTask")]
    //    public bool? IsChangeRegardingTask { get; set; }

    //    [BsonElement("isChangeRegardingSubTask")]
    //    public bool? IsChangeRegardingSubTask { get; set; }
    //}

    public class GetTaskHistoryDto
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public IdNameDto Targetedtask { get; set; }     
        public IdNameDto Targetedbusiness { get; set; }      
        public CreatedByDto CreatedBy { get; set; }
        public string Description { get; set; }
        public ChangeTypeEnum ChangeType { get; set; }
    }


    //    public class TaskInfo
    //    {
    //        public string title { get; set; }
    //        public string description { get; set; }
    //        public string type { get; set; }
    //    }

    //    public class UserInfo
    //    {
    //        public string name { get; set; }
    //        public string email { get; set; }
    //        public string role { get; set; }
    //    }

    public class TaskHistoryDto
    {
        public string Id { get; set; }
        public IdNameDto Target { get; set; }
        public CreatedByDto CreatedBy { get; set; }

        /// <summary>
        /// Description of the updated task/client
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Which type of change 
        /// </summary>
        public ChangeTypeEnum ChangeType { get; set; }
    }


}
