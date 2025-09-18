namespace backend.Application.Families.Commands.DeleteFamily;

public record DeleteFamilyCommand(Guid Id) : IRequest;