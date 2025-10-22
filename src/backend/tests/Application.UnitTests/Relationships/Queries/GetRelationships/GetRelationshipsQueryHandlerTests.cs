using AutoFixture.AutoMoq;
using backend.Application.Common.Mappings;
using backend.Application.Relationships.Queries;
using backend.Application.Relationships.Queries.GetRelationships;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Queries.GetRelationships;

public class GetRelationshipsQueryHandlerTests : TestBase
{
    private readonly GetRelationshipsQueryHandler _handler;

    public GetRelationshipsQueryHandlerTests()
    {
        _fixture.Customize(new AutoMoqCustomization());
        _handler = new GetRelationshipsQueryHandler(
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
        // 2. Act: Gọi phương thức Handle với một GetRelationshipsQuery không có bộ lọc.
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

        var expectedDto1 = new RelationshipListDto { Id = relationship1.Id, SourceMemberId = relationship1.SourceMemberId, TargetMemberId = relationship1.TargetMemberId, Type = relationship1.Type, SourceMember = new RelationshipMemberDto { Id = sourceMember1.Id, FullName = sourceMember1.FullName }, TargetMember = new RelationshipMemberDto { Id = targetMember1.Id, FullName = targetMember1.FullName } };
        var expectedDto2 = new RelationshipListDto { Id = relationship2.Id, SourceMemberId = relationship2.SourceMemberId, TargetMemberId = relationship2.TargetMemberId, Type = relationship2.Type, SourceMember = new RelationshipMemberDto { Id = sourceMember2.Id, FullName = sourceMember2.FullName }, TargetMember = new RelationshipMemberDto { Id = targetMember2.Id, FullName = targetMember2.FullName } };

        var query = new GetRelationshipsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.Items.Should().ContainEquivalentOf(expectedDto1);
        result.Value.Items.Should().ContainEquivalentOf(expectedDto2);
        // 💡 Giải thích: Handler phải trả về tất cả các mối quan hệ khi không có tiêu chí lọc.
    }

    [Fact]
    public async Task Handle_ShouldFilterRelationshipsByFamilyId()
    {
        // 🎯 Mục tiêu của test: Xác minh handler lọc các mối quan hệ theo FamilyId.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thêm các mối quan hệ với FamilyId khác nhau vào _context.
        // 2. Act: Gọi phương thức Handle với một GetRelationshipsQuery có FamilyId được chỉ định.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chỉ chứa các mối quan hệ thuộc FamilyId đó.

        var familyId1 = Guid.NewGuid();
        var familyId2 = Guid.NewGuid();

        var family1 = new Family { Id = familyId1, Code = "FAM001", Name = "Test Family 1" };
        var family2 = new Family { Id = familyId2, Code = "FAM002", Name = "Test Family 2" };
        _context.Families.AddRange(family1, family2);

        var sourceMember1 = new Member { Id = Guid.NewGuid(), FamilyId = familyId1, Code = "SM001", FirstName = "Source1", LastName = "Member1" };
        var targetMember1 = new Member { Id = Guid.NewGuid(), FamilyId = familyId1, Code = "TM001", FirstName = "Target1", LastName = "Member1" };
        var sourceMember2 = new Member { Id = Guid.NewGuid(), FamilyId = familyId2, Code = "SM002", FirstName = "Source2", LastName = "Member2" };
        var targetMember2 = new Member { Id = Guid.NewGuid(), FamilyId = familyId2, Code = "TM002", FirstName = "Target2", LastName = "Member2" };
        _context.Members.AddRange(sourceMember1, targetMember1, sourceMember2, targetMember2);

        var relationship1 = new Relationship { Id = Guid.NewGuid(), SourceMemberId = sourceMember1.Id, TargetMemberId = targetMember1.Id, Type = RelationshipType.Father, FamilyId = familyId1 };
        var relationship2 = new Relationship { Id = Guid.NewGuid(), SourceMemberId = sourceMember2.Id, TargetMemberId = targetMember2.Id, Type = RelationshipType.Mother, FamilyId = familyId2 };
        _context.Relationships.AddRange(relationship1, relationship2);
        await _context.SaveChangesAsync();

        var expectedDto1 = new RelationshipListDto { Id = relationship1.Id, SourceMemberId = relationship1.SourceMemberId, TargetMemberId = relationship1.TargetMemberId, Type = relationship1.Type, SourceMember = new RelationshipMemberDto { Id = sourceMember1.Id, FullName = sourceMember1.FullName }, TargetMember = new RelationshipMemberDto { Id = targetMember1.Id, FullName = targetMember1.FullName } };



        var query = new GetRelationshipsQuery { FamilyId = familyId1 };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.Should().ContainEquivalentOf(expectedDto1);
        // 💡 Giải thích: Handler chỉ nên trả về các mối quan hệ thuộc FamilyId được chỉ định.

    }

    [Fact]
    public async Task Handle_ShouldFilterRelationshipsBySourceMemberId()
    {
        // 🎯 Mục tiêu của test: Xác minh handler lọc các mối quan hệ theo SourceMemberId.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thêm các mối quan hệ với SourceMemberId khác nhau vào _context.
        // 2. Act: Gọi phương thức Handle với một GetRelationshipsQuery có SourceMemberId được chỉ định.
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

        var expectedDto1 = new RelationshipListDto { Id = relationship1.Id, SourceMemberId = relationship1.SourceMemberId, TargetMemberId = relationship1.TargetMemberId, Type = relationship1.Type, SourceMember = new RelationshipMemberDto { Id = sourceMember1.Id, FullName = sourceMember1.FullName }, TargetMember = new RelationshipMemberDto { Id = targetMember1.Id, FullName = targetMember1.FullName } };

        var query = new GetRelationshipsQuery { SourceMemberId = sourceMember1.Id };

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
        // 1. Arrange: Thêm các mối quan hệ với TargetMemberId khác nhau vào _context.
        // 2. Act: Gọi phương thức Handle với một GetRelationshipsQuery có TargetMemberId được chỉ định.
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

        var expectedDto1 = new RelationshipListDto { Id = relationship1.Id, SourceMemberId = relationship1.SourceMemberId, TargetMemberId = relationship1.TargetMemberId, Type = relationship1.Type, SourceMember = new RelationshipMemberDto { Id = sourceMember1.Id, FullName = sourceMember1.FullName }, TargetMember = new RelationshipMemberDto { Id = targetMember1.Id, FullName = targetMember1.FullName } };

        var query = new GetRelationshipsQuery { TargetMemberId = targetMember1.Id };

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
        // 2. Act: Gọi phương thức Handle với một GetRelationshipsQuery có Type được chỉ định.
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

        var expectedDto1 = new RelationshipListDto { Id = relationship1.Id, SourceMemberId = relationship1.SourceMemberId, TargetMemberId = relationship1.TargetMemberId, Type = relationship1.Type, SourceMember = new RelationshipMemberDto { Id = sourceMember1.Id, FullName = sourceMember1.FullName }, TargetMember = new RelationshipMemberDto { Id = targetMember1.Id, FullName = targetMember1.FullName } };

        var query = new GetRelationshipsQuery { Type = RelationshipType.Father.ToString() };

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
        // 2. Act: Gọi phương thức Handle với một GetRelationshipsQuery có SortBy là "SourceMemberFullName" và SortOrder là "asc".
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

        var expectedDto1 = new RelationshipListDto { Id = relationship1.Id, SourceMemberId = relationship1.SourceMemberId, TargetMemberId = relationship1.TargetMemberId, Type = relationship1.Type, SourceMember = new RelationshipMemberDto { Id = sourceMemberB.Id, FullName = sourceMemberB.LastName + " " + sourceMemberB.FirstName }, TargetMember = new RelationshipMemberDto { Id = targetMember.Id, FullName = targetMember.LastName + " " + targetMember.FirstName } };
        var expectedDto2 = new RelationshipListDto { Id = relationship2.Id, SourceMemberId = relationship2.SourceMemberId, TargetMemberId = relationship2.TargetMemberId, Type = relationship2.Type, SourceMember = new RelationshipMemberDto { Id = sourceMemberA.Id, FullName = sourceMemberA.LastName + " " + sourceMemberA.FirstName }, TargetMember = new RelationshipMemberDto { Id = targetMember.Id, FullName = targetMember.LastName + " " + targetMember.FirstName } };

        var query = new GetRelationshipsQuery { SortBy = "SourceMemberFullName", SortOrder = "asc" };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.Items.First().Should().BeEquivalentTo(expectedDto1, options => options.Excluding(x => x.Id));
        result.Value.Items.Last().Should().BeEquivalentTo(expectedDto2, options => options.Excluding(x => x.Id));
        // 💡 Giải thích: Các mối quan hệ phải được sắp xếp theo tên đầy đủ của thành viên nguồn tăng dần.
    }

    [Fact]
    public async Task Handle_ShouldSortRelationshipsBySourceMemberFullNameDescending()
    {
        // 🎯 Mục tiêu của test: Xác minh handler sắp xếp các mối quan hệ theo tên đầy đủ của thành viên nguồn giảm dần.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thêm các mối quan hệ với các thành viên nguồn có tên khác nhau vào _context.
        // 2. Act: Gọi phương thức Handle với một GetRelationshipsQuery có SortBy là "SourceMemberFullName" và SortOrder là "desc".
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

        var expectedDto1 = new RelationshipListDto { Id = relationship2.Id, SourceMemberId = relationship2.SourceMemberId, TargetMemberId = relationship2.TargetMemberId, Type = relationship2.Type, SourceMember = new RelationshipMemberDto { Id = sourceMemberA.Id, FullName = sourceMemberA.LastName + " " + sourceMemberA.FirstName }, TargetMember = new RelationshipMemberDto { Id = targetMember.Id, FullName = targetMember.LastName + " " + targetMember.FirstName } };
        var expectedDto2 = new RelationshipListDto { Id = relationship1.Id, SourceMemberId = relationship1.SourceMemberId, TargetMemberId = relationship1.TargetMemberId, Type = relationship1.Type, SourceMember = new RelationshipMemberDto { Id = sourceMemberB.Id, FullName = sourceMemberB.LastName + " " + sourceMemberB.FirstName }, TargetMember = new RelationshipMemberDto { Id = targetMember.Id, FullName = targetMember.LastName + " " + targetMember.FirstName } };

        var query = new GetRelationshipsQuery { SortBy = "SourceMemberFullName", SortOrder = "desc" };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.Items.First().Should().BeEquivalentTo(expectedDto1, options => options.Excluding(x => x.Id));
        result.Value.Items.Last().Should().BeEquivalentTo(expectedDto2, options => options.Excluding(x => x.Id));
    }

    [Fact]
    public async Task Handle_ShouldSortRelationshipsByTargetMemberFullNameAscending()
    {
        // 🎯 Mục tiêu của test: Xác minh handler sắp xếp các mối quan hệ theo tên đầy đủ của thành viên đích tăng dần.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thêm các mối quan hệ với các thành viên đích có tên khác nhau vào _context.
        // 2. Act: Gọi phương thức Handle với một GetRelationshipsQuery có SortBy là "TargetMemberFullName" và SortOrder là "asc".
        // 3. Assert: Kiểm tra kết quả trả về là thành công và các mối quan hệ được sắp xếp đúng thứ tự.

        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Code = "FAM001", Name = "Test Family" };
        _context.Families.Add(family);

        var sourceMember = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "SM", FirstName = "Source", LastName = "Member" };
        var targetMemberA = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "TMA", FirstName = "Alice", LastName = "Smith" };
        var targetMemberB = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "TMB", FirstName = "Bob", LastName = "Johnson" };
        _context.Members.AddRange(sourceMember, targetMemberA, targetMemberB);

        var relationship1 = new Relationship { Id = Guid.NewGuid(), SourceMemberId = sourceMember.Id, TargetMemberId = targetMemberB.Id, Type = RelationshipType.Father, FamilyId = familyId };
        var relationship2 = new Relationship { Id = Guid.NewGuid(), SourceMemberId = sourceMember.Id, TargetMemberId = targetMemberA.Id, Type = RelationshipType.Mother, FamilyId = familyId };
        _context.Relationships.AddRange(relationship1, relationship2);
        await _context.SaveChangesAsync();

        var expectedDto1 = new RelationshipListDto { Id = relationship1.Id, SourceMemberId = relationship1.SourceMemberId, TargetMemberId = relationship1.TargetMemberId, Type = relationship1.Type, SourceMember = new RelationshipMemberDto { Id = sourceMember.Id, FullName = sourceMember.LastName + " " + sourceMember.FirstName }, TargetMember = new RelationshipMemberDto { Id = targetMemberB.Id, FullName = targetMemberB.LastName + " " + targetMemberB.FirstName } };
        var expectedDto2 = new RelationshipListDto { Id = relationship2.Id, SourceMemberId = relationship2.SourceMemberId, TargetMemberId = relationship2.TargetMemberId, Type = relationship2.Type, SourceMember = new RelationshipMemberDto { Id = sourceMember.Id, FullName = sourceMember.LastName + " " + sourceMember.FirstName }, TargetMember = new RelationshipMemberDto { Id = targetMemberA.Id, FullName = targetMemberA.LastName + " " + targetMemberA.FirstName } };

        var query = new GetRelationshipsQuery { SortBy = "TargetMemberFullName", SortOrder = "asc" };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.Items.First().Should().BeEquivalentTo(expectedDto1, options => options.Excluding(x => x.Id));
        result.Value.Items.Last().Should().BeEquivalentTo(expectedDto2, options => options.Excluding(x => x.Id));
        // 💡 Giải thích: Các mối quan hệ phải được sắp xếp theo tên đầy đủ của thành viên đích tăng dần.
    }

    [Fact]
    public async Task Handle_ShouldSortRelationshipsByTargetMemberFullNameDescending()
    {
        // 🎯 Mục tiêu của test: Xác minh handler sắp xếp các mối quan hệ theo tên đầy đủ của thành viên đích giảm dần.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thêm các mối quan hệ với các thành viên đích có tên khác nhau vào _context.
        // 2. Act: Gọi phương thức Handle với một GetRelationshipsQuery có SortBy là "TargetMemberFullName" và SortOrder là "desc".
        // 3. Assert: Kiểm tra kết quả trả về là thành công và các mối quan hệ được sắp xếp đúng thứ tự.

        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Code = "FAM001", Name = "Test Family" };
        _context.Families.Add(family);

        var sourceMember = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "SM", FirstName = "Source", LastName = "Member" };
        var targetMemberA = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "TMA", FirstName = "Alice", LastName = "Smith" };
        var targetMemberB = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "TMB", FirstName = "Bob", LastName = "Johnson" };
        _context.Members.AddRange(sourceMember, targetMemberA, targetMemberB);

        var relationship1 = new Relationship { Id = Guid.NewGuid(), SourceMemberId = sourceMember.Id, TargetMemberId = targetMemberB.Id, Type = RelationshipType.Father, FamilyId = familyId };
        var relationship2 = new Relationship { Id = Guid.NewGuid(), SourceMemberId = sourceMember.Id, TargetMemberId = targetMemberA.Id, Type = RelationshipType.Mother, FamilyId = familyId };
        _context.Relationships.AddRange(relationship1, relationship2);
        await _context.SaveChangesAsync();

        var expectedDto1 = new RelationshipListDto { Id = relationship2.Id, SourceMemberId = relationship2.SourceMemberId, TargetMemberId = relationship2.TargetMemberId, Type = relationship2.Type, SourceMember = new RelationshipMemberDto { Id = sourceMember.Id, FullName = sourceMember.LastName + " " + sourceMember.FirstName }, TargetMember = new RelationshipMemberDto { Id = targetMemberA.Id, FullName = targetMemberA.LastName + " " + targetMemberA.FirstName } };
        var expectedDto2 = new RelationshipListDto { Id = relationship1.Id, SourceMemberId = relationship1.SourceMemberId, TargetMemberId = relationship1.TargetMemberId, Type = relationship1.Type, SourceMember = new RelationshipMemberDto { Id = sourceMember.Id, FullName = sourceMember.LastName + " " + sourceMember.FirstName }, TargetMember = new RelationshipMemberDto { Id = targetMemberB.Id, FullName = targetMemberB.LastName + " " + targetMemberB.FirstName } };

        var query = new GetRelationshipsQuery { SortBy = "TargetMemberFullName", SortOrder = "desc" };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.Items.First().Should().BeEquivalentTo(expectedDto1, options => options.Excluding(x => x.Id));
        result.Value.Items.Last().Should().BeEquivalentTo(expectedDto2, options => options.Excluding(x => x.Id));
        // 💡 Giải thích: Các mối quan hệ phải được sắp xếp theo tên đầy đủ của thành viên đích giảm dần.
    }

    [Fact]
    public async Task Handle_ShouldSortRelationshipsByTypeAscending()
    {
        // 🎯 Mục tiêu của test: Xác minh handler sắp xếp các mối quan hệ theo Type tăng dần.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thêm các mối quan hệ với các Type khác nhau vào _context.
        // 2. Act: Gọi phương thức Handle với một GetRelationshipsQuery có SortBy là "Type" và SortOrder là "asc".
        // 3. Assert: Kiểm tra kết quả trả về là thành công và các mối quan hệ được sắp xếp đúng thứ tự.

        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Code = "FAM001", Name = "Test Family" };
        _context.Families.Add(family);

        var sourceMember = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "SM", FirstName = "Source", LastName = "Member" };
        var targetMember1 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "TM1", FirstName = "Target1", LastName = "Member" };
        var targetMember2 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "TM2", FirstName = "Target2", LastName = "Member" };
        _context.Members.AddRange(sourceMember, targetMember1, targetMember2);

        var relationship1 = new Relationship { Id = Guid.NewGuid(), SourceMemberId = sourceMember.Id, TargetMemberId = targetMember1.Id, Type = RelationshipType.Mother, FamilyId = familyId };
        var relationship2 = new Relationship { Id = Guid.NewGuid(), SourceMemberId = sourceMember.Id, TargetMemberId = targetMember2.Id, Type = RelationshipType.Father, FamilyId = familyId };
        _context.Relationships.AddRange(relationship1, relationship2);
        await _context.SaveChangesAsync();

        var expectedDto1 = new RelationshipListDto { Id = relationship2.Id, SourceMemberId = relationship2.SourceMemberId, TargetMemberId = relationship2.TargetMemberId, Type = relationship2.Type, SourceMember = new RelationshipMemberDto { Id = sourceMember.Id, FullName = sourceMember.LastName + " " + sourceMember.FirstName }, TargetMember = new RelationshipMemberDto { Id = targetMember2.Id, FullName = targetMember2.LastName + " " + targetMember2.FirstName } };
        var expectedDto2 = new RelationshipListDto { Id = relationship1.Id, SourceMemberId = relationship1.SourceMemberId, TargetMemberId = relationship1.TargetMemberId, Type = relationship1.Type, SourceMember = new RelationshipMemberDto { Id = sourceMember.Id, FullName = sourceMember.LastName + " " + sourceMember.FirstName }, TargetMember = new RelationshipMemberDto { Id = targetMember1.Id, FullName = targetMember1.LastName + " " + targetMember1.FirstName } };

        var query = new GetRelationshipsQuery { SortBy = "Type", SortOrder = "asc" };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.Items.First().Should().BeEquivalentTo(expectedDto1, options => options.Excluding(x => x.Id));
        result.Value.Items.Last().Should().BeEquivalentTo(expectedDto2, options => options.Excluding(x => x.Id));
        // 💡 Giải thích: Các mối quan hệ phải được sắp xếp theo Type tăng dần.
    }

    [Fact]
    public async Task Handle_ShouldSortRelationshipsByTypeDescending()
    {
        // 🎯 Mục tiêu của test: Xác minh handler sắp xếp các mối quan hệ theo Type giảm dần.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thêm các mối quan hệ với các Type khác nhau vào _context.
        // 2. Act: Gọi phương thức Handle với một GetRelationshipsQuery có SortBy là "Type" và SortOrder là "desc".
        // 3. Assert: Kiểm tra kết quả trả về là thành công và các mối quan hệ được sắp xếp đúng thứ tự.

        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Code = "FAM001", Name = "Test Family" };
        _context.Families.Add(family);

        var sourceMember = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "SM", FirstName = "Source", LastName = "Member" };
        var targetMember1 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "TM1", FirstName = "Target1", LastName = "Member" };
        var targetMember2 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, Code = "TM2", FirstName = "Target2", LastName = "Member" };
        _context.Members.AddRange(sourceMember, targetMember1, targetMember2);

        var relationship1 = new Relationship { Id = Guid.NewGuid(), SourceMemberId = sourceMember.Id, TargetMemberId = targetMember1.Id, Type = RelationshipType.Mother, FamilyId = familyId };
        var relationship2 = new Relationship { Id = Guid.NewGuid(), SourceMemberId = sourceMember.Id, TargetMemberId = targetMember2.Id, Type = RelationshipType.Father, FamilyId = familyId };
        _context.Relationships.AddRange(relationship1, relationship2);
        await _context.SaveChangesAsync();

        var expectedDto1 = new RelationshipListDto { Id = relationship1.Id, SourceMemberId = relationship1.SourceMemberId, TargetMemberId = relationship1.TargetMemberId, Type = relationship1.Type, SourceMember = new RelationshipMemberDto { Id = sourceMember.Id, FullName = sourceMember.LastName + " " + sourceMember.FirstName }, TargetMember = new RelationshipMemberDto { Id = targetMember1.Id, FullName = targetMember1.LastName + " " + targetMember1.FirstName } };
        var expectedDto2 = new RelationshipListDto { Id = relationship2.Id, SourceMemberId = relationship2.SourceMemberId, TargetMemberId = relationship2.TargetMemberId, Type = relationship2.Type, SourceMember = new RelationshipMemberDto { Id = sourceMember.Id, FullName = sourceMember.LastName + " " + sourceMember.FirstName }, TargetMember = new RelationshipMemberDto { Id = targetMember2.Id, FullName = targetMember2.LastName + " " + targetMember2.FirstName } };

        var query = new GetRelationshipsQuery { SortBy = "Type", SortOrder = "desc" };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.Items.First().Should().BeEquivalentTo(expectedDto1, options => options.Excluding(x => x.Id));
        result.Value.Items.Last().Should().BeEquivalentTo(expectedDto2, options => options.Excluding(x => x.Id));
        // 💡 Giải thích: Các mối quan hệ phải được sắp xếp theo Type giảm dần.
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedRelationships()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về các mối quan hệ được phân trang chính xác.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thêm nhiều mối quan hệ vào _context.
        // 2. Act: Gọi phương thức Handle với một GetRelationshipsQuery có PageNumber và PageSize được chỉ định.

        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Code = "FAM001", Name = "Test Family" };
        _context.Families.Add(family);

        var members = new List<Member>();
        for (int i = 0; i < 10; i++)
        {
            members.Add(new Member
            {
                Id = Guid.NewGuid(),
                FamilyId = familyId,
                Code = $"MEM{i:D3}",
                FirstName = $"FirstName{i}",
                LastName = $"LastName{i}"
            });
        }
        _context.Members.AddRange(members);

        var relationships = new List<Relationship>();
        for (int i = 0; i < 10; i++)
        {
            relationships.Add(new Relationship
            {
                Id = Guid.NewGuid(),
                SourceMemberId = members[i].Id,
                TargetMemberId = members[(i + 1) % 10].Id,
                Type = RelationshipType.Father,
                FamilyId = familyId
            });
        }
        _context.Relationships.AddRange(relationships);
        await _context.SaveChangesAsync();

        var query = new GetRelationshipsQuery { FamilyId = familyId, Page = 2, ItemsPerPage = 3 };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(3);
        result.Value.TotalItems.Should().Be(10);
        result.Value.Page.Should().Be(2);
        result.Value.TotalPages.Should().Be(4);

        var expectedRelationships = relationships.OrderBy(r => r.Id).Skip((query.Page - 1) * query.ItemsPerPage).Take(query.ItemsPerPage).ToList();
        var expectedDtos = _mapper.Map<List<RelationshipListDto>>(expectedRelationships);

        result.Value.Items.Should().BeEquivalentTo(expectedDtos, options => options.Excluding(x => x.Id));
        // 💡 Giải thích: Handler phải trả về chính xác các mối quan hệ trên trang được yêu cầu và thông tin phân trang phải đúng.
    }

    [Fact]
    public async Task PaginatedListAsync_ShouldReturnCorrectPageOfItems()
    {
        // 🎯 Mục tiêu của test: Xác minh phương thức mở rộng PaginatedListAsync trả về đúng trang của các mục.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thêm nhiều đối tượng Relationship vào _context.
        // 2. Act: Gọi PaginatedListAsync trên IQueryable<Relationship> với Page và ItemsPerPage được chỉ định.
        // 3. Assert: Kiểm tra kết quả trả về là đúng số lượng mục, tổng số mục và thông tin phân trang.

        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Code = "FAM001", Name = "Test Family" };
        _context.Families.Add(family);

        var members = new List<Member>();
        for (int i = 0; i < 10; i++)
        {
            members.Add(new Member
            {
                Id = Guid.NewGuid(),
                FamilyId = familyId,
                Code = $"MEM{i:D3}",
                FirstName = $"FirstName{i}",
                LastName = $"LastName{i}"
            });
        }
        _context.Members.AddRange(members);

        var relationships = new List<Relationship>();
        for (int i = 0; i < 10; i++)
        {
            relationships.Add(new Relationship
            {
                Id = Guid.NewGuid(),
                SourceMemberId = members[i].Id,
                TargetMemberId = members[(i + 1) % 10].Id,
                Type = RelationshipType.Father,
                FamilyId = familyId
            });
        }
        _context.Relationships.AddRange(relationships);
        await _context.SaveChangesAsync();

        var queryableRelationships = _context.Relationships.OrderBy(r => r.Id);

        var page = 2;
        var itemsPerPage = 3;

        var paginatedList = await queryableRelationships.PaginatedListAsync(page, itemsPerPage);

        paginatedList.Should().NotBeNull();
        paginatedList.Items.Should().HaveCount(3);
        paginatedList.TotalItems.Should().Be(10);
        paginatedList.Page.Should().Be(2);
        paginatedList.TotalPages.Should().Be(4);
        // 💡 Giải thích: Phương thức PaginatedListAsync phải trả về chính xác các mục trên trang được yêu cầu và thông tin phân trang phải đúng.
    }
}
