using System.Dynamic;
using System.Text.Json; // For JsonSerializerOptions and deserialization
using backend.Application.AI.DTOs;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Queries; // For EventDto
using backend.Application.Families.Commands.IncrementFamilyAiChatUsage;
using backend.Application.Families.Queries; // For FamilyDto
using backend.Application.Members.Queries; // For MemberDto
using backend.Application.Prompts.Queries.GetPromptById;
using Microsoft.Extensions.Logging;

namespace backend.Application.AI.Commands.GenerateAiContent;

/// <summary>
/// Handler cho lệnh GenerateAiContentCommand.
/// </summary>
public class GenerateAiContentCommandHandler : IRequestHandler<GenerateAiContentCommand, Result<ExpandoObject>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;
    private readonly IAiGenerateService _aiGenerateService;
    private readonly ILogger<GenerateAiContentCommandHandler> _logger;

    public GenerateAiContentCommandHandler(IApplicationDbContext context, IMediator mediator, IAiGenerateService aiGenerateService, ILogger<GenerateAiContentCommandHandler> logger)
    {
        _context = context;
        _mediator = mediator;
        _aiGenerateService = aiGenerateService;
        _logger = logger;
    }

    public async Task<Result<ExpandoObject>> Handle(GenerateAiContentCommand request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra và tăng hạn mức sử dụng AI
        var incrementUsageResult = await _mediator.Send(new IncrementFamilyAiChatUsageCommand { FamilyId = request.FamilyId }, cancellationToken);
        if (!incrementUsageResult.IsSuccess)
        {
            _logger.LogWarning("AI content generation failed for family {FamilyId} due to quota: {Error}", request.FamilyId, incrementUsageResult.Error);
            return Result<ExpandoObject>.Failure(incrementUsageResult.Error ?? ErrorMessages.AiChatQuotaExceeded, incrementUsageResult.ErrorSource ?? ErrorSources.QuotaExceeded);
        }

        // 2. Lấy System Prompt dựa trên ContentType
        string promptCode = GetPromptCodeForContentType(request.ContentType);
        var promptResult = await _mediator.Send(new GetPromptByIdQuery { Code = promptCode }, cancellationToken);

        string systemPromptContent;
        if (promptResult.IsSuccess && promptResult.Value != null)
        {
            systemPromptContent = promptResult.Value.Content;
            _logger.LogInformation("Successfully fetched system prompt '{PromptCode}' from database.", promptCode);
        }
        else
        {
            _logger.LogWarning("Could not fetch system prompt '{PromptCode}' from database. Using default fallback. Error: {Error}", promptCode, promptResult.Error);
            systemPromptContent = GetFallbackSystemPrompt(request.ContentType); // Fallback to a generic prompt
        }

        // 3. Chuẩn bị yêu cầu AI
        var aiRequest = new GenerateRequest
        {
            SystemPrompt = systemPromptContent,
            ChatInput = request.ChatInput,
            SessionId = Guid.NewGuid().ToString(), // Tạo SessionId mới cho mỗi yêu cầu
            Metadata = new Dictionary<string, object>
            {
                { "familyId", request.FamilyId },
                { "contentType", request.ContentType }
            }
        };

        // 4. Gửi yêu cầu đến dịch vụ AI và phân tích phản hồi
        Result<ExpandoObject> finalResult;
        switch (request.ContentType.ToLower())
        {
            case "family":
                var familyDtoResult = await _aiGenerateService.GenerateDataAsync<FamilyDto>(aiRequest, cancellationToken);
                finalResult = ConvertResultToExpando(familyDtoResult);
                break;
            case "member":
                var memberDtoResult = await _aiGenerateService.GenerateDataAsync<MemberDto>(aiRequest, cancellationToken);
                finalResult = ConvertResultToExpando(memberDtoResult);
                break;
            case "event":
                var eventDtoResult = await _aiGenerateService.GenerateDataAsync<EventDto>(aiRequest, cancellationToken);
                finalResult = ConvertResultToExpando(eventDtoResult);
                break;
            case "chat":
            default: // Default case also handles "chat" and any other unexpected types as plain text
                var stringResult = await _aiGenerateService.GenerateDataAsync<string>(aiRequest, cancellationToken);
                if (!stringResult.IsSuccess || string.IsNullOrEmpty(stringResult.Value))
                {
                    return Result<ExpandoObject>.Failure(stringResult.Error ?? ErrorMessages.NoAIResponse, stringResult.ErrorSource ?? ErrorSources.ExternalServiceError);
                }
                dynamic expandoText = new ExpandoObject();
                expandoText.Text = stringResult.Value;
                finalResult = Result<ExpandoObject>.Success(expandoText);
                break;
        }

        return finalResult;
    }

    private Result<ExpandoObject> ConvertResultToExpando<T>(Result<T> result)
    {
        if (!result.IsSuccess)
        {
            return Result<ExpandoObject>.Failure(result.Error ?? "Lỗi không xác định khi tạo nội dung AI.", result.ErrorSource ?? ErrorSources.ExternalServiceError);
        }

        if (result.Value == null)
        {
            return Result<ExpandoObject>.Failure(ErrorMessages.NoAIResponse, ErrorSources.ExternalServiceError);
        }

        var jsonString = JsonSerializer.Serialize(result.Value);
        var jsonDocument = JsonDocument.Parse(jsonString);
        var expando = new ExpandoObject();
        AddJsonElementToExpando(expando, jsonDocument.RootElement);
        return Result<ExpandoObject>.Success(expando);
    }

    private string GetPromptCodeForContentType(string contentType)
    {
        return contentType.ToLower() switch
        {
            "family" => PromptConstants.FamilyDataGenerationPromptCode,
            "member" => PromptConstants.GenerateMemberBiographyPromptCode,
            "event" => PromptConstants.GenerateEventDetailsPromptCode,
            "chat" => PromptConstants.AiAssistantChatPromptCode,
            _ => throw new ArgumentException($"Invalid ContentType: {contentType}")
        };
    }

    private string GetFallbackSystemPrompt(string contentType)
    {
        return contentType.ToLower() switch
        {
            "family" => "Bạn là một AI tạo dữ liệu gia đình có cấu trúc từ văn bản người dùng. Phản hồi bằng JSON.",
            "member" => "Bạn là một AI tạo tiểu sử thành viên. Phản hồi bằng JSON.",
            "event" => "Bạn là một AI tạo chi tiết sự kiện. Phản hồi bằng JSON.",
            "chat" => "Bạn là một trợ lý AI hữu ích.",
            _ => "Bạn là một trợ lý AI hữu ích."
        };
    }

    private void AddJsonElementToExpando(ExpandoObject expando, JsonElement jsonElement)
    {
        if (jsonElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in jsonElement.EnumerateObject())
            {
                ((IDictionary<string, object?>)expando)[property.Name] = ConvertJsonElementToDynamic(property.Value);
            }
        }
        else if (jsonElement.ValueKind == JsonValueKind.Array)
        {
            var list = new List<object?>();
            foreach (var item in jsonElement.EnumerateArray())
            {
                list.Add(ConvertJsonElementToDynamic(item));
            }
            // If expando is supposed to be an array, this requires different handling
            // For now, assuming top-level expando is object
        }
    }

    private object? ConvertJsonElementToDynamic(JsonElement jsonElement)
    {
        return jsonElement.ValueKind switch
        {
            JsonValueKind.Object => ConvertJsonElementToExpandoObject(jsonElement),
            JsonValueKind.Array => jsonElement.EnumerateArray().Select(ConvertJsonElementToDynamic).ToList(),
            JsonValueKind.String =>
                jsonElement.GetString() switch
                {
                    string s when Guid.TryParse(s, out var guid) => guid,
                    string s => s,
                    _ => null
                },
            JsonValueKind.Number => jsonElement.GetDecimal(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => throw new NotSupportedException($"Unsupported JsonValueKind: {jsonElement.ValueKind}")
        };
    }

    private ExpandoObject ConvertJsonElementToExpandoObject(JsonElement jsonElement)
    {
        var expando = new ExpandoObject();
        if (jsonElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in jsonElement.EnumerateObject())
            {
                ((IDictionary<string, object?>)expando)[property.Name] = ConvertJsonElementToDynamic(property.Value);
            }
        }
        return expando;
    }
}
