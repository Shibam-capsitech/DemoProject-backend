using DemoProject_backend.Enums;
using DemoProject_backend.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DemoProject_backend.Dtos
{
    public class CreateBusinessDto
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Building { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string Postcode { get; set; }
    }

    public class GetAllBusinessesDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string BId { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public AddressDto Address { get; set; }

        [BsonElement("CreatedBy")]
        public CreatedByDto CreatedBy { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateBusinessDto
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Building { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string Postcode { get; set; }
        public bool IsActive { get; set; }
    }

    public class GetFilteredBusinessDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Bid { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public AddressDto Address { get; set; }

        [BsonElement("CreatedBy")]
        public CreatedByDto CreatedBy { get; set; }
        public bool IsActive { get; set; }
    }

}
