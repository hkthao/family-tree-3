using backend.Application.Common.Models;
using MediatR;
using backend.Application.Prompts.DTOs; // Added

namespace backend.Application.Prompts.Commands.ImportPrompts;

/// <summary>
/// Đại diện cho lệnh nhập danh sách lời nhắc.
/// </summary>
/// <param name="Prompts">Danh sách các đối tượng ImportPromptItemDto để nhập.</param>
public record ImportPromptsCommand(List<ImportPromptItemDto> Prompts) : IRequest<Result<List<PromptDto>>>;