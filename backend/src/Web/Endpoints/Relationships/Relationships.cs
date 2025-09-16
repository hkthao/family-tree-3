using backend.Application.Common.Interfaces;
using backend.Application.Relationships.Commands.CreateRelationship;
using backend.Application.Relationships.Commands.DeleteRelationship;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Threading.Tasks;

namespace backend.Web.Endpoints;

public class Relationships : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapPost("/", CreateRelationship);
        group.MapDelete("/{id}", DeleteRelationship);
    }

    public async Task<Created<string>> CreateRelationship(ISender sender, CreateRelationshipCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/relationships/{id}", id);
    }

    public async Task<Results<NoContent, NotFound>> DeleteRelationship(ISender sender, string id)
    {
        await sender.Send(new DeleteRelationshipCommand(id));
        return TypedResults.NoContent();
    }
}
