using backend.Application.Families.Commands.CreateFamily;
using backend.Application.Families.Commands.DeleteFamily;
using backend.Application.Families.Commands.UpdateFamily;
using backend.Application.Families.Queries.GetFamilies;
using backend.Application.Families.Queries.GetFamilyById;

namespace backend.Web.Endpoints;

public class Families : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            //.RequireAuthorization() // Uncomment this line to secure the endpoints
            .MapGet(GetFamilies)
            .MapGet(GetFamilyById, "{id}")
            .MapPost(CreateFamily)
            .MapPut(UpdateFamily, "{id}")
            .MapDelete(DeleteFamily, "{id}");
    }

    public async Task<List<FamilyDto>> GetFamilies(ISender sender)
    {
        return await sender.Send(new GetFamiliesQuery());
    }

    public async Task<FamilyDto> GetFamilyById(ISender sender, string id)
    {
        return await sender.Send(new GetFamilyByIdQuery(id));
    }

    public async Task<string> CreateFamily(ISender sender, CreateFamilyCommand command)
    {
        return await sender.Send(command);
    }

    public async Task<IResult> UpdateFamily(ISender sender, string id, UpdateFamilyCommand command)
    {
        if (id != command.Id) return Results.BadRequest();
        await sender.Send(command);
        return Results.NoContent();
    }

    public async Task<IResult> DeleteFamily(ISender sender, string id)
    {
        await sender.Send(new DeleteFamilyCommand(id));
        return Results.NoContent();
    }
}