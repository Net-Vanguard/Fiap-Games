namespace Fiap.Infra.CrossCutting.Common.Http.CRM.Models
{
    public class BaseResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        
        [JsonPropertyName("data")]
        public T? Data { get; set; }
        
        [JsonPropertyName("error")]
        public string? Error { get; set; }

        public static BaseResponse<T> Ok(T data) => new()
        {
            Success = true,
            Data = data
        };

        public static BaseResponse<T> Fail(string error) => new()
        {
            Success = false,
            Error = error
        };
    }
}