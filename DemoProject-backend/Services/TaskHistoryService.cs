using DemoProject_backend.Dtos;
using DemoProject_backend.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Task = System.Threading.Tasks.Task;

namespace DemoProject_backend.Services
{
    public class TaskHistoryService
    {
        private readonly IMongoCollection<TaskHistoryModel > _taskHistory;
        public TaskHistoryService(IConfiguration config)
        {
            var settings = config.GetSection("MongoDbSettings").Get<MongoDbSettings>();
            var client = new MongoClient(settings?.ConnectionString);
            var database = client.GetDatabase(settings?.DatabaseName);
            _taskHistory = database.GetCollection<TaskHistoryModel>(settings?.TaskHistoryCollectionName);
        }

        public async Task CreateTaskHistory(TaskHistoryModel taskHistory)
        {
            await _taskHistory.InsertOneAsync(taskHistory);
        }
        public async Task<object> GetTaskHistoryById(string id)
        {
            var objectId = new ObjectId(id);

            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$match", new BsonDocument("TargetTask._id", objectId)),

                //new BsonDocument("$lookup", new BsonDocument
                //{
                //    { "from", "Tasks" },
                //    { "localField", "targetedtask._id" },
                //    { "foreignField", "_id" },
                //    { "as", "task" }
                //}),
                //new BsonDocument("$addFields", new BsonDocument
                //{
                //    { "task", new BsonDocument("$arrayElemAt", new BsonArray { "$task", 0 }) }
                //}),

                //new BsonDocument("$lookup", new BsonDocument
                //{
                //    { "from", "Businesses" },
                //    { "localField", "targetedbusiness.id" },
                //    { "foreignField", "_id" },
                //    { "as", "business" }
                //}),
                //new BsonDocument("$addFields", new BsonDocument
                //{
                //    { "business", new BsonDocument("$arrayElemAt", new BsonArray { "$business", 0 }) }
                //}),

                //new BsonDocument("$lookup", new BsonDocument
                //{
                //    { "from", "Users" },
                //    { "localField", "CreatedBy.UserId" },
                //    { "foreignField", "_id" },
                //    { "as", "creator" }
                //}),
                //new BsonDocument("$addFields", new BsonDocument
                //{
                //    { "creator", new BsonDocument("$arrayElemAt", new BsonArray { "$creator", 0 }) }
                //}),

                //new BsonDocument("$project", new BsonDocument
                //{
                //    { "_id", 1 },
                //    { "Description", 1 },
                //    { "ChangeType", 1 },
                //    { "Target", new BsonDocument {
                //        { "Id", "$targetedtask.id" },
                //        { "Name", "$targetedtask.name" }
                //    }},
                //    { "Business", new BsonDocument {
                //        { "Id", "$targetedbusiness.id" },
                //        { "Name", "$targetedbusiness.name" }
                //    }},
                //    { "CreatedBy", new BsonDocument {
                //        { "UserId", "$CreatedBy.UserId" },
                //        { "UserName", "$creator.name" },
                //        { "UserRole", "$creator.role" }
                //    }}
                //})
            };

            var docs = await _taskHistory.Aggregate<BsonDocument>(pipeline).ToListAsync();

            var result = docs.Select(doc =>
                BsonSerializer.Deserialize<TaskHistoryModel>(doc)
            ).ToList();

            return result;
        }


        public async Task<List<TaskHistoryModel>> GetTaskHistoryByBusinessId(string id)
        {
            var objectId = new ObjectId(id);
            var pipeline = new BsonDocument[]
            {
                  new BsonDocument("$match", new BsonDocument("TargetBusiness._id", objectId)),
                //new BsonDocument("$lookup", new BsonDocument{
                //     { "from", "Tasks" },
                //     { "localField", "targetedtask._id" },
                //     { "foreignField", "_id" },
                //     { "as", "taskdetails" }
                //}),
                //new BsonDocument("$addFields", new BsonDocument
                //{
                //     { "taskdetails", new BsonDocument("$arrayElemAt", new BsonArray { "$taskdetails", 0 }) }
                //}),
                //new BsonDocument("$lookup", new BsonDocument{
                //     { "from", "Users" },
                //     { "localField", "CreatedBy._id" },
                //     { "foreignField", "_id" },
                //     { "as", "userdetails" }
                //}),
                //new BsonDocument("$addFields", new BsonDocument
                //{
                //     { "userdetails", new BsonDocument("$arrayElemAt", new BsonArray { "$userdetails", 0 }) }
                //}),
                //    new BsonDocument("$project", new BsonDocument
                //    {
                //    { "_id", 1 },
                //    { "Description", 1 },
                //    { "changeType", 1 },
                //    { "taskdetails.title", 1 },
                //    { "taskdetails.description", 1 },
                //    { "taskdetails.type", 1 },
                //    { "userdetails.name", 1 },
                //    { "userdetails.email", 1 },
                //    { "userdetails.role", 1 },
                //    { "CreatedBy.Date", 1 }
                //     })
            };
            var docs = await _taskHistory.Aggregate<BsonDocument>(pipeline).ToListAsync();

            var taskHistories = docs.Select(doc =>
                BsonSerializer.Deserialize<TaskHistoryModel>(doc)
            ).ToList();

            return taskHistories;
        }
    }
}

