using AutoMapper;
using backend.Application.Common.Exceptions;
using backend.Application.Common.Mappings;
using backend.Application.Members.Queries.GetMemberById;
using backend.Domain.Entities;
using FluentAssertions;
using backend.Application.Common.Interfaces;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Queries.GetMemberById;

public class GetMemberByIdQueryHandlerTests
{
    private readonly GetMemberByIdQueryHandler _handler;
    private readonly Mock<IMemberRepository> _mockMemberRepository;
    private readonly IMapper _mapper;

    public GetMemberByIdQueryHandlerTests()
    {
        _mockMemberRepository = new Mock<IMemberRepository>();

        // Setup AutoMapper
        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = configurationProvider.CreateMapper();

        _handler = new GetMemberByIdQueryHandler(_mockMemberRepository.Object, _mapper);
    }

    [Fact]
    public async Task Handle_Should_Return_Member_When_Found()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var member = new Member { Id = memberId, FirstName = "Test", LastName = "Member" };
        _mockMemberRepository.Setup(repo => repo.GetByIdAsync(memberId)).ReturnsAsync(member);

        // Act
        var result = await _handler.Handle(new GetMemberByIdQuery(memberId), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(memberId);
        result.FirstName.Should().Be(member.FirstName);
        result.LastName.Should().Be(member.LastName);
    }

        [Fact]
        public async Task Handle_Should_Throw_NotFoundException_When_Not_Found()
        {
            // Arrange
            var nonExistentMemberId = Guid.NewGuid();
            var command = new GetMemberByIdQuery(nonExistentMemberId);
            _mockMemberRepository.Setup(repo => repo.GetByIdAsync(nonExistentMemberId)).ReturnsAsync((Member)null!);
    
            // Act
            var act = () => _handler.Handle(command, CancellationToken.None);
    
            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }}
