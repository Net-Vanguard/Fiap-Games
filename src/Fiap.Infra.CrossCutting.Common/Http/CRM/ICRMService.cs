namespace Fiap.Infra.CrossCutting.Common.Http.CRM
{
    public interface ICRMService
    {
        [Get("/api/v1/users")]
        Task<BaseResponse<List<UserResponse>>> GetAllAsync();

        [Get("/api/v1/users/{userId}")]
        Task<BaseResponse<UserResponse>> GetAsync(int userId);
    }
}