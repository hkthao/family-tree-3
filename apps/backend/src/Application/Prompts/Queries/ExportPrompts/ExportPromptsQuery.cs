using MediatR;
using backend.Application.Common.Models; // Added
using backend.Application.Prompts.DTOs; // Added

namespace backend.Application.Prompts.Queries.ExportPrompts;

/// <summary>
/// Đại diện cho truy vấn để xuất tất cả lời nhắc.
/// </summary>
public record ExportPromptsQuery : IRequest<Result<List<PromptDto>>>;