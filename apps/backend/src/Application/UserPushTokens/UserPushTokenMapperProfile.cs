using AutoMapper;
using backend.Application.UserPushTokens.Commands;
using backend.Application.UserPushTokens.DTOs;
using backend.Domain.Entities;

namespace backend.Application.UserPushTokens;

public class UserPushTokenMapperProfile : Profile
{
    public UserPushTokenMapperProfile()
    {
        CreateMap<UserPushToken, UserPushTokenDto>();
        CreateMap<CreateUserPushTokenInput, UserPushToken>();
        CreateMap<UpdateUserPushTokenInput, UserPushToken>();
    }
}
