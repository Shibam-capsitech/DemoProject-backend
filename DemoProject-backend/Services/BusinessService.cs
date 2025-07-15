using DemoProject_backend.Dtos;
using DemoProject_backend.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Reflection.Metadata;

namespace DemoProject_backend.Services
{
    public class BusinessService
    {
        private readonly IMongoCollection<Business> _business;
        public BusinessService(IConfiguration config)
        {
            var settings = config.GetSection("MongoDbSettings").Get<MongoDbSettings>();
            var client = new MongoClient(settings?.ConnectionString);
            var database = client.GetDatabase(settings?.DatabaseName);
            _business = database.GetCollection<Business>(settings?.BusinessCollectionName);
        }
        
        public async System.Threading.Tasks.Task CreateBusiness(Business business)
        {
            await _business.InsertOneAsync(business);
        }

        public async Task<object> GetAllBusinessesForAdmin()
        {
            var pipeline = new[]
            {
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "Users" },
                    { "localField", "CreatedBy._id" },
                    { "foreignField", "_id" },
                    { "as", "userDetails" }
                }),
                new BsonDocument("$addFields", new BsonDocument
                {
                    { "CreatedBy.Name", new BsonDocument("$arrayElemAt", new BsonArray { "$userDetails.Username", 0 }) },
                     { "CreatedBy.Email", new BsonDocument("$arrayElemAt", new BsonArray { "$userDetails.Email", 0 }) }

                }),
                new BsonDocument("$project", new BsonDocument
                {
                    { "userDetails", 0 } 
                })
            };

            var docs = await _business.Aggregate<BsonDocument>(pipeline).ToListAsync();

            var businesses = docs.Select(doc =>
                BsonSerializer.Deserialize<Business>(doc)
            ).ToList();

            return businesses;
        }

        public async Task<Object> GetAllBusinessesAddedByCurrentUser(string userId)
        {
            var objectId = new ObjectId(userId);
            var pipeline = new[]
            {
            new BsonDocument("$match", new BsonDocument("CreatedBy._id", objectId) ),
            new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "Users" },
                    { "localField", "CreatedBy._id" },
                    { "foreignField", "_id" },
                    { "as", "userDetails" }
                }),
            new BsonDocument("$addFields", new BsonDocument
                {
                    { "CreatedBy.Name", new BsonDocument("$arrayElemAt", new BsonArray { "$userDetails.Username", 0 }) },
                    { "CreatedBy.Email", new BsonDocument("$arrayElemAt", new BsonArray { "$userDetails.Email", 0 }) }

                }),
            new BsonDocument("$project", new BsonDocument
                {
                    { "userDetails", 0 }
                })
            };
            var docs = await _business.Aggregate<BsonDocument>(pipeline).ToListAsync();

            var businesses = docs.Select(doc =>
                BsonSerializer.Deserialize<Business>(doc)
            ).ToList();

            return businesses;
        }
        public async System.Threading.Tasks.Task DeleteBusiness(string id)
        {
            await _business.DeleteOneAsync(id);
        }

        public async Task<Business?> GetLastBusinessAsync()
        {
            return await _business
                .Find(FilterDefinition<Business>.Empty)
                .SortByDescending(b => b.BId)
                .Limit(1)
                .FirstOrDefaultAsync();
        }

        public async Task<GetAllBusinessesDto> GetBusinessById(string id)
        {
            var objectId = new ObjectId(id);
            Console.WriteLine(objectId);
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("_id", objectId) ),
                new BsonDocument("$lookup",new BsonDocument
                {
                 { "from", "Users" },
                 { "localField", "CreatedBy._id" },
                 { "foreignField", "_id" },
                 { "as", "userDetails" }
                }),
                new BsonDocument("$addFields", new BsonDocument
                {
                    { "CreatedBy.Name", new BsonDocument("$arrayElemAt", new BsonArray { "$userDetails.Username", 0 }) },
                    { "CreatedBy.Email", new BsonDocument("$arrayElemAt", new BsonArray { "$userDetails.Email", 0 }) },
                    { "CreatedBy.Address", new BsonDocument("$arrayElemAt", new BsonArray { "$userDetails.Address", 0 }) }

                }),
                new BsonDocument("$project", new BsonDocument
                {
                    { "userDetails", 0 }
                })
            };
            var doc = await _business.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
            GetAllBusinessesDto business = BsonSerializer.Deserialize<GetAllBusinessesDto>(doc);
            return business;
        }

        public async System.Threading.Tasks.Task UpdateBusiness(Business updatedBusiness, string id)
        {
            updatedBusiness.Id = id; 
            await _business.FindOneAndReplaceAsync(b => b.Id == id, updatedBusiness);
        }

        public async Task<List<Business>> FilterBusinessesAsync(string criteria, string value)
        {
            //var filter = Builders<Business>.Filter.Eq(criteria, value);
            //var result = await _business.Find(filter).ToListAsync();
            //return result;


            var pipeline = new[]
            {
            new BsonDocument("$match", new BsonDocument(criteria.ToString(), value )),
            new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "Users" },
                    { "localField", "CreatedBy._id" },
                    { "foreignField", "_id" },
                    { "as", "userDetails" }
                }),
                new BsonDocument("$addFields", new BsonDocument
                {
                    { "CreatedBy.Name", new BsonDocument("$arrayElemAt", new BsonArray { "$userDetails.Username", 0 }) },
                     { "CreatedBy.Email", new BsonDocument("$arrayElemAt", new BsonArray { "$userDetails.Email", 0 }) }

                }),
                new BsonDocument("$project", new BsonDocument
                {
                    { "userDetails", 0 }
                })
            };
            var docs = await _business.Aggregate<BsonDocument>(pipeline).ToListAsync();

            var businesses = docs.Select(doc =>
                BsonSerializer.Deserialize<Business>(doc)
            ).ToList();

            return businesses;

        }

    }
}
