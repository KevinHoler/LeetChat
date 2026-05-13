// BlazorChatApp.Client/AuthTokenHandler.cs
using BlazorChatApp.Client.Services;
using BlazorChatApp.Client.States;
using System.Net.Http.Headers;

namespace BlazorChatApp.Client
{
    public class AuthTokenHandler : DelegatingHandler
    {
        private readonly IAuthService _auth;
        public AuthTokenHandler(IAuthService auth) => _auth = auth;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken ct)
        {
            if (!string.IsNullOrWhiteSpace(_auth.Token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _auth.Token);
            return base.SendAsync(request, ct);
        }
    }
}