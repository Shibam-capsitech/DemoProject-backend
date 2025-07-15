using DemoProject_backend.Dtos;
using DemoProject_backend.Enums;
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
        public IdNameModel TargetTask { get; set; }
        public IdNameModel TargetBusiness { get; set; }
        public CreatedByModel CreatedBy { get; set; }
        public string Description { get; set; }
        public ChangeTypeEnum ChangeType { get; set; }
    }

}
