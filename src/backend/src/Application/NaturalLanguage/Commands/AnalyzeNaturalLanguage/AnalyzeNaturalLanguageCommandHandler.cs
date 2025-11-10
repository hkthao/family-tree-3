using MediatR;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Exceptions;
using backend.Application.AI.Prompts;
using backend.Application.NaturalLanguage.Models;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using FluentValidation.Results;

namespace backend.Application.NaturalLanguage.Commands.AnalyzeNaturalLanguage;

/// <summary>
/// Xử lý lệnh phân tích văn bản ngôn ngữ tự nhiên.
/// </summary>
public class AnalyzeNaturalLanguageCommandHandler : IRequestHandler<AnalyzeNaturalLanguageCommand, Result<AnalyzedDataDto>>
{
    private readonly IN8nService _n8nService;
    private readonly IApplicationDbContext _context;

    public AnalyzeNaturalLanguageCommandHandler(IN8nService n8nService, IApplicationDbContext context)
    {
        _n8nService = n8nService;
        _context = context;
    }

    public async Task<Result<AnalyzedDataDto>> Handle(AnalyzeNaturalLanguageCommand request, CancellationToken cancellationToken)
    {
        var sessionId = request.SessionId;

        // 1. Xây dựng prompt cho AI Agent
        var aiPrompt = PromptBuilder.BuildNaturalLanguageAnalysisPrompt(request.Content);

        // 2. Gửi prompt đến n8n Service chat
        var n8nResult = await _n8nService.CallChatWebhookAsync(sessionId, aiPrompt, cancellationToken);

        if (!n8nResult.IsSuccess)
        {
            throw new Common.Exceptions.ValidationException(n8nResult.Error ?? "Lỗi không xác định từ N8nService.");
        }

        // 3. Phân tích phản hồi từ AI (dự kiến là JSON)
        if (string.IsNullOrWhiteSpace(n8nResult.Value))
        {
            return Result<AnalyzedDataDto>.Failure("Phản hồi từ AI trống hoặc không hợp lệ.", "AIResponse");
        }

        try
        {
            var analyzedData = JsonSerializer.Deserialize<AnalyzedDataDto>(n8nResult.Value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (analyzedData == null)
            {
                return Result<AnalyzedDataDto>.Failure("Phản hồi từ AI không hợp lệ hoặc trống.", "AIResponse");
            }

            // 4. Map thông tin, kiểm tra sự tồn tại và validate từng thành viên
            var familyId = request.FamilyId;
            var memberValidator = new MemberDataDtoValidator();

            if (familyId != Guid.Empty)
            {
                foreach (var member in analyzedData.Members)
                {
                    // Kiểm tra sự tồn tại
                    Domain.Entities.Member? existingMember = null;
                    if (!string.IsNullOrWhiteSpace(member.Code))
                    {
                        existingMember = await _context.Members
                            .AsNoTracking()
                            .FirstOrDefaultAsync(m => m.Code == member.Code && m.FamilyId == familyId, cancellationToken);
                    }
                    if (existingMember != null)
                    {
                        member.IsExisting = true;
                        member.Id = existingMember.Id;
                    }

                    // Validate và gán lỗi nếu có
                    var validationResult = await memberValidator.ValidateAsync(member, cancellationToken);
                    if (!validationResult.IsValid)
                    {
                        member.ErrorMessage = string.Join(" ", validationResult.Errors.Select(e => e.ErrorMessage));
                    }
                }
            }

            // 5. Validate từng sự kiện
            var eventValidator = new EventDataDtoValidator();
            foreach (var ev in analyzedData.Events)
            {
                var validationResult = await eventValidator.ValidateAsync(ev, cancellationToken);
                if (!validationResult.IsValid)
                {
                    ev.ErrorMessage = string.Join(" ", validationResult.Errors.Select(e => e.ErrorMessage));
                }
            }

            // Luôn trả về success, thông tin lỗi nằm trong từng DTO
            return Result<AnalyzedDataDto>.Success(analyzedData);
        }
        catch (JsonException ex)
        {
            return Result<AnalyzedDataDto>.Failure($"Không thể phân tích phản hồi JSON từ AI: {ex.Message}", "AIResponseParsing");
        }
        catch (Exception ex)
        {
            return Result<AnalyzedDataDto>.Failure($"Đã xảy ra lỗi khi xử lý phản hồi từ AI: {ex.Message}", "AIResponseProcessing");
        }
    }
}
