namespace DemoProject_backend.Models
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string UserCollectionName { get; set; } = null!;
        public string BusinessCollectionName { get; set; } = null!;
        public string TaskCollectionName { get; set; } = null!;
        public string TaskHistoryCollectionName { get; set; } = null!;
    }
}
