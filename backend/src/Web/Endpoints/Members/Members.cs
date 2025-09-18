using backend.Application.Common.Interfaces;
using backend.Application.Members.Commands.CreateMember;
using backend.Application.Members.Commands.DeleteMember;
using backend.Application.Members.Commands.UpdateMember;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.Members.Queries.GetMemberById;
using backend.Application.Members;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Threading.Tasks;

namespace backend.Web.Endpoints;

public class Members : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetMembers);
        group.MapGet("/{id}", GetMemberById);
        group.MapPost("/", CreateMember);
        group.MapPut("/{id}", UpdateMember);
        group.MapDelete("/{id}", DeleteMember);
    }

    public async Task<Ok<List<MemberDto>>> GetMembers(ISender sender)
    {
        var result = await sender.Send(new GetMembersQuery());
        return TypedResults.Ok(result);
    }

    public async Task<Ok<MemberDto>> GetMemberById(ISender sender, Guid id)
    {
        var result = await sender.Send(new GetMemberByIdQuery(id));
        return TypedResults.Ok(result);
    }

    public async Task<Created<Guid>> CreateMember(ISender sender, CreateMemberCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/members/{id}", id);
    }

    public async Task<Results<NoContent, NotFound>> UpdateMember(ISender sender, Guid id, UpdateMemberCommand command)
    {
        if (id != command.Id)
        {
            return TypedResults.NotFound();
        }
        await sender.Send(command);
        return TypedResults.NoContent();
    }

    public async Task<Results<NoContent, NotFound>> DeleteMember(ISender sender, Guid id)
    {
        await sender.Send(new DeleteMemberCommand(id));
        return TypedResults.NoContent();
    }
}
