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
        public async Task<Object> GetTaskHistoryById(string id)
        {
            var objectId = new ObjectId(id);
            var pipeline = new BsonDocument[]
            {
                  new BsonDocument("$match", new BsonDocument("TaskId", objectId)),
                new BsonDocument("$lookup", new BsonDocument{
                     { "from", "Tasks" },
                     { "localField", "TaskId" },
                     { "foreignField", "_id" },
                     { "as", "taskdetails" }
                }),
                new BsonDocument("$addFields", new BsonDocument
                {
                     { "taskdetails", new BsonDocument("$arrayElemAt", new BsonArray { "$taskdetails", 0 }) }
                }),
                new BsonDocument("$lookup", new BsonDocument{
                     { "from", "Users" },
                     { "localField", "UpdatedBy" },
                     { "foreignField", "_id" },
                     { "as", "userdetails" }
                }),
                new BsonDocument("$addFields", new BsonDocument
                {
                     { "userdetails", new BsonDocument("$arrayElemAt", new BsonArray { "$userdetails", 0 }) }
                }),
                    new BsonDocument("$project", new BsonDocument
                    {
                    { "_id", 1 },
                    { "timestamp", 1 },
                    { "changes", 1 },
                    { "taskdetails.title", 1 },
                    { "taskdetails.description", 1 },
                    { "taskdetails.type", 1 },
                    { "userdetails.name", 1 },
                    { "userdetails.email", 1 },
                    { "userdetails.role", 1 }
                     })
            };
            var docs = await _taskHistory.Aggregate<BsonDocument>(pipeline).ToListAsync();

            var taskHistories = docs.Select(doc =>
                BsonSerializer.Deserialize<GetTaskHistoryDto>(doc)
            ).ToList();

            return taskHistories;
        }

        public async Task<Object> GetTaskHistoryByBusinessId(string id)
        {
            var objectId = new ObjectId(id);
            var pipeline = new BsonDocument[]
            {
                  new BsonDocument("$match", new BsonDocument("BusinessId", objectId)),
                new BsonDocument("$lookup", new BsonDocument{
                     { "from", "Tasks" },
                     { "localField", "TaskId" },
                     { "foreignField", "_id" },
                     { "as", "taskdetails" }
                }),
                new BsonDocument("$addFields", new BsonDocument
                {
                     { "taskdetails", new BsonDocument("$arrayElemAt", new BsonArray { "$taskdetails", 0 }) }
                }),
                new BsonDocument("$lookup", new BsonDocument{
                     { "from", "Users" },
                     { "localField", "UpdatedBy" },
                     { "foreignField", "_id" },
                     { "as", "userdetails" }
                }),
                new BsonDocument("$addFields", new BsonDocument
                {
                     { "userdetails", new BsonDocument("$arrayElemAt", new BsonArray { "$userdetails", 0 }) }
                }),
                    new BsonDocument("$project", new BsonDocument
                    {
                    { "_id", 1 },
                    { "timestamp", 1 },
                    { "changes", 1 },
                    { "taskdetails.title", 1 },
                    { "taskdetails.description", 1 },
                    { "taskdetails.type", 1 },
                    { "userdetails.name", 1 },
                    { "userdetails.email", 1 },
                    { "userdetails.role", 1 }
                     })
            };
            var docs = await _taskHistory.Aggregate<BsonDocument>(pipeline).ToListAsync();

            var taskHistories = docs.Select(doc =>
                BsonSerializer.Deserialize<GetTaskHistoryDto>(doc)
            ).ToList();

            return taskHistories;
        }
    }
}

