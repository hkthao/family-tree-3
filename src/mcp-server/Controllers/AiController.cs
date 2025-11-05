using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using McpServer.Services;
using System.Security.Claims;

namespace McpServer.Controllers
{
    /// <summary>
    /// Controller để xử lý các yêu cầu liên quan đến AI Assistant.
    /// </summary>
    [ApiController]
    [Route("api/ai")]
    [Authorize] // Yêu cầu xác thực JWT cho tất cả các endpoint trong controller này
    public class AiController : ControllerBase
    {
        private readonly AiService _aiService;
        private readonly ILogger<AiController> _logger;

        public AiController(AiService aiService, ILogger<AiController> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        /// <summary>
        /// Kiểm tra trạng thái của MCP server và AI Assistant.
        /// </summary>
        /// <returns>Trạng thái hoạt động của server.</returns>
        [HttpGet("status")]
        [AllowAnonymous] // Cho phép truy cập mà không cần xác thực
        public async Task<ActionResult<string>> GetStatus()
        {
            _logger.LogInformation("Received status check request.");
            var aiStatus = await _aiService.GetStatusAsync();
            return Ok($"MCP Server is running. {aiStatus}");
        }

        /// <summary>
        /// Nhận prompt từ frontend và trả về kết quả từ AI Assistant.
        /// </summary>
        /// <param name="request">Đối tượng chứa prompt từ người dùng.</param>
        /// <returns>Kết quả phản hồi từ AI Assistant.</returns>
        [HttpPost("query")]
        public async Task<ActionResult<string>> QueryAi([FromBody] AiQueryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest("Prompt cannot be empty.");
            }

            // Lấy JWT token từ header yêu cầu để truyền xuống FamilyTreeBackendService
            var jwtToken = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(jwtToken))
            {
                _logger.LogWarning("JWT Token not found in Authorization header for AI query.");
                // Depending on your security policy, you might return Unauthorized or proceed without RAG
                // For now, we'll proceed, but RAG will be limited.
            }

            _logger.LogInformation("Received AI query: {Prompt} from user {User}", request.Prompt, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var aiResponse = await _aiService.GetAiResponseAsync(request.Prompt, jwtToken);
            return Ok(aiResponse);
        }
    }

    /// <summary>
    /// Request DTO cho AI query.
    /// </summary>
    public class AiQueryRequest
    {
        public string Prompt { get; set; } = string.Empty;
    }
}