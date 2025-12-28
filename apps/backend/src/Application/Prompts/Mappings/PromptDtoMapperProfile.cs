using backend.Application.Prompts.DTOs;
using backend.Domain.Entities;
using backend.Application.Prompts.Commands.ImportPrompts; // Added

namespace backend.Application.Prompts.Mappings;

public class PromptDtoMapperProfile : Profile
{
    public PromptDtoMapperProfile()
    {
        CreateMap<Prompt, PromptDto>();
        CreateMap<ImportPromptItemDto, Prompt>(); // Added
    }
}
