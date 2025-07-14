using MongoDB.Bson.Serialization.Attributes;

namespace DemoProject_backend.Models
{
    public class AddressModel
    {
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string? building { get; set; }

        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string? city { get; set; }

        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string? country { get; set; }

        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string? state { get; set; }

        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string? postcode { get; set; }
    }
}
