using backend.Application.Families.Commands.CreateFamily;
using backend.Application.Families.Commands.DeleteFamily;
using backend.Application.Families.Commands.UpdateFamily;
using backend.Application.Families.Queries.GetFamilyById;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Infrastructure.IntegrationTests;

public class FamilyTests : IClassFixture<Testing>
{
    private readonly Testing _testing;

    public FamilyTests(Testing testing)
    {
        _testing = testing;
    }

    [Fact]
    public async Task ShouldCreateFamily() 
    {
        // Thiết lập: Tạo một lệnh tạo gia đình mới
        var command = new CreateFamilyCommand
        {
            Name = "Nguyễn",
            Description = "Gia đình Nguyễn",
            Visibility = "Private"
        };

        // Thực thi: Gửi lệnh tạo gia đình
        var familyId = (await Testing.SendAsync(command)).Value;

        // Xác minh: Kiểm tra xem gia đình đã được tạo thành công trong cơ sở dữ liệu chưa
        var family = await Testing.FindAsync<Family>(familyId);

        family.Should().NotBeNull();
        family!.Name.Should().Be(command.Name);
        family.Description.Should().Be(command.Description);
    }

    [Fact]
    public async Task ShouldUpdateFamily()
    {
        // Thiết lập: Tạo một gia đình mới
        var createCommand = new CreateFamilyCommand
        {
            Name = "Trần",
            Description = "Gia đình Trần",
            Visibility = "Private"
        };
        var familyId = (await Testing.SendAsync(createCommand)).Value;

        // Cập nhật: Tạo lệnh cập nhật gia đình
        var updateCommand = new UpdateFamilyCommand
        {
            Id = familyId,
            Name = "Trần Văn",
            Description = "Gia đình Trần Văn đã cập nhật"
        };
        await Testing.SendAsync(updateCommand);

        // Xác minh: Kiểm tra xem gia đình đã được cập nhật thành công chưa
        var updatedFamily = await Testing.FindAsync<Family>(familyId);

        updatedFamily.Should().NotBeNull();
        updatedFamily!.Name.Should().Be(updateCommand.Name);
        updatedFamily.Description.Should().Be(updateCommand.Description);
    }

    [Fact]
    public async Task ShouldDeleteFamily()
    {
        // Thiết lập: Tạo một gia đình mới
        var createCommand = new CreateFamilyCommand
        {
            Name = "Lê",
            Description = "Gia đình Lê",
            Visibility = "Private"
        };
        var familyId = (await Testing.SendAsync(createCommand)).Value;

        // Xóa: Tạo lệnh xóa gia đình
        var deleteCommand = new DeleteFamilyCommand(familyId);
        await Testing.SendAsync(deleteCommand);

        // Xác minh: Kiểm tra xem gia đình đã bị xóa khỏi cơ sở dữ liệu chưa
        var deletedFamily = await Testing.FindAsync<Family>(familyId);

        deletedFamily.Should().BeNull();
    }

    [Fact]
    public async Task ShouldGetFamilyById()
    {
        // Thiết lập: Tạo một gia đình mới
        var createCommand = new CreateFamilyCommand
        {
            Name = "Phạm",
            Description = "Gia đình Phạm",
            Visibility = "Private"
        };
        var familyId = (await Testing.SendAsync(createCommand)).Value;

        // Truy vấn: Lấy thông tin gia đình theo ID
        var query = new GetFamilyByIdQuery(familyId);
        var familyDto = await Testing.SendAsync(query);

        // Xác minh: Kiểm tra thông tin gia đình trả về
        familyDto.Should().NotBeNull();
        familyDto.Value.Should().NotBeNull();
        familyDto.Value!.Name.Should().Be(createCommand.Name);
        familyDto.Value!.Description.Should().Be(createCommand.Description);
    }
}