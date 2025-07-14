using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DemoProject_backend.Dtos
{
    public class IdNameDto
    {
        /// <summary>
        /// Client/Task Id
        /// </summary>
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        /// <summary>
        /// Client/Task Name
        /// </summary>
        public string Name { get; set; }
    }
}
