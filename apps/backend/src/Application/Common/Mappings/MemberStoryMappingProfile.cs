using backend.Application.MemberStories.Commands.CreateMemberStory; // Updated
using backend.Application.MemberStories.Commands.UpdateMemberStory; // Updated
using backend.Application.MemberStories.DTOs; // Updated
using backend.Domain.Entities;

namespace backend.Application.MemberStories; // Updated

public class MemberStoryMappingProfile : Profile
{
    public MemberStoryMappingProfile()
    {
        CreateMap<MemberStory, MemberStoryDto>()
            .ForMember(dest => dest.MemberFullName, opt => opt.MapFrom(src => src.Member.FullName))
            .ForMember(dest => dest.MemberAvatarUrl, opt => opt.MapFrom(src => src.Member.AvatarUrl))
            .ForMember(dest => dest.MemberGender, opt => opt.MapFrom(src => src.Member.Gender));
        CreateMap<CreateMemberStoryCommand, MemberStory>();
        CreateMap<UpdateMemberStoryCommand, MemberStory>();
    }
}
