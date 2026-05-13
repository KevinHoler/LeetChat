using BlazorChatApp.Shared.DTO;

namespace BlazorChatApp.Client.Services;

public interface IAuthService
{
    string? Name { get; }
    string? Token { get; }
    bool IsAuthenticated { get; }

    Task InitializeAsync();
    Task LoginAsync(AuthResponseDto dto);
    Task LogoutAsync();

    event Action OnChange;
}