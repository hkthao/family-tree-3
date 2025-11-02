using AutoFixture.AutoMoq;
using AutoMapper;
using backend.Application.Relationships.Queries;
using backend.Application.Relationships.Queries.GetRelationshipById;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Queries.GetRelationshipById;

public class GetRelationshipByIdQueryHandlerTests : TestBase
{
    private readonly Mock<IMapper> _mockMapper;
    private readonly GetRelationshipByIdQueryHandler _handler;

    public GetRelationshipByIdQueryHandlerTests()
    {
        _mockMapper = new Mock<IMapper>();
        _fixture.Customize(new AutoMoqCustomization());

        // Mock ConfigurationProvider for ProjectTo
        _mockMapper.Setup(m => m.ConfigurationProvider).Returns(new MapperConfiguration(cfg => cfg.AddProfile<backend.Application.UnitTests.Common.MappingProfile>()).CreateMapper().ConfigurationProvider);

        _handler = new GetRelationshipByIdQueryHandler(
            _context,
            _mockMapper.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureWhenRelationshipNotFound()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi không tìm thấy mối quan hệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Đảm bảo không có mối quan hệ nào trong _context khớp với request.Id.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var query = new GetRelationshipByIdQuery(Guid.NewGuid()); // Non-existent ID

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Relationship with ID {query.Id} not found.");
        // 💡 Giải thích: Handler phải báo cáo lỗi khi mối quan hệ không tồn tại.
    }

    [Fact]
    public async Task Handle_ShouldReturnRelationshipDtoWhenRelationshipFound()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về RelationshipDto khi tìm thấy mối quan hệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thêm một mối quan hệ vào _context. Thiết lập _mapper để ánh xạ Relationship sang RelationshipDto.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chứa RelationshipDto mong đợi.
        var relationshipId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var sourceMemberId = Guid.NewGuid();
        var targetMemberId = Guid.NewGuid();

        var family = new Family { Id = familyId, Code = "FAM001", Name = "Test Family" };
        var sourceMember = new Member { Id = sourceMemberId, FamilyId = familyId, Code = "SM001", FirstName = "Source", LastName = "Member" };
        var targetMember = new Member { Id = targetMemberId, FamilyId = familyId, Code = "TM001", FirstName = "Target", LastName = "Member" };
        var relationship = new Relationship
        {
            Id = relationshipId,
            SourceMemberId = sourceMemberId,
            TargetMemberId = targetMemberId,
            Type = Domain.Enums.RelationshipType.Father,
            Order = 1,
            FamilyId = familyId
        };

        _context.Families.Add(family);
        _context.Members.AddRange(sourceMember, targetMember);
        _context.Relationships.Add(relationship);
        await _context.SaveChangesAsync();

        var expectedSourceMemberDto = new RelationshipMemberDto
        {
            Id = sourceMemberId,
            FullName = $"{sourceMember.FirstName} {sourceMember.LastName}"
        };
        var expectedTargetMemberDto = new RelationshipMemberDto
        {
            Id = targetMemberId,
            FullName = $"{targetMember.FirstName} {targetMember.LastName}"
        };

        var expectedDto = new RelationshipDto
        {
            Id = relationshipId,
            SourceMemberId = sourceMemberId,
            SourceMember = expectedSourceMemberDto,
            TargetMemberId = targetMemberId,
            TargetMember = expectedTargetMemberDto,
            Type = Domain.Enums.RelationshipType.Father,
            Order = 1,
            FamilyId = familyId
        };

        _mockMapper.Setup(m => m.Map<RelationshipDto>(It.IsAny<Relationship>()))
            .Returns(expectedDto);

        var query = new GetRelationshipByIdQuery(relationshipId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDto);
        // 💡 Giải thích: Handler phải trả về thông tin mối quan hệ khi tìm thấy.
    }
}
