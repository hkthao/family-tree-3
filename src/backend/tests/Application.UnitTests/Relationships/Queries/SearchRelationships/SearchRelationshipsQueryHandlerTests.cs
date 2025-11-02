using AutoFixture.AutoMoq;
using backend.Application.Relationships.Queries;
using backend.Application.Relationships.Queries.SearchRelationships;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Queries.SearchRelationships;

public class SearchRelationshipsQueryHandlerTests : TestBase
{
    private readonly SearchRelationshipsQueryHandler _handler;

    public SearchRelationshipsQueryHandlerTests()
    {
        _fixture.Customize(new AutoMoqCustomization());
        _handler = new SearchRelationshipsQueryHandler(
            _context,
            _mapper
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnAllRelationshipsWhenNoFiltersProvided()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về tất cả các mối quan hệ khi không có bộ lọc nào được cung cấp.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thêm một số mối quan hệ vào _context. Thiết lập _mapper.
        // 2. Act: Gọi phương thức Handle với một SearchRelationshipsQuery không có bộ lọc.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chứa tất cả các mối quan hệ.
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Code = "FAM001", Name = "Test Family" };
        _context.Families.Add(family);

        var sourceMember1 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "SM001", FirstName = "Source1", LastName = "Member1" };
        var targetMember1 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "TM001", FirstName = "Target1", LastName = "Member1" };
        var sourceMember2 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "SM002", FirstName = "Source2", LastName = "Member2" };
        var targetMember2 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "TM002", FirstName = "Target2", LastName = "Member2" };
        _context.Members.AddRange(sourceMember1, targetMember1, sourceMember2, targetMember2);

        var relationship1 = new Relationship { Id = Guid.NewGuid(), SourceMemberId = sourceMember1.Id, TargetMemberId = targetMember1.Id, Type = RelationshipType.Father, FamilyId = familyId };
        var relationship2 = new Relationship { Id = Guid.NewGuid(), SourceMemberId = sourceMember2.Id, TargetMemberId = targetMember2.Id, Type = RelationshipType.Mother, FamilyId = familyId };
        _context.Relationships.AddRange(relationship1, relationship2);
        await _context.SaveChangesAsync();

        var expectedDto1 = new RelationshipListDto { Id = relationship1.Id, SourceMemberId = relationship1.SourceMemberId, TargetMemberId = relationship1.TargetMemberId, Type = relationship1.Type, SourceMember = new RelationshipMemberDto { Id = sourceMember1.Id, FullName = sourceMember1.FirstName + " " + sourceMember1.LastName }, TargetMember = new RelationshipMemberDto { Id = targetMember1.Id, FullName = targetMember1.FirstName + " " + targetMember1.LastName } };
        var expectedDto2 = new RelationshipListDto { Id = relationship2.Id, SourceMemberId = relationship2.SourceMemberId, TargetMemberId = relationship2.TargetMemberId, Type = relationship2.Type, SourceMember = new RelationshipMemberDto { Id = sourceMember2.Id, FullName = sourceMember2.FirstName + " " + sourceMember2.LastName }, TargetMember = new RelationshipMemberDto { Id = targetMember2.Id, FullName = targetMember2.FirstName + " " + targetMember2.LastName } };

        var query = new SearchRelationshipsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.Items.Should().ContainEquivalentOf(expectedDto1);
        result.Value.Items.Should().ContainEquivalentOf(expectedDto2);
        // 💡 Giải thích: Handler phải trả về tất cả các mối quan hệ khi không có tiêu chí lọc.
    }

    [Fact]
    public async Task Handle_ShouldFilterRelationshipsBySourceMemberId()
    {
        // 🎯 Mục tiêu của test: Xác minh handler lọc các mối quan hệ theo SourceMemberId.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thêm một số mối quan hệ vào _context.
        // 2. Act: Gọi phương thức Handle với một SearchRelationshipsQuery có SourceMemberId được chỉ định.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chỉ chứa các mối quan hệ có SourceMemberId đó.
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Code = "FAM001", Name = "Test Family" };
        _context.Families.Add(family);

        var sourceMember1 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "SM001", FirstName = "Source1", LastName = "Member1" };
        var targetMember1 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "TM001", FirstName = "Target1", LastName = "Member1" };
        var sourceMember2 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "SM002", FirstName = "Source2", LastName = "Member2" };
        var targetMember2 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "TM002", FirstName = "Target2", LastName = "Member2" };
        _context.Members.AddRange(sourceMember1, targetMember1, sourceMember2, targetMember2);

        var relationship1 = new Relationship { Id = Guid.NewGuid(), SourceMemberId = sourceMember1.Id, TargetMemberId = targetMember1.Id, Type = RelationshipType.Father, FamilyId = familyId };
        var relationship2 = new Relationship { Id = Guid.NewGuid(), SourceMemberId = sourceMember2.Id, TargetMemberId = targetMember2.Id, Type = RelationshipType.Mother, FamilyId = familyId };
        _context.Relationships.AddRange(relationship1, relationship2);
        await _context.SaveChangesAsync();

        var expectedDto1 = new RelationshipListDto { Id = relationship1.Id, SourceMemberId = relationship1.SourceMemberId, TargetMemberId = relationship1.TargetMemberId, Type = relationship1.Type, SourceMember = new RelationshipMemberDto { Id = sourceMember1.Id, FullName = sourceMember1.FirstName + " " + sourceMember1.LastName }, TargetMember = new RelationshipMemberDto { Id = targetMember1.Id, FullName = targetMember1.FirstName + " " + targetMember1.LastName } };

        var query = new SearchRelationshipsQuery { SourceMemberId = sourceMember1.Id };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.Should().ContainEquivalentOf(expectedDto1);
        // 💡 Giải thích: Handler chỉ nên trả về các mối quan hệ có SourceMemberId được chỉ định.
    }

    [Fact]
    public async Task Handle_ShouldFilterRelationshipsByTargetMemberId()
    {
        // 🎯 Mục tiêu của test: Xác minh handler lọc các mối quan hệ theo TargetMemberId.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thêm một số mối quan hệ vào _context.
        // 2. Act: Gọi phương thức Handle với một SearchRelationshipsQuery có TargetMemberId được chỉ định.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chỉ chứa các mối quan hệ có TargetMemberId đó.
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Code = "FAM001", Name = "Test Family" };
        _context.Families.Add(family);

        var sourceMember1 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "SM001", FirstName = "Source1", LastName = "Member1" };
        var targetMember1 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "TM001", FirstName = "Target1", LastName = "Member1" };
        var sourceMember2 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "SM002", FirstName = "Source2", LastName = "Member2" };
        var targetMember2 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "TM002", FirstName = "Target2", LastName = "Member2" };
        _context.Members.AddRange(sourceMember1, targetMember1, sourceMember2, targetMember2);

        var relationship1 = new Relationship { Id = Guid.NewGuid(), SourceMemberId = sourceMember1.Id, TargetMemberId = targetMember1.Id, Type = RelationshipType.Father, FamilyId = familyId };
        var relationship2 = new Relationship { Id = Guid.NewGuid(), SourceMemberId = sourceMember2.Id, TargetMemberId = targetMember2.Id, Type = RelationshipType.Mother, FamilyId = familyId };
        _context.Relationships.AddRange(relationship1, relationship2);
        await _context.SaveChangesAsync();

        var expectedDto1 = new RelationshipListDto { Id = relationship1.Id, SourceMemberId = relationship1.SourceMemberId, TargetMemberId = relationship1.TargetMemberId, Type = relationship1.Type, SourceMember = new RelationshipMemberDto { Id = sourceMember1.Id, FullName = sourceMember1.FirstName + " " + sourceMember1.LastName }, TargetMember = new RelationshipMemberDto { Id = targetMember1.Id, FullName = targetMember1.FirstName + " " + targetMember1.LastName } };

        var query = new SearchRelationshipsQuery { TargetMemberId = targetMember1.Id };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.Should().ContainEquivalentOf(expectedDto1);
        // 💡 Giải thích: Handler chỉ nên trả về các mối quan hệ có TargetMemberId được chỉ định.
    }

    [Fact]
    public async Task Handle_ShouldFilterRelationshipsByType()
    {
        // 🎯 Mục tiêu của test: Xác minh handler lọc các mối quan hệ theo Type.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Thêm các mối quan hệ với Type khác nhau vào _context.

        // 2. Act: Gọi phương thức Handle với một SearchRelationshipsQuery có Type được chỉ định.

        // 3. Assert: Kiểm tra kết quả trả về là thành công và chỉ chứa các mối quan hệ có Type đó.

        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Code = "FAM001", Name = "Test Family" };
        _context.Families.Add(family);

        var sourceMember1 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "SM001", FirstName = "Source1", LastName = "Member1" };
        var targetMember1 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "TM001", FirstName = "Target1", LastName = "Member1" };
        var sourceMember2 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "SM002", FirstName = "Source2", LastName = "Member2" };
        var targetMember2 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "TM002", FirstName = "Target2", LastName = "Member2" };
        _context.Members.AddRange(sourceMember1, targetMember1, sourceMember2, targetMember2);

        var relationship1 = new Relationship { Id = Guid.NewGuid(), SourceMemberId = sourceMember1.Id, TargetMemberId = targetMember1.Id, Type = RelationshipType.Father, FamilyId = familyId };
        var relationship2 = new Relationship { Id = Guid.NewGuid(), SourceMemberId = sourceMember2.Id, TargetMemberId = targetMember2.Id, Type = RelationshipType.Mother, FamilyId = familyId };
        _context.Relationships.AddRange(relationship1, relationship2);
        await _context.SaveChangesAsync();

        var expectedDto1 = new RelationshipListDto { Id = relationship1.Id, SourceMemberId = relationship1.SourceMemberId, TargetMemberId = relationship1.TargetMemberId, Type = relationship1.Type, SourceMember = new RelationshipMemberDto { Id = sourceMember1.Id, FullName = sourceMember1.FirstName + " " + sourceMember1.LastName }, TargetMember = new RelationshipMemberDto { Id = targetMember1.Id, FullName = targetMember1.FirstName + " " + targetMember1.LastName } };

        var query = new SearchRelationshipsQuery { Type = RelationshipType.Father.ToString() };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.Should().ContainEquivalentOf(expectedDto1);
        // 💡 Giải thích: Handler chỉ nên trả về các mối quan hệ có Type được chỉ định.
    }

    [Fact]
    public async Task Handle_ShouldSortRelationshipsBySourceMemberFullNameAscending()
    {
        // 🎯 Mục tiêu của test: Xác minh handler sắp xếp các mối quan hệ theo tên đầy đủ của thành viên nguồn tăng dần.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Thêm các mối quan hệ với các thành viên nguồn có tên khác nhau vào _context.

        // 2. Act: Gọi phương thức Handle với một SearchRelationshipsQuery có SortBy là "SourceMemberFullName" và SortOrder là "asc".

        // 3. Assert: Kiểm tra kết quả trả về là thành công và các mối quan hệ được sắp xếp đúng thứ tự.

        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Code = "FAM001", Name = "Test Family" };
        _context.Families.Add(family);

        var sourceMemberA = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "SMA", FirstName = "Alice", LastName = "Smith" };
        var sourceMemberB = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "SMB", FirstName = "Bob", LastName = "Johnson" };
        var targetMember = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "TM", FirstName = "Target", LastName = "Member" };
        _context.Members.AddRange(sourceMemberA, sourceMemberB, targetMember);

        var relationship1 = new Relationship { Id = Guid.NewGuid(), SourceMemberId = sourceMemberB.Id, TargetMemberId = targetMember.Id, Type = RelationshipType.Father, FamilyId = familyId };
        var relationship2 = new Relationship { Id = Guid.NewGuid(), SourceMemberId = sourceMemberA.Id, TargetMemberId = targetMember.Id, Type = RelationshipType.Mother, FamilyId = familyId };
        _context.Relationships.AddRange(relationship1, relationship2);
        await _context.SaveChangesAsync();

        var expectedDto1 = new RelationshipListDto { Id = relationship1.Id, SourceMemberId = relationship1.SourceMemberId, TargetMemberId = relationship1.TargetMemberId, Type = relationship1.Type, SourceMember = new RelationshipMemberDto { Id = sourceMemberB.Id, FullName = sourceMemberB.FirstName + " " + sourceMemberB.LastName }, TargetMember = new RelationshipMemberDto { Id = targetMember.Id, FullName = targetMember.FirstName + " " + targetMember.LastName } };
        var expectedDto2 = new RelationshipListDto { Id = relationship2.Id, SourceMemberId = relationship2.SourceMemberId, TargetMemberId = relationship2.TargetMemberId, Type = relationship2.Type, SourceMember = new RelationshipMemberDto { Id = sourceMemberA.Id, FullName = sourceMemberA.FirstName + " " + sourceMemberA.LastName }, TargetMember = new RelationshipMemberDto { Id = targetMember.Id, FullName = targetMember.FirstName + " " + targetMember.LastName } };

        var query = new SearchRelationshipsQuery { SortBy = "SourceMemberFullName", SortOrder = "asc" };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.Items.First().Should().BeEquivalentTo(expectedDto1, options => options.Excluding(x => x.Id));
        result.Value.Items.Last().Should().BeEquivalentTo(expectedDto2, options => options.Excluding(x => x.Id));
    }

    [Fact]
    public async Task Handle_ShouldSortRelationshipsBySourceMemberFullNameDescending()
    {
        // 🎯 Mục tiêu của test: Xác minh handler sắp xếp các mối quan hệ theo tên đầy đủ của thành viên nguồn giảm dần.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Thêm các mối quan hệ với các thành viên nguồn có tên khác nhau vào _context.

        // 2. Act: Gọi phương thức Handle với một SearchRelationshipsQuery có SortBy là "SourceMemberFullName" và SortOrder là "desc".

        // 3. Assert: Kiểm tra kết quả trả về là thành công và các mối quan hệ được sắp xếp đúng thứ tự.

        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Code = "FAM001", Name = "Test Family" };
        _context.Families.Add(family);

        var sourceMemberA = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "SMA", FirstName = "Alice", LastName = "Smith" };
        var sourceMemberB = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "SMB", FirstName = "Bob", LastName = "Johnson" };
        var targetMember = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "TM", FirstName = "Target", LastName = "Member" };
        _context.Members.AddRange(sourceMemberA, sourceMemberB, targetMember);

        var relationship1 = new Relationship { Id = Guid.NewGuid(), SourceMemberId = sourceMemberB.Id, TargetMemberId = targetMember.Id, Type = RelationshipType.Father, FamilyId = familyId };
        var relationship2 = new Relationship { Id = Guid.NewGuid(), SourceMemberId = sourceMemberA.Id, TargetMemberId = targetMember.Id, Type = RelationshipType.Mother, FamilyId = familyId };
        _context.Relationships.AddRange(relationship1, relationship2);
        await _context.SaveChangesAsync();

        var expectedDto1 = new RelationshipListDto { Id = relationship2.Id, SourceMemberId = relationship2.SourceMemberId, TargetMemberId = relationship2.TargetMemberId, Type = relationship2.Type, SourceMember = new RelationshipMemberDto { Id = sourceMemberA.Id, FullName = sourceMemberA.FirstName + " " + sourceMemberA.LastName }, TargetMember = new RelationshipMemberDto { Id = targetMember.Id, FullName = targetMember.FirstName + " " + targetMember.LastName } };
        var expectedDto2 = new RelationshipListDto { Id = relationship1.Id, SourceMemberId = relationship1.SourceMemberId, TargetMemberId = relationship1.TargetMemberId, Type = relationship1.Type, SourceMember = new RelationshipMemberDto { Id = sourceMemberB.Id, FullName = sourceMemberB.FirstName + " " + sourceMemberB.LastName }, TargetMember = new RelationshipMemberDto { Id = targetMember.Id, FullName = targetMember.FirstName + " " + targetMember.LastName } };

        var query = new SearchRelationshipsQuery { SortBy = "SourceMemberFullName", SortOrder = "desc" };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.Items.First().Should().BeEquivalentTo(expectedDto1, options => options.Excluding(x => x.Id));
        result.Value.Items.Last().Should().BeEquivalentTo(expectedDto2, options => options.Excluding(x => x.Id));
        // 💡 Giải thích: Các mối quan hệ phải được sắp xếp theo tên đầy đủ của thành viên nguồn giảm dần.
    }
}
