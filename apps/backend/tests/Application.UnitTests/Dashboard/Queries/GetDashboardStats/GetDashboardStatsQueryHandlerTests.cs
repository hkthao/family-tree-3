using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Dashboard.Queries.GetDashboardStats;
using backend.Application.Families.Queries;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Dashboard.Queries.GetDashboardStats;

public class GetDashboardStatsQueryHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IMediator> _mockMediator; // NEW
    private readonly GetDashboardStatsQueryHandler _handler;

    public GetDashboardStatsQueryHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _currentUserMock = new Mock<ICurrentUser>();
        _mockMediator = new Mock<IMediator>(); // NEW
        // Setup default mock for IMediator.Send to return a successful FamilyLimitConfigurationDto
        _mockMediator.Setup(m => m.Send(It.IsAny<GetFamilyLimitConfigurationQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FamilyLimitConfigurationDto>.Success(new FamilyLimitConfigurationDto
            {
                Id = Guid.NewGuid(),
                FamilyId = It.IsAny<Guid>(), // This will be overridden by the actual FamilyId in the query
                MaxMembers = 50,
                MaxStorageMb = 1024
            }));
        _handler = new GetDashboardStatsQueryHandler(_context, _authorizationServiceMock.Object, _currentUserMock.Object, _mockDateTime.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnDashboardStats_ForAdminUser()
    {
        // 🎯 Mục tiêu của test: Đảm bảo handler trả về thống kê chính xác cho người dùng quản trị (admin).

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Thiết lập IAuthorizationService để xác định người dùng là admin.
        _authorizationServiceMock.Setup(s => s.IsAdmin()).Returns(true);

        // 2. Thêm dữ liệu mẫu vào cơ sở dữ liệu: Families, Members, Relationships.
        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family 1", Code = "FAM001", IsDeleted = false };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family 2", Code = "FAM002", IsDeleted = false };
        var family3 = new Family { Id = Guid.NewGuid(), Name = "Family 3", Code = "FAM003", IsDeleted = false };

        _context.Families.Add(family1);
        _context.Families.Add(family2);
        _context.Families.Add(family3);

        var member1_f1 = new Member("Last1", "First1", "MEM001", family1.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        var member2_f1 = new Member("Last2", "First2", "MEM002", family1.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        var member3_f2 = new Member("Last3", "First3", "MEM003", family2.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        var member4_f3 = new Member("Last4", "First4", "MEM004", family3.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };

        _context.Members.Add(member1_f1);
        _context.Members.Add(member2_f1);
        _context.Members.Add(member3_f2);
        _context.Members.Add(member4_f3);

        var relMember1_f1 = new Member("RelLast1", "RelFirst1", "REL001", family1.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        var relMember2_f1 = new Member("RelLast2", "RelFirst2", "REL002", family1.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        _context.Members.Add(relMember1_f1);
        _context.Members.Add(relMember2_f1);

        _context.Relationships.Add(new Relationship(family1.Id, relMember1_f1.Id, relMember2_f1.Id, RelationshipType.Father, null));

        await _context.SaveChangesAsync();

        _context.Families.Count().Should().Be(3);

        var query = new GetDashboardStatsQuery { FamilyId = family1.Id };

        // Act: Gọi handler để xử lý query.
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert: Kiểm tra kết quả trả về là thành công và chứa các thống kê chính xác.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalFamilies.Should().Be(1); // Should be 1 because we are querying for a specific family.
        result.Value!.TotalMembers.Should().Be(4); // 2 initial + 2 for relationship, all from family1
        result.Value!.TotalRelationships.Should().Be(1);


        // 💡 Giải thích: Khi người dùng là admin, handler sẽ tính toán thống kê dựa trên tất cả các gia đình, thành viên và mối quan hệ trong hệ thống.
    }

    [Fact]
    public async Task Handle_ShouldReturnDashboardStats_ForNonAdminUserWithAccessibleFamilies()
    {
        // 🎯 Mục tiêu của test: Đảm bảo handler trả về thống kê chính xác cho người dùng không phải admin, chỉ dựa trên các gia đình mà họ có quyền truy cập.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Thiết lập IAuthorizationService để xác định người dùng không phải admin.
        _authorizationServiceMock.Setup(s => s.IsAdmin()).Returns(false);
        Guid userId = Guid.NewGuid();

        // 2. Thiết lập ICurrentUser với một UserId cụ thể.
        _currentUserMock.Setup(u => u.UserId).Returns(userId);

        // 3. Thêm dữ liệu mẫu: Families, Members, Relationships và FamilyUsers để xác định quyền truy cập.
        var accessibleFamily1 = new Family { Id = Guid.NewGuid(), Name = "Accessible Family 1", Code = "ACC001", IsDeleted = false };
        var accessibleFamily2 = new Family { Id = Guid.NewGuid(), Name = "Accessible Family 2", Code = "ACC002", IsDeleted = false };
        var inaccessibleFamily = new Family { Id = Guid.NewGuid(), Name = "Inaccessible Family", Code = "INACC001", IsDeleted = false };

        _context.Families.Add(accessibleFamily1);
        _context.Families.Add(accessibleFamily2);
        _context.Families.Add(inaccessibleFamily);

        _context.FamilyUsers.Add(new FamilyUser(accessibleFamily1.Id, userId, FamilyRole.Admin));
        _context.FamilyUsers.Add(new FamilyUser(accessibleFamily2.Id, userId, FamilyRole.Admin));

        var accMember1_f1 = new Member("AccLast1", "AccFirst1", "ACC_MEM001", accessibleFamily1.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        var accMember2_f1 = new Member("AccLast2", "AccFirst2", "ACC_MEM002", accessibleFamily1.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        var accMember3_f2 = new Member("AccLast3", "AccFirst3", "ACC_MEM003", accessibleFamily2.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        var inaccMember1_f3 = new Member("InaccLast1", "InaccFirst1", "INACC_MEM001", inaccessibleFamily.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };

        _context.Members.Add(accMember1_f1);
        _context.Members.Add(accMember2_f1);
        _context.Members.Add(accMember3_f2);
        _context.Members.Add(inaccMember1_f3);

        var accRelMember1 = new Member("AccRelLast1", "AccRelFirst1", "ACC_REL001", accessibleFamily1.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        var accRelMember2 = new Member("AccRelLast2", "AccRelFirst2", "ACC_REL002", accessibleFamily1.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        _context.Members.Add(accRelMember1);
        _context.Members.Add(accRelMember2);

        _context.Relationships.Add(new Relationship(accessibleFamily1.Id, accRelMember1.Id, accRelMember2.Id, RelationshipType.Father, null));

        await _context.SaveChangesAsync();

        var query = new GetDashboardStatsQuery { FamilyId = accessibleFamily1.Id };

        // Act: Gọi handler để xử lý query.
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert: Kiểm tra kết quả trả về là thành công và chứa các thống kê chính xác chỉ cho các gia đình có thể truy cập.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalFamilies.Should().Be(1);
        result.Value!.TotalMembers.Should().Be(4);
        result.Value!.TotalRelationships.Should().Be(1);


        // 💡 Giải thích: Khi người dùng không phải admin, handler sẽ chỉ tính toán thống kê dựa trên các gia đình mà người dùng có quyền truy cập thông qua bảng FamilyUsers.
    }

    [Fact]
    public async Task Handle_ShouldReturnZeroStats_ForNonAdminUserWithNoAccessibleFamilies()
    {
        // 🎯 Mục tiêu của test: Đảm bảo handler trả về thống kê bằng 0 cho người dùng không phải admin và không có quyền truy cập vào bất kỳ gia đình nào.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Thiết lập IAuthorizationService để xác định người dùng không phải admin.
        _authorizationServiceMock.Setup(s => s.IsAdmin()).Returns(false);

        // 2. Thiết lập ICurrentUser với một UserId cụ thể.
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(u => u.UserId).Returns(userId);

        // 3. Thêm dữ liệu mẫu: Families, Members, Relationships nhưng không có FamilyUser nào cho userId này.
        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family 1", Code = "FAM001", IsDeleted = false };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family 2", Code = "FAM002", IsDeleted = false };

        _context.Families.Add(family1);
        _context.Families.Add(family2);

        _context.Members.Add(new Member("Last1", "First1", "MEM001", family1.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() });
        _context.Members.Add(new Member("Last2", "First2", "MEM002", family2.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() });

        await _context.SaveChangesAsync();

        var query = new GetDashboardStatsQuery();

        // Act: Gọi handler để xử lý query.
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert: Kiểm tra kết quả trả về là thành công và tất cả các thống kê đều bằng 0.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalFamilies.Should().Be(0);
        result.Value!.TotalMembers.Should().Be(0);
        result.Value!.TotalRelationships.Should().Be(0);


        // 💡 Giải thích: Khi người dùng không phải admin và không có quyền truy cập vào bất kỳ gia đình nào, handler sẽ trả về thống kê với tất cả các giá trị bằng 0.
    }

    [Fact]
    public async Task Handle_ShouldReturnDashboardStats_WhenFamilyIdIsSpecified()
    {
        // 🎯 Mục tiêu của test: Đảm bảo handler trả về thống kê chính xác khi một FamilyId cụ thể được chỉ định trong query.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Thiết lập IAuthorizationService để xác định người dùng là admin (để đơn giản hóa việc truy cập tất cả gia đình).
        _authorizationServiceMock.Setup(s => s.IsAdmin()).Returns(true);

        // 2. Thêm nhiều gia đình, thành viên và mối quan hệ.
        var targetFamilyId = Guid.NewGuid();
        var targetFamily = new Family { Id = targetFamilyId, Name = "Target Family", Code = "TARGET001", IsDeleted = false };
        var otherFamily1 = new Family { Id = Guid.NewGuid(), Name = "Other Family 1", Code = "OTHER001", IsDeleted = false };
        var otherFamily2 = new Family { Id = Guid.NewGuid(), Name = "Other Family 2", Code = "OTHER002", IsDeleted = false };

        _context.Families.Add(targetFamily);
        _context.Families.Add(otherFamily1);
        _context.Families.Add(otherFamily2);

        var targetMember1 = new Member("TargetLast1", "TargetFirst1", "TMEM001", targetFamily.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        var targetMember2 = new Member("TargetLast2", "TargetFirst2", "TMEM002", targetFamily.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        var otherMember1 = new Member("OtherLast1", "OtherFirst1", "OMEM001", otherFamily1.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };

        _context.Members.Add(targetMember1);
        _context.Members.Add(targetMember2);
        _context.Members.Add(otherMember1);

        var targetRelMember1 = new Member("TargetRelLast1", "TargetRelFirst1", "TREL001", targetFamily.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        var targetRelMember2 = new Member("TargetRelLast2", "TargetRelFirst2", "TREL002", targetFamily.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        _context.Members.Add(targetRelMember1);
        _context.Members.Add(targetRelMember2);

        _context.Relationships.Add(new Relationship(targetFamily.Id, targetRelMember1.Id, targetRelMember2.Id, RelationshipType.Father, null));
        _context.Relationships.Add(new Relationship(otherFamily1.Id, otherMember1.Id, otherMember1.Id, RelationshipType.Mother, null)); // Relationship in other family

        await _context.SaveChangesAsync();

        // 3. Cập nhật query với FamilyId cụ thể.
        var query = new GetDashboardStatsQuery { FamilyId = targetFamilyId };

        // Act: Gọi handler để xử lý query.
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert: Kiểm tra kết quả trả về là thành công và chứa các thống kê chính xác chỉ cho FamilyId đã chỉ định.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalFamilies.Should().Be(1);
        result.Value!.TotalMembers.Should().Be(4); // 2 initial + 2 for relationship
        result.Value!.TotalRelationships.Should().Be(1);


        // 💡 Giải thích: Khi một FamilyId được chỉ định, handler sẽ lọc và chỉ tính toán thống kê cho gia đình đó.
    }

    [Fact]
    public async Task Handle_ShouldCalculateAverageAgeCorrectly()
    {
        // 🎯 Mục tiêu của test: Đảm bảo handler tính toán tuổi trung bình chính xác.

        // Arrange:
        _authorizationServiceMock.Setup(s => s.IsAdmin()).Returns(true);
        _mockDateTime.Setup(dt => dt.Now).Returns(new DateTime(2024, 1, 1)); // Mock current date

        var family = new Family { Id = Guid.NewGuid(), Name = "Family Age", Code = "FAGE", IsDeleted = false };
        _context.Families.Add(family);

        var member1 = new Member("Doe", "John", "JMD1", family.Id, null, Gender.Male.ToString(), new DateTime(1974, 6, 15), null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() }; // Age: 49 (2024 - 1974, month/day not passed yet)
        var member2 = new Member("Doe", "Jane", "JFD1", family.Id, null, Gender.Female.ToString(), new DateTime(1994, 1, 1), null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() }; // Age: 30 (2024 - 1994)
        var member3 = new Member("Doe", "Jim", "JID1", family.Id, null, Gender.Male.ToString(), new DateTime(2004, 12, 1), new DateTime(2020, 1, 1), null, null, null, null, null, null, null, null, null, true) { Id = Guid.NewGuid() }; // Deceased, not counted for living age
        var member4 = new Member("Doe", "Jill", "JLD1", family.Id, null, Gender.Female.ToString(), new DateTime(1980, 1, 1), null, null, null, null, null, null, null, null, null, null, true) { Id = Guid.NewGuid(), IsDeleted = true }; // Deleted, not counted

        _context.Members.AddRange(member1, member2, member3, member4);
        await _context.SaveChangesAsync();

        var query = new GetDashboardStatsQuery { FamilyId = family.Id };

        // Act:
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AverageAge.Should().Be((int)Math.Round((49.0 + 30.0) / 2.0)); // (49+30)/2 = 39.5 -> 40
    }

    [Fact]
    public async Task Handle_ShouldReturnZeroAverageAge_WhenNoLivingMembersWithBirthDate()
    {
        // 🎯 Mục tiêu của test: Đảm bảo handler trả về tuổi trung bình là 0 khi không có thành viên nào còn sống có ngày sinh.

        // Arrange:
        _authorizationServiceMock.Setup(s => s.IsAdmin()).Returns(true);
        _mockDateTime.Setup(dt => dt.Now).Returns(new DateTime(2024, 1, 1));

        var family = new Family { Id = Guid.NewGuid(), Name = "Family No Age", Code = "FNOAGE", IsDeleted = false };
        _context.Families.Add(family);

        // Members with no birth date or are deceased/deleted
        var member1 = new Member("Doe", "John", "JMD1", family.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() }; // No birth date
        var member2 = new Member("Doe", "Jane", "JFD1", family.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() }; // Null birth date
        var member3 = new Member("Doe", "Jim", "JID1", family.Id, null, Gender.Male.ToString(), new DateTime(1950, 1, 1), new DateTime(2000, 1, 1), null, null, null, null, null, null, null, null, null, true) { Id = Guid.NewGuid() }; // Deceased
        var member4 = new Member("Doe", "Jill", "JLD1", family.Id, null, Gender.Female.ToString(), new DateTime(1980, 1, 1), null, null, null, null, null, null, null, null, null, null, true) { Id = Guid.NewGuid(), IsDeleted = true }; // Deleted

        _context.Members.AddRange(member1, member2, member3, member4);
        await _context.SaveChangesAsync();

        var query = new GetDashboardStatsQuery();

        // Act:
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AverageAge.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldCalculateMembersPerGenerationCorrectly()
    {
        // 🎯 Mục tiêu của test: Đảm bảo handler tính toán thành viên trên mỗi thế hệ chính xác.

        // Arrange:
        _authorizationServiceMock.Setup(s => s.IsAdmin()).Returns(true);
        var family = new Family { Id = Guid.NewGuid(), Name = "Family Generations", Code = "FGEN", IsDeleted = false };
        _context.Families.Add(family);

        var gen1_parent1 = new Member("Parent1", "A", "P1", family.Id, null, Gender.Male.ToString(), new DateTime(1950, 1, 1), null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        var gen1_parent2 = new Member("Parent2", "B", "P2", family.Id, null, Gender.Female.ToString(), new DateTime(1955, 1, 1), null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        var gen2_child1 = new Member("Child1", "C", "C1", family.Id, null, Gender.Male.ToString(), new DateTime(1980, 1, 1), null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        var gen2_child2 = new Member("Child2", "D", "C2", family.Id, null, Gender.Female.ToString(), new DateTime(1985, 1, 1), null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        var gen3_grandchild1 = new Member("Grandchild1", "E", "G1", family.Id, null, Gender.Male.ToString(), new DateTime(2010, 1, 1), null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };

        _context.Members.AddRange(gen1_parent1, gen1_parent2, gen2_child1, gen2_child2, gen3_grandchild1);

        _context.Relationships.Add(new Relationship(family.Id, gen1_parent1.Id, gen2_child1.Id, RelationshipType.Father, null));
        _context.Relationships.Add(new Relationship(family.Id, gen1_parent2.Id, gen2_child2.Id, RelationshipType.Mother, null));
        _context.Relationships.Add(new Relationship(family.Id, gen2_child1.Id, gen3_grandchild1.Id, RelationshipType.Father, null));

        await _context.SaveChangesAsync();

        var query = new GetDashboardStatsQuery { FamilyId = family.Id };

        // Act:
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.MembersPerGeneration.Should().HaveCount(3);
        result.Value.MembersPerGeneration[1].Should().Be(2); // gen1_parent1, gen1_parent2
        result.Value.MembersPerGeneration[2].Should().Be(2); // gen2_child1, gen2_child2
        result.Value.MembersPerGeneration[3].Should().Be(1); // gen3_grandchild1
    }

    [Fact]
    public async Task Handle_ShouldFilterDeletedEntitiesCorrectly()
    {
        // 🎯 Mục tiêu của test: Đảm bảo handler lọc các thực thể đã xóa (IsDeleted = true) và không bao gồm chúng trong thống kê.

        // Arrange:
        _authorizationServiceMock.Setup(s => s.IsAdmin()).Returns(true);
        var family = new Family { Id = Guid.NewGuid(), Name = "Family Deleted", Code = "FDELETED", IsDeleted = false };
        _context.Families.Add(family);

        var activeMember = new Member("Active", "Mem", "AM1", family.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        var deletedMember = new Member("Deleted", "Mem", "DM1", family.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, true) { Id = Guid.NewGuid(), IsDeleted = true };
        _context.Members.AddRange(activeMember, deletedMember);

        var activeRelationship = new Relationship(family.Id, activeMember.Id, activeMember.Id, RelationshipType.Father, null);
        var deletedRelationship = new Relationship(family.Id, activeMember.Id, deletedMember.Id, RelationshipType.Husband, null) { IsDeleted = true };
        _context.Relationships.AddRange(activeRelationship, deletedRelationship);

        await _context.SaveChangesAsync();

        var query = new GetDashboardStatsQuery { FamilyId = family.Id };

        // Act:
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalMembers.Should().Be(1); // Only activeMember
        result.Value!.TotalRelationships.Should().Be(1); // Only activeRelationship
    }

    [Fact]
    public async Task Handle_ShouldCalculateGenderRatiosCorrectly()
    {
        // 🎯 Mục tiêu của test: Đảm bảo handler tính toán tỷ lệ giới tính chính xác.

        // Arrange:
        _authorizationServiceMock.Setup(s => s.IsAdmin()).Returns(true);
        var family = new Family { Id = Guid.NewGuid(), Name = "Family Gender", Code = "FGENDER", IsDeleted = false };
        _context.Families.Add(family);

        var maleMember1 = new Member("Male", "One", "M1", family.Id, null, Gender.Male.ToString(), null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        var maleMember2 = new Member("Male", "Two", "M2", family.Id, null, Gender.Male.ToString(), null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        var femaleMember1 = new Member("Female", "One", "F1", family.Id, null, Gender.Female.ToString(), null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        var unknownGenderMember = new Member("Unknown", "Gender", "UG", family.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() }; // Should not be counted in ratio calculation

        _context.Members.AddRange(maleMember1, maleMember2, femaleMember1, unknownGenderMember);
        await _context.SaveChangesAsync();

        var query = new GetDashboardStatsQuery { FamilyId = family.Id };

        // Act:
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.MaleRatio.Should().BeApproximately(0.67, 0.005);
        result.Value.FemaleRatio.Should().BeApproximately(0.33, 0.005);
    }

    [Fact]
    public async Task Handle_ShouldCalculateLivingAndDeceasedMembersCountCorrectly()
    {
        // 🎯 Mục tiêu của test: Đảm bảo handler tính toán số lượng thành viên còn sống và đã mất chính xác.

        // Arrange:
        _authorizationServiceMock.Setup(s => s.IsAdmin()).Returns(true);
        var family = new Family { Id = Guid.NewGuid(), Name = "Family Status", Code = "FSTATUS", IsDeleted = false };
        _context.Families.Add(family);

        var livingMember1 = new Member("Living", "One", "L1", family.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        var livingMember2 = new Member("Living", "Two", "L2", family.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = Guid.NewGuid() };
        var deceasedMember1 = new Member("Deceased", "One", "D1", family.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, true) { Id = Guid.NewGuid() };
        var deletedMember = new Member("Deleted", "One", "DEL1", family.Id, null, null, null, null, null, null, null, null, null, null, null, null, null, true) { Id = Guid.NewGuid(), IsDeleted = true }; // Should not be counted

        _context.Members.AddRange(livingMember1, livingMember2, deceasedMember1, deletedMember);
        await _context.SaveChangesAsync();

        var query = new GetDashboardStatsQuery { FamilyId = family.Id };

        // Act:
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.LivingMembersCount.Should().Be(2);
        result.Value!.DeceasedMembersCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnZeroRatiosAndCounts_WhenNoMembers()
    {
        // 🎯 Mục tiêu của test: Đảm bảo handler trả về tỷ lệ giới tính và số lượng thành viên còn sống/đã mất bằng 0 khi không có thành viên nào.

        // Arrange:
        _authorizationServiceMock.Setup(s => s.IsAdmin()).Returns(true);
        var family = new Family { Id = Guid.NewGuid(), Name = "Family Empty", Code = "FEMPTY", IsDeleted = false };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var query = new GetDashboardStatsQuery();

        // Act:
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.MaleRatio.Should().Be(0.0);
        result.Value!.FemaleRatio.Should().Be(0.0);
        result.Value!.LivingMembersCount.Should().Be(0);
        result.Value!.DeceasedMembersCount.Should().Be(0);
    }
}
