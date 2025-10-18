using backend.Application.Common.Models;
using backend.Application.Members.Queries;

namespace Application.NaturalLanguageInput.Commands.GenerateData;

public record GenerateMemberDataCommand(string Prompt) : IRequest<Result<List<MemberDto>>>;
