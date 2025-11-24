using AutoMapper;
using backend.Application.Memories.Commands.CreateMemory;
using backend.Application.Memories.Commands.UpdateMemory;
using backend.Application.Memories.DTOs;
using backend.Domain.Entities;

namespace backend.Application.Memories;

public class MemoryMappingProfile : Profile
{
    public MemoryMappingProfile()
    {
        CreateMap<Memory, MemoryDto>();
        CreateMap<CreateMemoryCommand, Memory>();
        CreateMap<UpdateMemoryCommand, Memory>();
        CreateMap<PhotoAnalysisResult, PhotoAnalysisResultDto>();
    }
}
