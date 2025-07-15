using MongoDB.Bson.Serialization.Attributes;

namespace DemoProject_backend.Models
{
    public class AddressModel
    {
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string? Building { get; set; }

        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string? City { get; set; }

        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string? Country { get; set; }

        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string? State { get; set; }

        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string? Postcode { get; set; }
    }
}
