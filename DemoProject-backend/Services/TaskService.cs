using DemoProject_backend.Dtos;
using DemoProject_backend.Models;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IMongoCollection<User> _user;
        private readonly IMongoCollection<Business> _business;
        public TaskService(IConfiguration config)
        {
            var settings = config.GetSection("MongoDbSettings").Get<MongoDbSettings>();
            var client = new MongoClient(settings?.ConnectionString);
            var database = client.GetDatabase(settings?.DatabaseName);
            _task = database.GetCollection<TaskModel>(settings?.TaskCollectionName);
            _user = database.GetCollection<User>(settings?.UserCollectionName);
            _business = database.GetCollection<Business>(settings?.UserCollectionName);
        }

        public async Task CreateTask(TaskModel task)
        {

            await _task.InsertOneAsync(task);
        }

        public async Task<List<TaskModel>> GetAllTaskForAdmin()
        {
            List<TaskModel> tasks = await _task.Find(t=> t.IsActive == true).ToListAsync();
            return tasks;
        }

        public async Task<List<TaskModel>> GetAllTaskAddedByCurrentUser(string userId)
        {
            var tasks = await _task.Find(t =>
                t.IsActive == true &&
                (
                    (t.CreatedBy != null && t.CreatedBy.Id == userId) ||
                    (t.Assignee != null && t.Assignee.Id == userId)
                )
            ).ToListAsync();

            return tasks;
        } 

        public async Task<TaskModel> GetTaskByTaskId(string taskId)
        {
            return await _task.Find(t => t.Id == taskId).FirstOrDefaultAsync();
        }

        public async Task<List<TaskModel>> GetTasksByBusinessId(string businessId)
        {
            return await _task.Find(b => b.BusinessDetails.Id == businessId).ToListAsync();
        }

        public async Task UpdateTask(string taskId, TaskModel updatedTask)
        {
            await _task.FindOneAndReplaceAsync(task => task.Id == taskId, updatedTask);
        }

        public async Task<TaskModel?> GetLastTask()
        {
            return await _task
                .Find(FilterDefinition<TaskModel>.Empty)
                .SortByDescending(b => b.Tid)
                .Limit(1)
                .FirstOrDefaultAsync();
        }

        public async Task DisableTask(string taskId)
        { 

            var filter = Builders<TaskModel>.Filter.Eq(t => t.Id, taskId);
            var update = Builders<TaskModel>.Update.Set(t => t.IsActive, false);

            await _task.UpdateOneAsync(filter, update);
        }

        public async Task CreateSubTask(SubTask subtaskDto, string taskId)
        {
            var subtask = new SubTask
            {
                Id= ObjectId.GenerateNewId().ToString(),
                Title = subtaskDto.Title,
                Status = subtaskDto.Status

            };

            //Note:Builders<T>.Update: This is part of MongoDB's C# driver. It helps you build update queries for a collection of type T. In your case, T is TaskModel.
            //.Push(...): This generates a $push MongoDB update operation. It adds an element to an array field(here, SubTask list).
            var update = Builders<TaskModel>.Update.Push(t => t.Subtask, subtask);


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
                t => t.Subtask, s => s.Id == subtaskObjectId.ToString()
            );
            Console.WriteLine(filter);
            var update = Builders<TaskModel>.Update.Set(
                "Subtask.$.Status", newStatus
            );
            Console.WriteLine(update);
            var result = await _task.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                throw new Exception("Subtask not found or status not updated.");
            }
        }

        public async Task DisableSubTaskAsync(string subtaskId)
        {

            var filter = Builders<TaskModel>.Filter.ElemMatch(
                t => t.Subtask, s => s.Id == subtaskId
            );

            var update = Builders<TaskModel>.Update.Set("Subtask.$.IsActive", false);

            await _task.UpdateOneAsync(filter, update);
        }

        public async Task CompleteTask(string taskId, string userId, string userName)
        {
            var filter = Builders<TaskModel>.Filter.Eq(t => t.Id, taskId);

            var update = Builders<TaskModel>.Update.Combine(
                Builders<TaskModel>.Update.Set(t => t.IsCompleted, true),
                Builders<TaskModel>.Update.Set(t => t.CompletionDate, DateTime.UtcNow),
                Builders<TaskModel>.Update.Set(t => t.CompletedBy.Id, userId),
                Builders<TaskModel>.Update.Set(t => t.CompletedBy.Name, userName)
            );

            await _task.UpdateOneAsync(filter, update);
        }

        public async Task<List<TaskModel>> FilterTasksAsync(string criteria, string value)
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
            var docs = await _task.Aggregate<BsonDocument>(pipeline).ToListAsync();

            var tasks = docs.Select(doc =>
                BsonSerializer.Deserialize<TaskModel>(doc)
            ).ToList();

            return tasks;
        }

        public async Task<List<TaskModel>> FilterTasksByUserAsync(string criteria, string value, string userId)
        {
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument(criteria, value)),

                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "Users" },
                    { "localField", "CreatedBy._id" },
                    { "foreignField", "_id" },
                    { "as", "userDetails" }
                }),

                new BsonDocument("$unwind", "$userDetails"),

                new BsonDocument("$match", new BsonDocument("$or", new BsonArray
                {
                    new BsonDocument("userDetails._id", new ObjectId(userId)),
                    new BsonDocument("Assignee._id", new ObjectId(userId))
                })),

                new BsonDocument("$addFields", new BsonDocument
                {
                    { "CreatedBy.Name", "$userDetails.Username" },
                    { "CreatedBy.Email", "$userDetails.Email" }
                }),

                new BsonDocument("$project", new BsonDocument
                {
                    { "userDetails", 0 }
                })
            };

            var docs = await _task.Aggregate<BsonDocument>(pipeline).ToListAsync();

            var tasks = docs.Select(doc =>
                BsonSerializer.Deserialize<TaskModel>(doc)
            ).ToList();

            return tasks;
        }

        private async Task<List<object>> GroupByDay(DateTime start, DateTime end)
        {
            var pipeline = new List<BsonDocument>
    {
        new BsonDocument("$match", new BsonDocument
        {
            { "CreatedBy.Date", new BsonDocument {
                { "$gte", start },
                { "$lte", end }
            }}
        }),

        new BsonDocument("$group", new BsonDocument
        {
            { "_id", new BsonDocument {
                { "year", new BsonDocument("$year", "$CreatedBy.Date") },
                { "month", new BsonDocument("$month", "$CreatedBy.Date") },
                { "day", new BsonDocument("$dayOfMonth", "$CreatedBy.Date") }
            }},
            { "count", new BsonDocument("$sum", 1) }
        }),

        new BsonDocument("$sort", new BsonDocument
        {
            { "_id.year", 1 },
            { "_id.month", 1 },
            { "_id.day", 1 }
        })
    };

            var result = await _task.Aggregate<BsonDocument>(pipeline).ToListAsync();

            return result.Select(r => new
            {
                date = new DateTime(
                    r["_id"]["year"].AsInt32,
                    r["_id"]["month"].AsInt32,
                    r["_id"]["day"].AsInt32
                ).ToString("yyyy-MM-dd"),
                count = r["count"].AsInt32
            }).Cast<object>().ToList();
        }

        public async Task<object> TaskCreationStats()
        {
            var now = DateTime.UtcNow;

            var firstDayThisMonth = new DateTime(now.Year, now.Month, 1);
            var firstDayLastMonth = firstDayThisMonth.AddMonths(-1);
            var lastDayLastMonth = firstDayThisMonth.AddDays(-1);

            var thisMonthData = await GroupByDay(firstDayThisMonth, now);
            var lastMonthData = await GroupByDay(firstDayLastMonth, lastDayLastMonth);

            return new
            {
                thisMonth = thisMonthData,
                lastMonth = lastMonthData
            };
        }

        public async Task<object> TaskCreatedAndCompletedCountByUserPeriods()
        {
            var now = DateTime.UtcNow;

            var stats = new Dictionary<string, List<object>>();

            var thisMonthStart = new DateTime(now.Year, now.Month, 1);
            var nextMonthStart = thisMonthStart.AddMonths(1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);

            var periods = new[]
            {
        new { Key = "thisMonth", Start = thisMonthStart, End = nextMonthStart },
        new { Key = "lastMonth", Start = lastMonthStart, End = thisMonthStart }
    };

            foreach (var period in periods)
            {
                var pipeline = new BsonDocument[]
                {
                    new BsonDocument("$lookup", new BsonDocument
                    {
                        { "from", "Tasks" },
                        { "let", new BsonDocument("userId", "$_id") },
                        { "pipeline", new BsonArray
                            {
                                new BsonDocument("$match", new BsonDocument
                                {
                                    { "$expr", new BsonDocument("$and", new BsonArray
                                        {
                                            new BsonDocument("$eq", new BsonArray { "$CreatedBy._id", "$$userId" }),
                                            new BsonDocument("$gte", new BsonArray { "$CreatedBy.Date", period.Start }),
                                            new BsonDocument("$lt", new BsonArray { "$CreatedBy.Date", period.End })
                                        })
                                    }
                                })
                            }
                        },
                        { "as", "createdTasks" }
                    }),

                    new BsonDocument("$lookup", new BsonDocument
                    {
                        { "from", "Tasks" },
                        { "let", new BsonDocument("userId", "$_id") },
                        { "pipeline", new BsonArray
                            {
                                new BsonDocument("$match", new BsonDocument
                                {
                                    { "$expr", new BsonDocument("$and", new BsonArray
                                        {
                                            new BsonDocument("$eq", new BsonArray { "$CompletedBy._id", "$$userId" }),
                                            new BsonDocument("$gte", new BsonArray { "$CompletionDate", period.Start }),
                                            new BsonDocument("$lt", new BsonArray { "$CompletionDate", period.End })
                                        })
                                    }
                                })
                            }
                        },
                        { "as", "completedTasks" }
                    }),


                    new BsonDocument("$project", new BsonDocument
                    {
                        { "name", "$Name" },
                        { "creation_count", new BsonDocument("$size", "$createdTasks") },
                        { "completion_count", new BsonDocument("$size", "$completedTasks") }
                    }),
                    new BsonDocument("$match", new BsonDocument("$or", new BsonArray
                    {
                        new BsonDocument("creation_count", new BsonDocument("$gt", 0)),
                        new BsonDocument("completion_count", new BsonDocument("$gt", 0))
                    })),

                    new BsonDocument("$sort", new BsonDocument("creation_count", -1))
                };

                var result = await _user.Aggregate<BsonDocument>(pipeline).ToListAsync();

                stats[period.Key] = result.Select(r => new
                {
                    name = r.GetValue("name", "").AsString,
                    creation_count = r.GetValue("creation_count", 0).ToInt32(),
                    completion_count = r.GetValue("completion_count", 0).ToInt32()
                }).Cast<object>().ToList();
            }

            return stats;
        }


        public async Task<object> GetTaskCompletionStats()
        {
            var now = DateTime.UtcNow;

            var startOfCurrentMonth = new DateTime(now.Year, now.Month, 1);
            var startOfPreviousMonth = startOfCurrentMonth.AddMonths(-1);
            var endOfPreviousMonth = startOfCurrentMonth.AddTicks(-1);

            var totalCompleted = await _task.CountDocumentsAsync(t => t.IsCompleted);

            var totalIncomplete = await _task.CountDocumentsAsync(t => !t.IsCompleted);

            var completedCurrentMonth = await _task.CountDocumentsAsync(t =>
                t.IsCompleted &&
                t.CompletionDate >= startOfCurrentMonth &&
                t.CompletionDate <= now
            );

            var completedPreviousMonth = await _task.CountDocumentsAsync(t =>
                t.IsCompleted &&
                t.CompletionDate >= startOfPreviousMonth &&
                t.CompletionDate <= endOfPreviousMonth
            );
            var totalBusinesses = await _business.CountDocumentsAsync(_ => true);

            return new
            {
                TotalCompleted = totalCompleted,
                TotalIncomplete = totalIncomplete,
                CurrentMonthCompleted = completedCurrentMonth,
                PreviousMonthCompleted = completedPreviousMonth,
                TotalBusinesses = totalBusinesses
            };
        }

    }
}
