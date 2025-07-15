using DemoProject_backend.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace DemoProject_backend.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        [BsonRepresentation(BsonType.String)]
        public UserRole Role { get; set; }

        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public AddressModel? Address { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

