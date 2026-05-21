using BlazorChatApp.Client.Helpers;
using BlazorChatApp.Shared.DTO;
using Microsoft.JSInterop;

namespace BlazorChatApp.Client.Services;

public class AuthService : IAuthService
{
    private const string StorageKey = "authkey";
    private readonly IJSRuntime _js;

    public string? Name { get; private set; }
    public string? Token { get; private set; }
    public bool IsAuthenticated { get; private set; }

    public event Action? OnChange;

    public AuthService(IJSRuntime js) => _js = js;

    public async Task InitializeAsync()
    {
        try
        {
            var json = await _js.InvokeAsync<string?>("window.getFromStorage", StorageKey);
            if (!string.IsNullOrEmpty(json))
            {
                var dto = JsonConverter.Deserialize<AuthResponseDto>(json);
                Load(dto);
            }
        }
        catch {  }
    }

    public async Task LoginAsync(AuthResponseDto dto)
    {
        Load(dto);
        var json = JsonConverter.Serialize(dto);
        await _js.InvokeVoidAsync("window.setInStorage", StorageKey, json);
        OnChange?.Invoke();
    }

    public async Task LogoutAsync()
    {
        Name = null;
        Token = null;
        IsAuthenticated = false;
        await _js.InvokeVoidAsync("window.removeFromStorage", StorageKey);
        OnChange?.Invoke();
    }

    private void Load(AuthResponseDto dto)
    {
        Name = dto.Name;
        Token = dto.Token;
        IsAuthenticated = true;
    }
}