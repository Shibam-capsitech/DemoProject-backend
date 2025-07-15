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

        [BsonElement("CreatedBy")]
        public CreatedByModel CreatedBy { get; set; }
        public bool IsActive { get; set; }

    }


}

