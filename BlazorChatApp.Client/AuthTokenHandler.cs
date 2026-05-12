// BlazorChatApp.Client/AuthTokenHandler.cs
using BlazorChatApp.Client.States;
using System.Net.Http.Headers;

namespace BlazorChatApp.Client
{
    public class AuthTokenHandler : DelegatingHandler
    {
        private readonly AuthenticationState _authState;

        public AuthTokenHandler(AuthenticationState authState)
        {
            _authState = authState;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(_authState.Token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", _authState.Token);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}