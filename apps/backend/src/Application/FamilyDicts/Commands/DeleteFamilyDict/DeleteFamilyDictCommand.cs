namespace backend.Application.FamilyDicts.Commands.DeleteFamilyDict;

public record DeleteFamilyDictCommand(Guid Id) : IRequest;
