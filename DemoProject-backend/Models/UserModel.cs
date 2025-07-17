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

        /// <summary>
        /// User full name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// User's username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// User's email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// User's email
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// User's email
        /// </summary>
        [BsonRepresentation(BsonType.String)]
        public UserRole Role { get; set; }

        /// <summary>
        /// User's address
        /// </summary>
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public AddressModel? Address { get; set; }

        /// <summary>
        /// User's craetion date
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

