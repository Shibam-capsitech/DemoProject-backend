using DemoProject_backend.Dtos;
using DemoProject_backend.Models;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using static DemoProject_backend.Services.TaskService;
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
        public async Task<List<GetAllTaskDto>> GetAllTaskAddedByCurrentUser(string userId)
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

        public async Task<List<TaskModel>> GetTasksByBusinessId(string businessId)
        {
            return await _task.Find(b => b.businessDetails.Id == businessId).ToListAsync();
        }
        public async Task UpdateTask(string taskId, TaskModel updatedTask)
        {
            await _task.FindOneAndReplaceAsync(task => task.Id == taskId, updatedTask);
        }
        public async Task<TaskModel?> GetLastTask()
        {
            return await _task
                .Find(FilterDefinition<TaskModel>.Empty)
                .SortByDescending(b => b.tid)
                .Limit(1)
                .FirstOrDefaultAsync();
        }
        public async Task CreateSubTask(SubTask subtaskDto, string taskId)
        {
            var subtask = new SubTask
            {
                Id= ObjectId.GenerateNewId().ToString(),
                title = subtaskDto.title,
                status = subtaskDto.status

            };

            //Note:Builders<T>.Update: This is part of MongoDB's C# driver. It helps you build update queries for a collection of type T. In your case, T is TaskModel.
            //.Push(...): This generates a $push MongoDB update operation. It adds an element to an array field(here, SubTask list).
            var update = Builders<TaskModel>.Update.Push(t => t.subtask, subtask);


            var result = await _task.UpdateOneAsync(
                t => t.Id == taskId,
                update
            );

            if (result.ModifiedCount == 0)
            {
                throw new Exception($"Task with ID {taskId} not found.");
            }

        }

        public async Task UpdateSubtaskStaus(string subtaskId, string newStatus)
        {
            Console.WriteLine(newStatus);
            Console.WriteLine(newStatus.GetType());
            
            var subtaskObjectId = new ObjectId(subtaskId);
            var filter = Builders<TaskModel>.Filter.ElemMatch(
                t => t.subtask, s => s.Id == subtaskObjectId.ToString()
            );
            Console.WriteLine(filter);
            var update = Builders<TaskModel>.Update.Set(
                "subtask.$.status", newStatus
            );
            Console.WriteLine(update);
            var result = await _task.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                throw new Exception("Subtask not found or status not updated.");
            }
        }

        public async Task DeleteSubTask(string subtaskId)
        {
            var subtaskObjectId = ObjectId.Parse(subtaskId);

            var filter = Builders<TaskModel>.Filter.ElemMatch(
                t => t.subtask, s => s.Id == subtaskObjectId.ToString()
            );

            var update = Builders<TaskModel>.Update.PullFilter(
                t => t.subtask, s => s.Id == subtaskObjectId.ToString()
            );

            await _task.UpdateOneAsync(filter, update);
        }

        public async Task<Object> FilterTasksAsync(string criteria, string value)
        {
            //var filter = Builders<TaskModel>.Filter.Eq(criteria, value);
            //var result = await _task.Find(filter).ToListAsync();
            //return result;

            var pipeline = new[]
            {
            new BsonDocument("$match", new BsonDocument(criteria.ToString(), value )),
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
            new BsonDocument("$addFields", new BsonDocument
            {
            { "username", "$userDetails.username" }
            }),
            new BsonDocument("$addFields", new BsonDocument
            {
            { "email", "$userDetails.email" }
            }),
            };
            var docs = await _task.Aggregate<BsonDocument>(pipeline).ToListAsync();

            var tasks = docs.Select(doc =>
                BsonSerializer.Deserialize<GetFilteredTaskDto>(doc)
            ).ToList();

            return tasks;
        }


    }
}
