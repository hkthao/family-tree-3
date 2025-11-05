using System.Net.Http.Headers;
using System.Text.Json;
using McpServer.Config;
using Microsoft.Extensions.Options;

namespace McpServer.Services
{
    /// <summary>
    /// Dịch vụ để tương tác với backend Family Tree.
    /// </summary>
    public class FamilyTreeBackendService
    {
        private readonly HttpClient _httpClient;
        private readonly FamilyTreeBackendSettings _settings;
        private readonly ILogger<FamilyTreeBackendService> _logger;

        public FamilyTreeBackendService(HttpClient httpClient, IOptions<FamilyTreeBackendSettings> settings, ILogger<FamilyTreeBackendService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;

            // BaseAddress is set in Program.cs via AddHttpClient
            // _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Lấy danh sách thành viên từ backend Family Tree.
        /// </summary>
        /// <param name="jwtToken">JWT Token để xác thực.</param>
        /// <returns>Danh sách thành viên hoặc null nếu có lỗi.</returns>
        public async Task<List<MemberDto>?> GetMembersAsync(string jwtToken)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                var response = await _httpClient.GetAsync("api/member"); // Assuming this is the endpoint
                response.EnsureSuccessStatusCode(); // Throw if not a success code

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<MemberDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling Family Tree backend to get members.");
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing members from Family Tree backend.");
                return null;
            }
        }

        // TODO: Add methods for other entities like GetFamiliesAsync, GetEventsAsync
    }

    /// <summary>
    /// DTO mẫu cho thành viên. Cần khớp với cấu trúc từ Family Tree backend.
    /// </summary>
    public class MemberDto
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        // Add other properties as needed
    }
}