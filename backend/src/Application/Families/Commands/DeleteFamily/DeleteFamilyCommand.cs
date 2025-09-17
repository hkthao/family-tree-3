using MediatR;

namespace backend.Application.Families.Commands.DeleteFamily;

public record DeleteFamilyCommand(string Id) : IRequest;