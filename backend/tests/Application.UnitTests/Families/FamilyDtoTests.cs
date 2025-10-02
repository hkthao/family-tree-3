using AutoMapper;
using backend.Application.Common.Mappings;
using backend.Application.Families;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Families;

public class FamilyDtoTests
{
    private readonly IMapper _mapper;

    public FamilyDtoTests()
    {
        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        _mapper = configurationProvider.CreateMapper();
    }

    [Fact]
    public void Constructor_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Test Family";
        var description = "A test family description";
        var avatarUrl = "http://example.com/avatar.jpg";

        // Act
        var dto = new FamilyDto
        {
            Id = id,
            Name = name,
            Description = description,
            AvatarUrl = avatarUrl
        };

        // Assert
        dto.Id.Should().Be(id);
        dto.Name.Should().Be(name);
        dto.Description.Should().Be(description);
        dto.AvatarUrl.Should().Be(avatarUrl);
    }

    [Fact]
    public void ShouldMapFamilyToFamilyDto()
    {
        // Arrange
        var family = new Family
        {
            Id = Guid.NewGuid(),
            Name = "Mapped Family",
            Description = "Mapped description",
            AvatarUrl = "http://example.com/mapped_avatar.png"
        };

        // Act
        var dto = _mapper.Map<FamilyDto>(family);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(family.Id);
        dto.Name.Should().Be(family.Name);
        dto.Description.Should().Be(family.Description);
        dto.AvatarUrl.Should().Be(family.AvatarUrl);
    }
}
