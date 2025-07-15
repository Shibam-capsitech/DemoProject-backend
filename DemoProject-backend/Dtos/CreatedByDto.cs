using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DemoProject_backend.Dtos
{
    public class CreatedByDto
    {
        /// <summary>
        /// User Id
        /// </summary>
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        /// <summary>
        /// User Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// User email
        /// </summary>
        public string? Email { get; set; }
        /// <summary>
        /// User email
        /// </summary>
        public AddressDto? Address { get; set; }

        /// <summary>
        /// Created Date
        /// </summary>
        public DateTime Date { get; set; }
    }
}
