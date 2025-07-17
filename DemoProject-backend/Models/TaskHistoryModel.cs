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

        /// <summary>
        /// History is for which task
        /// </summary>
        public IdNameModel TargetTask { get; set; }

        /// <summary>
        /// history belong to the task under which business
        /// </summary>
        public IdNameModel TargetBusiness { get; set; }

        /// <summary>
        /// history created by which user
        /// </summary>
        public CreatedByModel CreatedBy { get; set; }

        /// <summary>
        /// a descrioption about the history
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Which kind of hsitory add , delete, update, unnkown
        /// </summary>
        public ChangeTypeEnum ChangeType { get; set; }
    }

}
