using DemoProject_backend.Models;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace DemoProject_backend.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _user;

        public UserService( IConfiguration config)
        {
            var settings = config.GetSection("MongoDbSettings").Get<MongoDbSettings>();
            var client = new MongoClient(settings?.ConnectionString);
            var database = client.GetDatabase(settings?.DatabaseName);
            _user = database.GetCollection<User>(settings?.UserCollectionName);
        }

        public async Task<User> GetUSerByEmail(string email)
        {
            return await _user.Find(u=> u.Email == email).FirstOrDefaultAsync();
        }

        //because the model Task and thraed task is conflicting
        public async System.Threading.Tasks.Task CreateUser(User user)
        {
            await _user.InsertOneAsync(user);
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await _user.Find(u => true).ToListAsync();
        }

        public async Task<User> GetUSerById(string id)
        {
            return await _user.Find(u => u.Id == id).FirstOrDefaultAsync();
        }
    }
    }

