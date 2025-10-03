using AutoMapper;
using backend.Application.Common.Mappings;
using backend.Application.Members;
using backend.Application.Members.Queries.GetMembers;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Application.Common.Interfaces;

namespace backend.Application.UnitTests.Members.Queries.GetMembers;

public class GetMembersQueryHandlerTests
{
    private readonly GetMembersQueryHandler _handler;
    private readonly Mock<IMemberRepository> _mockMemberRepository;
    private readonly IMapper _mapper;

    public GetMembersQueryHandlerTests()
    {
        _mockMemberRepository = new Mock<IMemberRepository>();

        // Setup AutoMapper
        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = configurationProvider.CreateMapper();

        _handler = new GetMembersQueryHandler(_mockMemberRepository.Object, _mapper);
    }

    [Fact]
    public async Task Handle_Should_Return_All_Members()
    {
        // Arrange
        var members = new List<Member>
        {
            new Member { Id = Guid.NewGuid(), FirstName = "Member", LastName = "1", FamilyId = Guid.NewGuid() },
            new Member { Id = Guid.NewGuid(), FirstName = "Member", LastName = "2", FamilyId = Guid.NewGuid() }
        };
        _mockMemberRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(members);

        // Act
        var result = await _handler.Handle(new GetMembersQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainEquivalentOf(new MemberDto { Id = members[0].Id, FirstName = "Member", LastName = "1" });
        result.Should().ContainEquivalentOf(new MemberDto { Id = members[1].Id, FirstName = "Member", LastName = "2" });
    }

    [Fact]
    public async Task Handle_Should_Return_EmptyList_When_NoMembersExist()
    {
        // Arrange - no members added to context
        _mockMemberRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Member>());

        // Act
        var result = await _handler.Handle(new GetMembersQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
