using InteractionApi.Models;

namespace InteractionApi.Services;

public interface IAuthServiceClient
{
    Task<UserInfo?> GetUserByToken(string authToken);

    Task<bool> IsServiceHealthyAsync();
} 