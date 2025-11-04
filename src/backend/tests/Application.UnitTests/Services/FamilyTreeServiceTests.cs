using backend.Application.Services;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using backend.Domain.Enums;

namespace backend.Application.UnitTests.Services;

/// <summary>
/// 🎯 Mục tiêu: Kiểm thử hành vi của FamilyTreeService.
/// ⚙️ Các bước: Arrange - Act - Assert.
/// 💡 Giải thích: Đảm bảo service tính toán và cập nhật số liệu thống kê cây gia phả một cách chính xác.
/// </summary>
public class FamilyTreeServiceTests : TestBase
{
    private readonly FamilyTreeService _service;

    public FamilyTreeServiceTests()
    {
        _service = new FamilyTreeService(_context);
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra CalculateTotalMembers trả về 0 khi không có thành viên nào trong gia đình.
    /// ⚙️ Arrange: Tạo một FamilyId không có thành viên liên quan.
    /// ⚙️ Act: Gọi CalculateTotalMembers.
    /// ⚙️ Assert: Kỳ vọng kết quả là 0.
    /// 💡 Giải thích: Service phải xử lý đúng trường hợp gia đình không có thành viên.
    /// </summary>
    [Fact]
    public async Task CalculateTotalMembers_ShouldReturnZero_WhenNoMembersExist()
    {
        // Arrange
        var familyId = Guid.NewGuid();

        // Act
        var result = await _service.CalculateTotalMembers(familyId);

        // Assert
        result.Should().Be(0);
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra CalculateTotalMembers trả về số lượng thành viên chính xác.
    /// ⚙️ Arrange: Thêm một số thành viên vào một gia đình cụ thể.
    /// ⚙️ Act: Gọi CalculateTotalMembers.
    /// ⚙️ Assert: Kỳ vọng kết quả là số lượng thành viên đã thêm.
    /// 💡 Giải thích: Service phải tính toán đúng tổng số thành viên.
    /// </summary>
    [Fact]
    public async Task CalculateTotalMembers_ShouldReturnCorrectCount_WhenMembersExist()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _context.Members.Add(new Member("Member", "1", "M1", familyId) { Id = Guid.NewGuid() });
        _context.Members.Add(new Member("Member", "2", "M2", familyId) { Id = Guid.NewGuid() });
        _context.Members.Add(new Member("Member", "3", "M3", familyId) { Id = Guid.NewGuid() });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CalculateTotalMembers(familyId);

        // Assert
        result.Should().Be(3);
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra CalculateTotalGenerations trả về 0 khi không có thành viên nào trong gia đình.
    /// ⚙️ Arrange: Tạo một FamilyId không có thành viên liên quan.
    /// ⚙️ Act: Gọi CalculateTotalGenerations.
    /// ⚙️ Assert: Kỳ vọng kết quả là 0.
    /// 💡 Giải thích: Service phải xử lý đúng trường hợp gia đình không có thành viên.
    /// </summary>
    [Fact]
    public async Task CalculateTotalGenerations_ShouldReturnZero_WhenNoMembersExist()
    {
        // Arrange
        var familyId = Guid.NewGuid();

        // Act
        var result = await _service.CalculateTotalGenerations(familyId);

        // Assert
        result.Should().Be(0);
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra CalculateTotalGenerations trả về 1 cho một thành viên duy nhất không có mối quan hệ.
    /// ⚙️ Arrange: Thêm một thành viên duy nhất vào một gia đình.
    /// ⚙️ Act: Gọi CalculateTotalGenerations.
    /// ⚙️ Assert: Kỳ vọng kết quả là 1.
    /// 💡 Giải thích: Một thành viên không có mối quan hệ được coi là 1 thế hệ.
    /// </summary>
    [Fact]
    public async Task CalculateTotalGenerations_ShouldReturnOne_ForSingleMember()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _context.Members.Add(new Member("Single", "Member", "SM1", familyId) { Id = Guid.NewGuid() });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CalculateTotalGenerations(familyId);

        // Assert
        result.Should().Be(1);
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra CalculateTotalGenerations trả về số thế hệ chính xác cho một gia đình tuyến tính (cha-con).
    /// ⚙️ Arrange: Tạo 3 thành viên với mối quan hệ cha-con tuyến tính.
    /// ⚙️ Act: Gọi CalculateTotalGenerations.
    /// ⚙️ Assert: Kỳ vọng kết quả là 3 thế hệ.
    /// 💡 Giải thích: Service phải tính toán đúng số thế hệ trong một chuỗi quan hệ đơn giản.
    /// </summary>
    [Fact]
    public async Task CalculateTotalGenerations_ShouldReturnCorrectGenerations_ForLinearFamily()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var grandParent = new Member("Grand", "Parent", "GP1", familyId) { Id = Guid.NewGuid() };
        var parent = new Member("Parent", "Child", "PC1", familyId) { Id = Guid.NewGuid() };
        var child = new Member("Child", "Grandchild", "GC1", familyId) { Id = Guid.NewGuid() };

        _context.Members.AddRange(grandParent, parent, child);
        _context.Relationships.Add(new Relationship(familyId, grandParent.Id, parent.Id, RelationshipType.Father) { Id = Guid.NewGuid() });
        _context.Relationships.Add(new Relationship(familyId, parent.Id, child.Id, RelationshipType.Mother) { Id = Guid.NewGuid() });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CalculateTotalGenerations(familyId);

        // Assert
        result.Should().Be(3);
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra CalculateTotalGenerations trả về số thế hệ chính xác cho một gia đình phức tạp hơn.
    /// ⚙️ Arrange: Tạo một cây gia phả với nhiều nhánh và gốc.
    /// ⚙️ Act: Gọi CalculateTotalGenerations.
    /// ⚙️ Assert: Kỳ vọng kết quả là số thế hệ tối đa trong cây.
    /// 💡 Giải thích: Service phải xử lý đúng các cấu trúc cây phức tạp.
    /// </summary>
    [Fact]
    public async Task CalculateTotalGenerations_ShouldReturnCorrectGenerations_ForComplexFamily()
    {
        // Arrange
        var familyId = Guid.NewGuid();

        var gen1_root1 = new Member("Root1", "Gen1", "G1R1", familyId) { Id = Guid.NewGuid() };
        var gen1_root2 = new Member("Root2", "Gen1", "G1R2", familyId) { Id = Guid.NewGuid() };

        var gen2_child1_of_root1 = new Member("Child1", "Gen2", "G2C1R1", familyId) { Id = Guid.NewGuid() };
        var gen2_child2_of_root1 = new Member("Child2", "Gen2", "G2C2R1", familyId) { Id = Guid.NewGuid() };
        var gen2_child1_of_root2 = new Member("Child1", "Gen2", "G2C1R2", familyId) { Id = Guid.NewGuid() };

        var gen3_child1_of_gen2_child1 = new Member("Child1", "Gen3", "G3C1G2C1", familyId) { Id = Guid.NewGuid() };

        _context.Members.AddRange(
            gen1_root1, gen1_root2,
            gen2_child1_of_root1, gen2_child2_of_root1, gen2_child1_of_root2,
            gen3_child1_of_gen2_child1
        );

        _context.Relationships.AddRange(
            new Relationship(familyId, gen1_root1.Id, gen2_child1_of_root1.Id, RelationshipType.Father) { Id = Guid.NewGuid() },
            new Relationship(familyId, gen1_root1.Id, gen2_child2_of_root1.Id, RelationshipType.Mother) { Id = Guid.NewGuid() },
            new Relationship(familyId, gen1_root2.Id, gen2_child1_of_root2.Id, RelationshipType.Father) { Id = Guid.NewGuid() },
            new Relationship(familyId, gen2_child1_of_root1.Id, gen3_child1_of_gen2_child1.Id, RelationshipType.Father) { Id = Guid.NewGuid() }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CalculateTotalGenerations(familyId);

        // Assert
        result.Should().Be(3); // Gen1 -> Gen2 -> Gen3
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra UpdateFamilyStats không làm gì khi Family không tồn tại.
    /// ⚙️ Arrange: Tạo một FamilyId không tồn tại.
    /// ⚙️ Act: Gọi UpdateFamilyStats.
    /// ⚙️ Assert: Không có thay đổi nào được lưu vào cơ sở dữ liệu.
    /// 💡 Giải thích: Service phải xử lý an toàn trường hợp Family không tồn tại.
    /// </summary>
    [Fact]
    public async Task UpdateFamilyStats_ShouldDoNothing_WhenFamilyNotFound()
    {
        // Arrange
        var nonExistentFamilyId = Guid.NewGuid();
        var initialFamilyCount = await _context.Families.CountAsync();

        // Act
        await _service.UpdateFamilyStats(nonExistentFamilyId);

        // Assert
        var finalFamilyCount = await _context.Families.CountAsync();
        finalFamilyCount.Should().Be(initialFamilyCount);
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra UpdateFamilyStats cập nhật TotalMembers và TotalGenerations chính xác.
    /// ⚙️ Arrange: Tạo một gia đình với một số thành viên và mối quan hệ.
    /// ⚙️ Act: Gọi UpdateFamilyStats.
    /// ⚙️ Assert: Kỳ vọng TotalMembers và TotalGenerations của gia đình được cập nhật đúng.
    /// 💡 Giải thích: Service phải tính toán và lưu trữ các số liệu thống kê chính xác.
    /// </summary>
    [Fact]
    public async Task UpdateFamilyStats_ShouldUpdateStatsCorrectly_WhenFamilyExists()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1", TotalMembers = 0, TotalGenerations = 0 };
        _context.Families.Add(family);

        var grandParent = new Member("Grand", "Parent", "GP1", familyId) { Id = Guid.NewGuid() };
        var parent = new Member("Parent", "Child", "PC1", familyId) { Id = Guid.NewGuid() };
        var child = new Member("Child", "Grandchild", "GC1", familyId) { Id = Guid.NewGuid() };

        _context.Members.AddRange(grandParent, parent, child);
        _context.Relationships.Add(new Relationship(familyId, grandParent.Id, parent.Id, RelationshipType.Father) { Id = Guid.NewGuid() });
        _context.Relationships.Add(new Relationship(familyId, parent.Id, child.Id, RelationshipType.Mother) { Id = Guid.NewGuid() });
        await _context.SaveChangesAsync();

        // Act
        await _service.UpdateFamilyStats(familyId);

        // Assert
        var updatedFamily = await _context.Families.FindAsync(familyId);
        updatedFamily.Should().NotBeNull();
        updatedFamily!.TotalMembers.Should().Be(3);
        updatedFamily.TotalGenerations.Should().Be(3);
    }
}