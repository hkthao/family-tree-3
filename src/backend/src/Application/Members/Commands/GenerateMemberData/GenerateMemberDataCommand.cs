using backend.Application.Common.Models;
using backend.Application.Members.Queries;

namespace backend.Application.Members.Commands.GenerateMemberData;

public record GenerateMemberDataCommand(string Prompt) : IRequest<Result<List<AIMemberDto>>>;