using DemoProject_backend.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace DemoProject_backend.Models
{
    public class Business
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>
        /// custom Id for a busieness
        /// </summary>
        [BsonElement("BId")]
        public string BId { get; set; }

        public string Type { get; set; }
        /// <summary>
        /// Business Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Address of business 
        /// </summary>
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public AddressModel? Address { get; set; }

        
        /// <summary>
        /// business created by which user
        /// </summary>
        [BsonElement("CreatedBy")]
        public CreatedByModel CreatedBy { get; set; }

        /// <summary>
        /// a field for soft delete of business 
        /// </summary>
        public bool IsActive { get; set; } = true;

    }


}

