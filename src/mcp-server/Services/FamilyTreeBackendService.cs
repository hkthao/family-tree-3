using System.Net.Http.Headers;
using System.Text.Json;
using McpServer.Config;
using McpServer.Common.Models;
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

        /// <summary>
        /// Tìm kiếm các gia đình từ backend Family Tree.
        /// </summary>
        /// <param name="jwtToken">JWT Token để xác thực.</param>
        /// <param name="query">Chuỗi truy vấn để tìm kiếm (tên, mã, v.v.).</param>
        /// <returns>Danh sách các gia đình phù hợp hoặc null nếu có lỗi.</returns>
        public async Task<List<FamilyDto>?> SearchFamiliesAsync(string jwtToken, string query)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                var response = await _httpClient.GetAsync($"api/Family/search?keyword={Uri.EscapeDataString(query)}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Result<PaginatedList<FamilyDto>>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Value?.Items;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling Family Tree backend to search families with query: {Query}", query);
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing families from Family Tree backend.");
                return null;
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một gia đình theo ID.
        /// </summary>
        public async Task<FamilyDto?> GetFamilyByIdAsync(Guid id, string jwtToken)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                var response = await _httpClient.GetAsync($"api/family/{id}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<FamilyDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling Family Tree backend to get family by ID: {Id}", id);
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing family from Family Tree backend.");
                return null;
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một thành viên theo ID.
        /// </summary>
        public async Task<MemberDetailDto?> GetMemberByIdAsync(Guid id, string jwtToken)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                var response = await _httpClient.GetAsync($"api/member/{id}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<MemberDetailDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling Family Tree backend to get member by ID: {Id}", id);
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing member from Family Tree backend.");
                return null;
            }
        }

        /// <summary>
        /// Tìm kiếm thành viên từ backend Family Tree.
        /// </summary>
        public async Task<List<MemberDetailDto>?> SearchMembersAsync(string jwtToken, string query, Guid? familyId = null)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                var url = $"api/member/search?query={Uri.EscapeDataString(query)}";
                if (familyId.HasValue)
                {
                    url += $"&familyId={familyId.Value}";
                }
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                // Assuming SearchMembers returns a PaginatedList<MemberListDto> and we need to extract the items
                var paginatedResult = JsonSerializer.Deserialize<PaginatedList<MemberDetailDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return paginatedResult?.Items;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling Family Tree backend to search members with query: {Query}", query);
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing members from Family Tree backend.");
                return null;
            }
        }

        /// <summary>
        /// Tìm kiếm sự kiện từ backend Family Tree.
        /// </summary>
        public async Task<List<EventDto>?> SearchEventsAsync(string jwtToken, string query, Guid? familyId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                var url = $"api/event/search?query={Uri.EscapeDataString(query)}";
                if (familyId.HasValue)
                {
                    url += $"&familyId={familyId.Value}";
                }
                if (startDate.HasValue)
                {
                    url += $"&startDate={startDate.Value:yyyy-MM-dd}";
                }
                if (endDate.HasValue)
                {
                    url += $"&endDate={endDate.Value:yyyy-MM-dd}";
                }
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var paginatedResult = JsonSerializer.Deserialize<PaginatedList<EventDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return paginatedResult?.Items;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling Family Tree backend to search events with query: {Query}", query);
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing events from Family Tree backend.");
                return null;
            }
        }

        /// <summary>
        /// Lấy danh sách các sự kiện sắp tới từ backend Family Tree.
        /// </summary>
        public async Task<List<EventDto>?> GetUpcomingEventsAsync(string jwtToken, Guid? familyId = null)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                var url = "api/event/upcoming";
                if (familyId.HasValue)
                {
                    url += $"?familyId={familyId.Value}";
                }
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<EventDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling Family Tree backend to get upcoming events.");
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing upcoming events from Family Tree backend.");
                return null;
                    }
                }
            
                /// <summary>
                /// Đại diện cho một danh sách kết quả được phân trang.
                /// </summary>
                /// <typeparam name="T">Loại của các mục trong danh sách.</typeparam>
                public class PaginatedList<T>
                {
                    public List<T> Items { get; set; } = new List<T>();
                    public int PageNumber { get; set; }
                    public int TotalPages { get; set; }
                    public int TotalCount { get; set; }
                }
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

    /// <summary>
    /// DTO cho gia đình. Cần khớp với cấu trúc từ Family Tree backend.
    /// </summary>
    public class FamilyDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? History { get; set; }
        // Add other properties as needed
    }

    /// <summary>
    /// DTO chi tiết cho thành viên. Cần khớp với cấu trúc từ Family Tree backend.
    /// </summary>
    public class MemberDetailDto
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PlaceOfBirth { get; set; }
        public string? Biography { get; set; }
        // Add other properties as needed
    }

    /// <summary>
    /// DTO cho sự kiện. Cần khớp với cấu trúc từ Family Tree backend.
    /// </summary>
    public class EventDto
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? EventDate { get; set; }
        public string? Location { get; set; }
        public string? EventType { get; set; }
        // Add other properties as needed
    }
}