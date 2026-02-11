using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SmartWebApp
{
    // Add the LoginResponse class definition
    public class LoginResponse
    {
        public string Token { get; set; }
    }

    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;
        private readonly NavigationManager _nav;

        public AuthService(HttpClient http, IJSRuntime js, NavigationManager nav)
        {
            _http = http;
            _js = js;
            _nav = nav;
        }

        public async Task<bool> Login(string username, string password)
        {
            var result = await _http.PostAsJsonAsync("auth/login", new { username, password });

            if (!result.IsSuccessStatusCode) return false;

            var content = await result.Content.ReadFromJsonAsync<LoginResponse>();
            await _js.InvokeVoidAsync("localStorage.setItem", "authToken", content.Token);

            _nav.NavigateTo("/");
            return true;
        }
    }

}
