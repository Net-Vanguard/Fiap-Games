namespace Fiap.Infra.CrossCutting.Common.Http.CRM.Models
{
    public class UserResponse
    {
        [JsonPropertyName("userId")]
        public int UserId { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        
        [JsonPropertyName("type")]
        public TypeUser Type { get; set; }
        
        [JsonPropertyName("active")]
        public bool Active { get; set; }
        
        [JsonPropertyName("gameIds")]
        public List<int> GameIds { get; set; } = [];
    }
    public enum TypeUser : byte
    {
        Customer = 1,
        Admin = 2
    }
}
