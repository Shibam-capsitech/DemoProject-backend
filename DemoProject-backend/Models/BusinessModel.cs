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

        [BsonElement("bid")]
        public string bId { get; set; }

        public string type { get; set; }
        /// <summary>
        /// Business Name
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Address of business 
        /// </summary>
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public AddressModel? address { get; set; }

        [BsonElement("createdby")]
        public CreatedByModel createdBy { get; set; }

    }


}

