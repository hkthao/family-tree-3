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