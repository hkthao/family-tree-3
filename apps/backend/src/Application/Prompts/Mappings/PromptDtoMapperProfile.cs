using backend.Application.Prompts.Commands.ImportPrompts; // Added
using backend.Application.Prompts.DTOs;
using backend.Domain.Entities;

namespace backend.Application.Prompts.Mappings;

public class PromptDtoMapperProfile : Profile
{
    public PromptDtoMapperProfile()
    {
        CreateMap<Prompt, PromptDto>();
        CreateMap<ImportPromptItemDto, Prompt>(); // Added
    }
}
