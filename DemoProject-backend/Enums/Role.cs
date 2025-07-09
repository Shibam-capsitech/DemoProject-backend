using System.Text.Json.Serialization;

namespace DemoProject_backend.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserRole
    {
        Admin,
        Manager,
        Staff,       
    }
}
