using DemoProject_backend.Dtos;
using DemoProject_backend.Models;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Task = System.Threading.Tasks.Task;
using TaskModel = DemoProject_backend.Models.Task;

namespace DemoProject_backend.Services
{
    public class TaskService
    {
        private readonly IMongoCollection<TaskModel> _task;
        public TaskService(IConfiguration config)
        {
            var settings = config.GetSection("MongoDbSettings").Get<MongoDbSettings>();
            var client = new MongoClient(settings?.ConnectionString);
            var database = client.GetDatabase(settings?.DatabaseName);
            _task = database.GetCollection<TaskModel>(settings?.TaskCollectionName);
        }
        public async Task CreateTask(TaskModel task)
        {
            await _task.InsertOneAsync(task);
        }
        public async Task<Object> GetAllTaskForAmin()
        {
            var pipeline = new[]
            {
            new BsonDocument("$lookup", new BsonDocument
            {
            { "from", "Users" },
            { "localField", "UserId" },
            { "foreignField", "_id" },
            { "as", "userDetails" }
            }),
            new BsonDocument("$addFields", new BsonDocument
            {
            { "userDetails", new BsonDocument("$arrayElemAt", new BsonArray { "$userDetails", 0 }) }
            }),
            new BsonDocument("$lookup", new BsonDocument
            {
            { "from", "Businesses" },
            { "localField", "BusinessId" },
            { "foreignField", "_id" },
            { "as", "businessDetails" }
            }),
            new BsonDocument("$addFields", new BsonDocument
            {
            { "businessDetails", new BsonDocument("$arrayElemAt", new BsonArray { "$businessDetails", 0 }) }
            }),
            };
            var docs = await _task.Aggregate<BsonDocument>(pipeline).ToListAsync();

            var tasks = docs.Select(doc =>
                BsonSerializer.Deserialize<GetAllTaskDto>(doc)
            ).ToList();

            return tasks;
        }
        public async Task<Object> GetAllTaskAddedByCurrentUser(string userId)
        {
            var objectId = new ObjectId(userId);
            var pipeline = new[]
{
                new BsonDocument("$match", new BsonDocument("UserId", objectId) ),
            new BsonDocument("$lookup", new BsonDocument
            {
            { "from", "Users" },
            { "localField", "UserId" },
            { "foreignField", "_id" },
            { "as", "userDetails" }
            }),
            new BsonDocument("$addFields", new BsonDocument
            {
            { "userDetails", new BsonDocument("$arrayElemAt", new BsonArray { "$userDetails", 0 }) }
            }),
            new BsonDocument("$lookup", new BsonDocument
            {
            { "from", "Businesses" },
            { "localField", "BusinessId" },
            { "foreignField", "_id" },
            { "as", "businessDetails" }
            }),
            new BsonDocument("$addFields", new BsonDocument
            {
            { "businessDetails", new BsonDocument("$arrayElemAt", new BsonArray { "$businessDetails", 0 }) }
            }),
            };
            var docs = await _task.Aggregate<BsonDocument>(pipeline).ToListAsync();

            var tasks = docs.Select(doc =>
                BsonSerializer.Deserialize<GetAllTaskDto>(doc)
            ).ToList();

            return tasks;
        }
        public async Task<TaskModel> GetTaskByTaskId(string taskId)
        {
            return await _task.Find(t => t.Id == taskId).FirstOrDefaultAsync();
        }
        public async Task UpdateTask(string taskId, TaskModel updatedTask)
        {
            await _task.FindOneAndReplaceAsync(task => task.Id == taskId, updatedTask);
        }

        //public async Task CreateSubTask(AddSubTaskDto subtask, string taskId)
        //{
        //    var task = await GetTaskByTaskId(taskId);


        //    task.SubTask.Add(subtask);

        //}


    }
}
