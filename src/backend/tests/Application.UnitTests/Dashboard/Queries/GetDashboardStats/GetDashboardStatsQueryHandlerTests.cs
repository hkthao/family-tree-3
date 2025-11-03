using backend.Application.Common.Interfaces;
using backend.Application.Dashboard.Queries.GetDashboardStats;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Dashboard.Queries.GetDashboardStats;

public class GetDashboardStatsQueryHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly GetDashboardStatsQueryHandler _handler;

    public GetDashboardStatsQueryHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _currentUserMock = new Mock<ICurrentUser>();
        _handler = new GetDashboardStatsQueryHandler(_context, _authorizationServiceMock.Object, _currentUserMock.Object);
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
        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family 1", Code = "FAM001", TotalGenerations = 3 };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family 2", Code = "FAM002", TotalGenerations = 2 };
        var family3 = new Family { Id = Guid.NewGuid(), Name = "Family 3", Code = "FAM003", TotalGenerations = 1 };

        _context.Families.Add(family1);
        _context.Families.Add(family2);
        _context.Families.Add(family3);

        var member1_f1 = new Member("Last1", "First1", "MEM001", family1.Id) { Id = Guid.NewGuid() };
        var member2_f1 = new Member("Last2", "First2", "MEM002", family1.Id) { Id = Guid.NewGuid() };
        var member3_f2 = new Member("Last3", "First3", "MEM003", family2.Id) { Id = Guid.NewGuid() };
        var member4_f3 = new Member("Last4", "First4", "MEM004", family3.Id) { Id = Guid.NewGuid() };

        _context.Members.Add(member1_f1);
        _context.Members.Add(member2_f1);
        _context.Members.Add(member3_f2);
        _context.Members.Add(member4_f3);

        var relMember1_f1 = new Member("RelLast1", "RelFirst1", "REL001", family1.Id) { Id = Guid.NewGuid() };
        var relMember2_f1 = new Member("RelLast2", "RelFirst2", "REL002", family1.Id) { Id = Guid.NewGuid() };
        _context.Members.Add(relMember1_f1);
        _context.Members.Add(relMember2_f1);

        _context.Relationships.Add(new Relationship(family1.Id, relMember1_f1.Id, relMember2_f1.Id, RelationshipType.Father));

        await _context.SaveChangesAsync();

        _context.Families.Count().Should().Be(3);

        var query = new GetDashboardStatsQuery();

        // Act: Gọi handler để xử lý query.
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert: Kiểm tra kết quả trả về là thành công và chứa các thống kê chính xác.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalFamilies.Should().Be(3);
        result.Value!.TotalMembers.Should().Be(6); // 4 initial + 2 for relationship
        result.Value!.TotalRelationships.Should().Be(1);
        result.Value!.TotalGenerations.Should().Be(6); // 3 + 2 + 1

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
        var accessibleFamily1 = new Family { Id = Guid.NewGuid(), Name = "Accessible Family 1", Code = "ACC001", TotalGenerations = 4 };
        var accessibleFamily2 = new Family { Id = Guid.NewGuid(), Name = "Accessible Family 2", Code = "ACC002", TotalGenerations = 2 };
        var inaccessibleFamily = new Family { Id = Guid.NewGuid(), Name = "Inaccessible Family", Code = "INACC001", TotalGenerations = 1 };

        _context.Families.Add(accessibleFamily1);
        _context.Families.Add(accessibleFamily2);
        _context.Families.Add(inaccessibleFamily);

        _context.FamilyUsers.Add(new FamilyUser(accessibleFamily1.Id, userId, FamilyRole.Admin));
        _context.FamilyUsers.Add(new FamilyUser(accessibleFamily2.Id, userId, FamilyRole.Admin));

        var accMember1_f1 = new Member("AccLast1", "AccFirst1", "ACC_MEM001", accessibleFamily1.Id) { Id = Guid.NewGuid() };
        var accMember2_f1 = new Member("AccLast2", "AccFirst2", "ACC_MEM002", accessibleFamily1.Id) { Id = Guid.NewGuid() };
        var accMember3_f2 = new Member("AccLast3", "AccFirst3", "ACC_MEM003", accessibleFamily2.Id) { Id = Guid.NewGuid() };
        var inaccMember1_f3 = new Member("InaccLast1", "InaccFirst1", "INACC_MEM001", inaccessibleFamily.Id) { Id = Guid.NewGuid() };

        _context.Members.Add(accMember1_f1);
        _context.Members.Add(accMember2_f1);
        _context.Members.Add(accMember3_f2);
        _context.Members.Add(inaccMember1_f3);

        var accRelMember1 = new Member("AccRelLast1", "AccRelFirst1", "ACC_REL001", accessibleFamily1.Id) { Id = Guid.NewGuid() };
        var accRelMember2 = new Member("AccRelLast2", "AccRelFirst2", "ACC_REL002", accessibleFamily1.Id) { Id = Guid.NewGuid() };
        _context.Members.Add(accRelMember1);
        _context.Members.Add(accRelMember2);

        _context.Relationships.Add(new Relationship(accessibleFamily1.Id, accRelMember1.Id, accRelMember2.Id, RelationshipType.Father));

        await _context.SaveChangesAsync();

        var query = new GetDashboardStatsQuery();

        // Act: Gọi handler để xử lý query.
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert: Kiểm tra kết quả trả về là thành công và chứa các thống kê chính xác chỉ cho các gia đình có thể truy cập.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalFamilies.Should().Be(2);
        result.Value!.TotalMembers.Should().Be(5); // 3 initial accessible + 2 for relationship
        result.Value!.TotalRelationships.Should().Be(1);
        result.Value!.TotalGenerations.Should().Be(6); // 4 + 2

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
        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family 1", Code = "FAM001", TotalGenerations = 3 };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family 2", Code = "FAM002", TotalGenerations = 2 };

        _context.Families.Add(family1);
        _context.Families.Add(family2);

        _context.Members.Add(new Member("Last1", "First1", "MEM001", family1.Id) { Id = Guid.NewGuid() });
        _context.Members.Add(new Member("Last2", "First2", "MEM002", family2.Id) { Id = Guid.NewGuid() });

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
        result.Value!.TotalGenerations.Should().Be(0);

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
        var targetFamily = new Family { Id = targetFamilyId, Name = "Target Family", Code = "TARGET001", TotalGenerations = 5 };
        var otherFamily1 = new Family { Id = Guid.NewGuid(), Name = "Other Family 1", Code = "OTHER001", TotalGenerations = 2 };
        var otherFamily2 = new Family { Id = Guid.NewGuid(), Name = "Other Family 2", Code = "OTHER002", TotalGenerations = 1 };

        _context.Families.Add(targetFamily);
        _context.Families.Add(otherFamily1);
        _context.Families.Add(otherFamily2);

        var targetMember1 = new Member("TargetLast1", "TargetFirst1", "TMEM001", targetFamily.Id) { Id = Guid.NewGuid() };
        var targetMember2 = new Member("TargetLast2", "TargetFirst2", "TMEM002", targetFamily.Id) { Id = Guid.NewGuid() };
        var otherMember1 = new Member("OtherLast1", "OtherFirst1", "OMEM001", otherFamily1.Id) { Id = Guid.NewGuid() };

        _context.Members.Add(targetMember1);
        _context.Members.Add(targetMember2);
        _context.Members.Add(otherMember1);

        var targetRelMember1 = new Member("TargetRelLast1", "TargetRelFirst1", "TREL001", targetFamily.Id) { Id = Guid.NewGuid() };
        var targetRelMember2 = new Member("TargetRelLast2", "TargetRelFirst2", "TREL002", targetFamily.Id) { Id = Guid.NewGuid() };
        _context.Members.Add(targetRelMember1);
        _context.Members.Add(targetRelMember2);

        _context.Relationships.Add(new Relationship(targetFamily.Id, targetRelMember1.Id, targetRelMember2.Id, RelationshipType.Father));
        _context.Relationships.Add(new Relationship(otherFamily1.Id, otherMember1.Id, otherMember1.Id, RelationshipType.Mother)); // Relationship in other family

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
        result.Value!.TotalGenerations.Should().Be(5);

        // 💡 Giải thích: Khi một FamilyId được chỉ định, handler sẽ lọc và chỉ tính toán thống kê cho gia đình đó.
    }
}
