using System.Text; // NEW
using backend.Application.AI.Commands.Chat.CallAiChatService;
using backend.Application.AI.Commands.DetermineChatContext;
using backend.Application.AI.DTOs;
using backend.Application.AI.Enums;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models; // Re-add this
using backend.Application.Common.Models.AppSetting;
using backend.Application.Common.Models.LLMGateway; // NEW
using backend.Application.Families.Commands.EnsureFamilyAiConfigExists;
using backend.Application.Families.Commands.GenerateFamilyData;
using backend.Application.Families.Commands.IncrementFamilyAiChatUsage;
using backend.Application.Families.Queries.CheckAiChatQuota;
using backend.Application.MemberFaces.Commands.DetectFaces;
using backend.Application.OCR.Commands; // NEW
using backend.Application.Prompts.Queries.GetPromptById;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace backend.Application.AI.Chat;

/// <summary>
/// Handler cho lệnh <see cref="ChatWithAssistantCommand"/>.
/// Đây là một Orchestrator, điều phối các hoạt động AI chat.
/// </summary>
public class ChatWithAssistantCommandHandler : IRequestHandler<ChatWithAssistantCommand, Result<ChatResponse>>
{
    private readonly ILogger<ChatWithAssistantCommandHandler> _logger;
    private readonly IMediator _mediator;
    private readonly LLMGatewaySettings _llmGatewaySettings; // Changed from ChatSettings
    private readonly IAuthorizationService _authorizationService;

    public ChatWithAssistantCommandHandler(
        ILogger<ChatWithAssistantCommandHandler> logger,
        IMediator mediator,
        IOptions<LLMGatewaySettings> llmGatewaySettings, // Inject IOptions<LLMGatewaySettings>
        IAuthorizationService authorizationService)
    {
        _logger = logger;
        _mediator = mediator;
        _llmGatewaySettings = llmGatewaySettings.Value; // Extract LLMGatewaySettings
        _authorizationService = authorizationService;
    }


