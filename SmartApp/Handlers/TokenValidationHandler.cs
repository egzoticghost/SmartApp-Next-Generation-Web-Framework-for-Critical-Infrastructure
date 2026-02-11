using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartApp.Data; // <-- Corrected namespace for AppDbContext
using SmartApp.Data.Models; // <-- Corrected namespace for AppUser model

namespace SmartApp.Handlers
{
    public class TokenValidationHandler : DelegatingHandler
    {
        private readonly HttpClient _httpClient;
        //private readonly AppDbContext _dbContext; // <-- Add this line
        private readonly string _introspectionEndpoint;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public TokenValidationHandler(HttpClient httpClient) // <-- Add dbContext parameter
        {
            _httpClient = httpClient;
            //_dbContext = dbContext; // <-- Assign dbContext

            _introspectionEndpoint = "https://your-idp.com/oauth2/introspect"; // <-- zmieñ
            _clientId = "your-client-id"; // <-- zmieñ
            _clientSecret = "your-client-secret"; // <-- zmieñ
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.Authorization == null || string.IsNullOrEmpty(request.Headers.Authorization.Parameter))
            {
                throw new HttpRequestException("Authorization header is missing or invalid.");
            }

            string token = request.Headers.Authorization.Parameter;

            // 1. Introspect token
            var form = new Dictionary<string, string>
            {
                { "token", token },
                { "token_type_hint", "access_token" }
            };

            var basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
            var introspectRequest = new HttpRequestMessage(HttpMethod.Post, _introspectionEndpoint)
            {
                Content = new FormUrlEncodedContent(form)
            };
            introspectRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);

            var response = await _httpClient.SendAsync(introspectRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("Token introspection failed.");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var json = JsonDocument.Parse(content);
            if (!json.RootElement.TryGetProperty("active", out var activeProp) || !activeProp.GetBoolean())
            {
                throw new HttpRequestException("Token is not active or valid.");
            }

            // 2. Extract user identifier (e.g. "sub" or "preferred_username")
            string userId = json.RootElement.GetProperty("sub").GetString(); // mo¿esz te¿ u¿yæ: "preferred_username"
            string username = json.RootElement.TryGetProperty("preferred_username", out var userProp)
                ? userProp.GetString()
                : userId;

            // 3. Check if user exists in DB
            //var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.ExternalId == userId, cancellationToken);

            //if (user == null)
            //{
            //    // 4. Create user
            //    user = new AppUser
            //    {
            //        ExternalId = userId,
            //        UserName = username,
            //        CreatedAt = DateTime.UtcNow
            //    };

            //    _dbContext.Users.Add(user);
            //    await _dbContext.SaveChangesAsync(cancellationToken);
            //}

            // Dalej przeka¿ ¿¹danie
            return await base.SendAsync(request, cancellationToken);
        }
    }
}