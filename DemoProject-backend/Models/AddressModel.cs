using MongoDB.Bson.Serialization.Attributes;

namespace DemoProject_backend.Models
{
    public class AddressModel
    {
        /// <summary>
        /// building name for address 
        /// </summary>
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string? Building { get; set; }

        /// <summary>
        /// City name for address 
        /// </summary>
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string? City { get; set; }

        /// <summary>
        /// Country name for address 
        /// </summary>
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string? Country { get; set; }

        /// <summary>
        /// State name for address 
        /// </summary>
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string? State { get; set; }

        /// <summary>
        /// Postcode name for address 
        /// </summary>
        [BsonIgnoreIfDefault, BsonIgnoreIfNull]
        public string? Postcode { get; set; }
    }
}