    public async Task<Result<ChatResponse>> Handle(ChatWithAssistantCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Bắt đầu xử lý yêu cầu trợ lý chat cho FamilyId: {FamilyId}", request.FamilyId);

        // 2. Đảm bảo cấu hình giới hạn AI cho gia đình tồn tại
        var ensureConfigResult = await _mediator.Send(new EnsureFamilyAiConfigExistsCommand { FamilyId = request.FamilyId }, cancellationToken);
        if (!ensureConfigResult.IsSuccess)
        {
            string error = ensureConfigResult.Error ?? "Không thể đảm bảo cấu hình giới hạn AI của gia đình.";
            string errorSource = ensureConfigResult.ErrorSource ?? "Unknown";
            return Result<ChatResponse>.Failure(error, errorSource);
        }

        // 3. Kiểm tra hạn ngạch AI Chat (áp dụng cho cả chat và data generation)
        var quotaCheckResult = await _mediator.Send(new CheckAiChatQuotaQuery { FamilyId = request.FamilyId }, cancellationToken);
        if (!quotaCheckResult.IsSuccess)
        {
            string error = quotaCheckResult.Error ?? ErrorMessages.AiChatQuotaExceeded;
            string errorSource = quotaCheckResult.ErrorSource ?? ErrorSources.QuotaExceeded;
            return Result<ChatResponse>.Failure(error, errorSource);
        }

        // 4. Xác định ngữ cảnh tin nhắn
        _logger.LogInformation("Đang xác định ngữ cảnh cho tin nhắn chat.");
        var determineContextCommand = new DetermineChatContextCommand
        {
            ChatMessage = request.ChatInput,
            SessionId = request.SessionId
        };
        var contextResult = await _mediator.Send(determineContextCommand, cancellationToken);
        string contextReasoning = "Not provided.";

        ContextType determinedContext;
        if (contextResult.IsSuccess && contextResult.Value != null)
        {
            determinedContext = contextResult.Value.Context;
            contextReasoning = contextResult.Value.Reasoning ?? contextReasoning;
            _logger.LogInformation("Ngữ cảnh được xác định: {ContextType} với lý do: {Reasoning}", determinedContext, contextReasoning);
        }
        else
        {
            _logger.LogError("Không thể xác định ngữ cảnh chat: {Error}. Fallback về ngữ cảnh QA.", contextResult.Error);
            determinedContext = ContextType.QA; // Fallback to QA
        }

        // 5. Lấy System Prompt chung cho QA (dùng cho các ngữ cảnh chat thông thường)
        string? qaSystemPromptContent = null;
        var qaPromptResult = await _mediator.Send(new GetPromptByIdQuery { Code = PromptConstants.CHAT_QA_PROMPT }, cancellationToken);
        if (qaPromptResult.IsSuccess && qaPromptResult.Value?.Content != null)
        {
            qaSystemPromptContent = qaPromptResult.Value.Content;
        }

        if (string.IsNullOrWhiteSpace(qaSystemPromptContent))
        {
            return Result<ChatResponse>.Failure("Không thể cấu hình hệ thống AI chat. Vui lòng liên hệ quản trị viên.", ErrorSources.InvalidConfiguration);
        }

        // --- NEW: Lấy System Prompt cho Family Data Lookup --- 
        string? familyDataLookupSystemPromptContent = null;
        var familyDataLookupPromptResult = await _mediator.Send(new GetPromptByIdQuery { Code = PromptConstants.CHAT_FAMILY_DATA_LOOKUP_PROMPT }, cancellationToken);
        if (familyDataLookupPromptResult.IsSuccess && familyDataLookupPromptResult.Value?.Content != null)
        {
            familyDataLookupSystemPromptContent = familyDataLookupPromptResult.Value.Content;
        }

        if (string.IsNullOrWhiteSpace(familyDataLookupSystemPromptContent))
        {
            _logger.LogError("Không thể lấy system prompt '{PromptCode}' từ database và không có prompt mặc định. Trả về lỗi.", PromptConstants.CHAT_FAMILY_DATA_LOOKUP_PROMPT);
            return Result<ChatResponse>.Failure("Không thể cấu hình hệ thống AI chat. Vui lòng liên hệ quản trị viên.", ErrorSources.InvalidConfiguration);
        }
        // --- END NEW ---

        // 6. Xử lý dựa trên ngữ cảnh
        Result<ChatResponse> finalChatResponseResult;
        switch (determinedContext)
        {
            case ContextType.Unknown:
                _logger.LogInformation("Ngữ cảnh không xác định. Trả về phản hồi không xác định được yêu cầu.");
                finalChatResponseResult = Result<ChatResponse>.Success(new ChatResponse
                {
                    Output = "Xin lỗi, tôi không thể xác định được yêu cầu của bạn. Vui lòng thử lại với một câu hỏi rõ ràng hơn.",
                    Intent = null // No specific intent for unknown requests
                });
                break;

            case ContextType.QA:
                _logger.LogInformation("Ngữ cảnh là QA. Gọi dịch vụ LLM Gateway Chat với prompt QA.");
                var qaLlmChatRequest = new LLMChatCompletionRequest // New LLM DTO
                {
                    Model = _llmGatewaySettings.LlmModel, // Use configured LLM Model
                    Messages = new List<LLMMessage>
                    {
                        new LLMMessage { Role = "system", Content = qaSystemPromptContent! }, // System prompt
                        new LLMMessage { Role = "user", Content = request.ChatInput } // User input
                    }
                    // Temperature, MaxTokens, Stream can be defaulted or configured as needed in the future
                };
                finalChatResponseResult = await _mediator.Send(new CallAiChatServiceCommand { LLMChatCompletionRequest = qaLlmChatRequest }, cancellationToken);
                break;

            case ContextType.FamilyDataLookup:
                _logger.LogInformation("Ngữ cảnh là FamilyDataLookup. Gọi dịch vụ LLM Gateway Chat với prompt Family Data Lookup.");
                var familyDataLlmChatRequest = new LLMChatCompletionRequest // New LLM DTO
                {
                    Model = _llmGatewaySettings.LlmModel, // Use configured LLM Model
                    Messages = new List<LLMMessage>
                    {
                        new LLMMessage { Role = "system", Content = familyDataLookupSystemPromptContent! }, // System prompt
                        new LLMMessage { Role = "user", Content = request.ChatInput } // User input
                    }
                    // Temperature, MaxTokens, Stream can be defaulted or configured as needed in the future
                };
                finalChatResponseResult = await _mediator.Send(new CallAiChatServiceCommand { LLMChatCompletionRequest = familyDataLlmChatRequest }, cancellationToken);
                break;

            case ContextType.DataGeneration:
                _logger.LogInformation("Ngữ cảnh là DataGeneration. Xử lý tệp đính kèm và gọi lệnh tạo dữ liệu gia đình.");

                // Kiểm tra quyền: Chỉ admin hoặc quản lý gia đình mới được tạo dữ liệu
                var isAdmin = _authorizationService.IsAdmin();
                var canManageFamily = _authorizationService.CanManageFamily(request.FamilyId);

                if (!isAdmin && !canManageFamily)
                {
                    return Result<ChatResponse>.Failure("Bạn không có quyền tạo dữ liệu gia đình.", ErrorSources.Authorization);
                }

                var combinedChatInput = new StringBuilder(request.ChatInput ?? string.Empty);

                // NEW: Xử lý thông tin vị trí nếu được cung cấp
                if (request.Location != null)
                {
                    combinedChatInput.AppendLine().AppendLine("--- Thông tin vị trí được cung cấp ---");
                    combinedChatInput.Append($"[Location: Latitude={request.Location.Latitude}, Longitude={request.Location.Longitude}");
                    if (!string.IsNullOrWhiteSpace(request.Location.Address))
                    {
                        combinedChatInput.Append($", Address={request.Location.Address}");
                    }
                    if (!string.IsNullOrWhiteSpace(request.Location.Source)) // NEW
                    {
                        combinedChatInput.Append($", Source={request.Location.Source}"); // NEW
                    }
                    combinedChatInput.AppendLine("]");
                    combinedChatInput.AppendLine("------------------------------------");
                    _logger.LogInformation("Đã thêm thông tin vị trí vào ChatInput: Latitude={Latitude}, Longitude={Longitude}, Address={Address}",
                        request.Location.Latitude, request.Location.Longitude, request.Location.Address);
                }

                if (request.Attachments != null && request.Attachments.Any())
                {
                    foreach (var attachment in request.Attachments)
                    {
                        if (attachment.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ||
                            attachment.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                _logger.LogInformation("Đang xử lý tệp đính kèm qua OCR: {FileName} ({ContentType})", attachment.Url, attachment.ContentType);
                                var ocrResult = await _mediator.Send(new ProcessOcrFileCommand
                                {
                                    FileUrl = attachment.Url, // Pass URL directly
                                    ContentType = attachment.ContentType,
                                    FileName = Path.GetFileName(new Uri(attachment.Url).LocalPath)
                                }, cancellationToken);

                                if (ocrResult.IsSuccess && !string.IsNullOrWhiteSpace(ocrResult.Value?.Text))
                                {
                                    combinedChatInput.AppendLine().AppendLine("--- Nội dung được trích xuất từ tệp đính kèm ---");
                                    combinedChatInput.AppendLine(ocrResult.Value.Text);
                                    combinedChatInput.AppendLine("--------------------------------------------------");
                                    _logger.LogInformation("Đã trích xuất thành công nội dung từ tệp đính kèm {FileName}. Độ dài: {Length}", attachment.Url, ocrResult.Value.Text.Length);
                                }
                                else
                                {
                                    _logger.LogWarning("Không thể trích xuất nội dung từ tệp đính kèm {FileName} qua OCR: {Error}", attachment.Url, ocrResult.Error ?? "Kết quả Text rỗng");
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Lỗi khi xử lý tệp đính kèm {FileName} (OCR) từ URL {Url}", attachment.Url, attachment.ContentType);
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Tệp đính kèm '{FileName}' không phải là hình ảnh/PDF và sẽ bị bỏ qua trong ngữ cảnh DataGeneration.", attachment.Url);
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(combinedChatInput.ToString()))
                {
                    finalChatResponseResult = Result<ChatResponse>.Failure("Không có nội dung chat hoặc nội dung được trích xuất từ tệp đính kèm nào để tạo dữ liệu gia đình.", "DataGenerationError");
                    break;
                }

                var generateDataCommand = new GenerateFamilyDataCommand
                {
                    FamilyId = request.FamilyId,
                    ChatInput = combinedChatInput.ToString()
                };
                var generatedDataResult = await _mediator.Send(generateDataCommand, cancellationToken);

                if (!generatedDataResult.IsSuccess)
                {
                    string error = generatedDataResult.Error ?? "Không thể tạo dữ liệu gia đình.";
                    string errorSource = generatedDataResult.ErrorSource ?? "Unknown";
                    finalChatResponseResult = Result<ChatResponse>.Failure(error, errorSource);
                }
                else
                {
                    finalChatResponseResult = generatedDataResult.Value != null
                        ? Result<ChatResponse>.Success(new ChatResponse
                        {
                            Output = "Dữ liệu gia đình đã được AI phân tích thành công. Vui lòng kiểm tra kỹ tính chính xác của dữ liệu trước khi sử dụng.",
                            GeneratedData = generatedDataResult.Value
                        })
                        : Result<ChatResponse>.Failure("Lỗi không xác định khi tạo dữ liệu gia đình: Kết quả rỗng.", "InternalError");
                }
                break;

            case ContextType.RelationshipLookup:
                _logger.LogInformation("Ngữ cảnh là RelationshipLookup. Đề xuất chuyển hướng đến trang quản lý mối quan hệ.");
                finalChatResponseResult = Result<ChatResponse>.Success(new ChatResponse
                {
                    Output = "Để xác định và quản lý mối quan hệ giữa các thành viên, vui lòng truy cập trang quản lý mối quan hệ.",
                    Intent = IntentConstants.RELATIONSHIP_LOOKUP_PAGE // Use the constant here
                });
                break;

            case ContextType.ImageRecognition:
                _logger.LogInformation("Ngữ cảnh là ImageRecognition. Xử lý nhận dạng khuôn mặt từ tệp đính kèm.");

                if (request.Attachments == null || !request.Attachments.Any())
                {
                    finalChatResponseResult = Result<ChatResponse>.Failure("Không tìm thấy tệp đính kèm để nhận dạng hình ảnh.", "ImageRecognitionError");
                    break;
                }

                var faceDetectionResults = new List<FaceDetectionResponseDto>();
                foreach (var attachment in request.Attachments)
                {
                    // Basic check for image content types
                    if (attachment.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            _logger.LogInformation("Đang tải và xử lý hình ảnh cho nhận dạng khuôn mặt: {ImageUrl}", attachment.Url);
                            var processImageCommand = new ProcessOcrFileCommand
                            {
                                FileUrl = attachment.Url,
                                ContentType = attachment.ContentType,
                                FileName = Path.GetFileName(new Uri(attachment.Url).LocalPath)
                            };
                            var processedImageResult = await _mediator.Send(processImageCommand, cancellationToken);

                            if (!processedImageResult.IsSuccess || processedImageResult.Value?.ProcessedBytes == null)
                            {
                                _logger.LogWarning("Không thể tải hoặc xử lý hình ảnh từ URL {ImageUrl} để nhận dạng khuôn mặt: {Error}", attachment.Url, processedImageResult.Error);
                                continue; // Skip to next attachment
                            }

                            // Now use the processed bytes with DetectFacesCommand
                            var detectFacesCommand = new DetectFacesCommand
                            {
                                FamilyId = request.FamilyId,
                                ImageBytes = processedImageResult.Value.ProcessedBytes, // Use processed bytes
                                FileName = Path.GetFileName(new Uri(attachment.Url).LocalPath),
                                ContentType = attachment.ContentType,
                                ReturnCrop = true,
                                ResizeImageForAnalysis = false
                            };

                            var detectionResult = await _mediator.Send(detectFacesCommand, cancellationToken);
                            if (detectionResult.IsSuccess && detectionResult.Value != null)
                            {
                                detectionResult.Value.OriginalImageUrl = attachment.Url;
                                faceDetectionResults.Add(detectionResult.Value);
                            }
                            else
                            {
                                _logger.LogWarning("Không thể nhận dạng khuôn mặt từ URL {ImageUrl}: {Error}", attachment.Url, detectionResult.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Lỗi khi xử lý tệp đính kèm hình ảnh từ URL {ImageUrl}", attachment.Url);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Tệp đính kèm '{FileName}' không phải là hình ảnh và sẽ bị bỏ qua trong ngữ cảnh ImageRecognition.", attachment.Url);
                    }
                }

                if (!faceDetectionResults.Any())
                {
                    finalChatResponseResult = Result<ChatResponse>.Failure("Không tìm thấy khuôn mặt nào hoặc không thể xử lý bất kỳ hình ảnh nào có thể nhận dạng.", "ImageRecognitionError");
                }
                else
                {
                    int totalDetectedFaces = faceDetectionResults.Sum(res => res.DetectedFaces.Count);
                    int mappedMembers = faceDetectionResults.SelectMany(res => res.DetectedFaces)
                                                            .Count(face => face.MemberId.HasValue);

                    string outputMessage = $"Đã hoàn thành nhận dạng hình ảnh. Tìm thấy tổng cộng {totalDetectedFaces} khuôn mặt. Trong đó, {mappedMembers} khuôn mặt được nhận dạng là thành viên gia đình.";

                    finalChatResponseResult = Result<ChatResponse>.Success(new ChatResponse
                    {
                        Output = outputMessage,
                        FaceDetectionResults = faceDetectionResults,
                        Intent = IntentConstants.IMAGE_RECOGNITION_PAGE
                    });
                }
                break;

            default:
                _logger.LogWarning("Ngữ cảnh không được xử lý: {Context}. Mặc định về QA.", determinedContext);
                goto case ContextType.QA;
        }

        // 7. Ghi nhận sử dụng AI Chat
        var incrementUsageResult = await _mediator.Send(new IncrementFamilyAiChatUsageCommand { FamilyId = request.FamilyId }, cancellationToken);
        if (!incrementUsageResult.IsSuccess)
        {
            _logger.LogError("Không thể tăng hạn ngạch AI Chat cho FamilyId {FamilyId}: {Error}", request.FamilyId, incrementUsageResult.Error);
            // Vẫn trả về kết quả chính, nhưng log lỗi tăng hạn ngạch
        }

        _logger.LogInformation("AI Chat usage incremented for FamilyId {FamilyId}. Final result: {IsSuccess}",
            request.FamilyId, finalChatResponseResult.IsSuccess);

        return finalChatResponseResult;
    }


}

