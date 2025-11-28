using backend.Application.MemberStories.Commands.CreateMemberStory; // Updated
using backend.Application.MemberStories.Commands.UpdateMemberStory; // Updated
using backend.Application.MemberStories.DTOs; // Updated
using backend.Domain.Entities;

namespace backend.Application.MemberStories; // Updated

public class MemberStoryMappingProfile : Profile
{
    public MemberStoryMappingProfile()
    {
        CreateMap<MemberStory, MemberStoryDto>();
        CreateMap<CreateMemberStoryCommand, MemberStory>();
        CreateMap<UpdateMemberStoryCommand, MemberStory>();
    }
}
