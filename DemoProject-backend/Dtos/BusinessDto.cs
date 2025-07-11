using DemoProject_backend.Enums;
using DemoProject_backend.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DemoProject_backend.Dtos
{
    public class CreateBusinessDto
    {
        public string type { get; set; }
        public string name { get; set; }
        public string building { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string state { get; set; }
        public string postcode { get; set; }
    }

public class GetAllBusinessesDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string bId { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string building { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string state { get; set; }
        public string postcode { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        public DateTime createdAt { get; set; }
        public User userDetails { get; set; }
    }

    public class UpdateBusinessDto
    {
        public string type { get; set; }
        public string name { get; set; }
        public string building { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string state { get; set; }
        public string postcode { get; set; }
    }

    public class GetFilteredBusinessDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string bId { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string building { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string state { get; set; }
        public string postcode { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        public DateTime createdAt { get; set; }
        public User userDetails { get; set; }

        public string username { get; set; }
        public string email { get; set; }
    }
}
