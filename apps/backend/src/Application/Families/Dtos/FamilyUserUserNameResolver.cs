using AutoMapper;
using backend.Domain.Entities;
using backend.Application.Families.Dtos;

namespace backend.Application.Families.Dtos;

public class FamilyUserUserNameResolver : IValueResolver<FamilyUser, FamilyUserDto, string?>
{
    public string? Resolve(FamilyUser source, FamilyUserDto destination, string? destMember, ResolutionContext context)
    {
        if (source.User != null)
        {
            if (source.User.Profile != null)
            {
                return source.User.Profile.Name;
            }
            return source.User.Email;
        }
        return null;
    }
}
