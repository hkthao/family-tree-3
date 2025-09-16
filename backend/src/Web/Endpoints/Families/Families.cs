using backend.Application.Common.Interfaces;
using backend.Application.Families.Commands.CreateFamily;
using backend.Application.Families.Commands.DeleteFamily;
using backend.Application.Families.Commands.UpdateFamily;
using backend.Application.Families.Queries.GetFamilies;
using backend.Application.Families.Queries.GetFamilyById;
using backend.Application.Families;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Threading.Tasks;

namespace backend.Web.Endpoints;

public class Families : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetFamilies);
        group.MapGet("/{id}", GetFamilyById);
        group.MapPost("/", CreateFamily);
        group.MapPut("/{id}", UpdateFamily);
        group.MapDelete("/{id}", DeleteFamily);
    }

    public async Task<Ok<List<FamilyDto>>> GetFamilies(ISender sender)
    {
        var result = await sender.Send(new GetFamiliesQuery());
        return TypedResults.Ok(result);
    }

    public async Task<Ok<FamilyDto>> GetFamilyById(ISender sender, string id)
    {
        var result = await sender.Send(new GetFamilyByIdQuery(id));
        return TypedResults.Ok(result);
    }

    public async Task<Created<string>> CreateFamily(ISender sender, CreateFamilyCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/families/{id}", id);
    }

    public async Task<Results<NoContent, NotFound>> UpdateFamily(ISender sender, string id, UpdateFamilyCommand command)
    {
        if (id != command.Id)
        {
            return TypedResults.NotFound();
        }
        await sender.Send(command);
        return TypedResults.NoContent();
    }

    public async Task<Results<NoContent, NotFound>> DeleteFamily(ISender sender, string id)
    {
        await sender.Send(new DeleteFamilyCommand(id));
        return TypedResults.NoContent();
    }
}
