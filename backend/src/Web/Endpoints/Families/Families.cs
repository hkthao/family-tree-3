using backend.Application.Families;
using backend.Application.Families.Commands.CreateFamily;
using backend.Application.Families.Commands.DeleteFamily;
using backend.Application.Families.Commands.UpdateFamily;
using backend.Application.Families.Queries.GetFamilies;
using backend.Application.Families.Queries.GetFamilyById;

namespace backend.Web.Endpoints;

public class Families : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetFamilies);
        group.MapGet("{id}", GetFamilyById);
        group.MapPost("/", CreateFamily);
        group.MapPut("{id}", UpdateFamily);
        group.MapDelete("{id}", DeleteFamily);
        //.RequireAuthorization(); // Uncomment this line to secure the endpoints
    }

    public async Task<List<FamilyDto>> GetFamilies(ISender sender)
    {
        return await sender.Send(new GetFamiliesQuery());
    }

    public async Task<FamilyDto> GetFamilyById(ISender sender, Guid id)
    {
        return await sender.Send(new GetFamilyByIdQuery(id));
    }

    public async Task<Guid> CreateFamily(ISender sender, CreateFamilyCommand command)
    {
        return await sender.Send(command);
    }

    public async Task<IResult> UpdateFamily(ISender sender, Guid id, UpdateFamilyCommand command)
    {
        if (id != command.Id) return Results.BadRequest();
        await sender.Send(command);
        return Results.NoContent();
    }

    public async Task<IResult> DeleteFamily(ISender sender, Guid id)
    {
        await sender.Send(new DeleteFamilyCommand(id));
        return Results.NoContent();
    }
}