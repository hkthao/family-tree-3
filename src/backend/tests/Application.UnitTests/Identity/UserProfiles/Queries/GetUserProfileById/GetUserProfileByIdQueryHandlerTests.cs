using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Identity.UserProfiles.Queries;
using backend.Application.Identity.UserProfiles.Queries.GetUserProfileById;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Identity.UserProfiles.Queries.GetUserProfileById;

public class GetUserProfileByIdQueryHandlerTests : TestBase
{
    private readonly GetUserProfileByIdQueryHandler _handler;

    public GetUserProfileByIdQueryHandlerTests()
    {
        _fixture.Customize(new AutoMoqCustomization());

        _handler = new GetUserProfileByIdQueryHandler(
            _context,
            _mapper
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureWhenUserProfileNotFound()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi không tìm thấy hồ sơ người dùng.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một GetUserProfileByIdQuery với Id không tồn tại trong DB.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var query = new GetUserProfileByIdQuery { Id = Guid.NewGuid() };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User profile not found.");
        result.ErrorSource.Should().Be("NotFound");
        // 💡 Giải thích: Hồ sơ người dùng phải tồn tại để có thể truy xuất.
    }

    [Fact]
    public async Task Handle_ShouldReturnUserProfileByIdSuccessfully()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về hồ sơ người dùng theo Id thành công.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thêm một UserProfile vào Context với một Id cụ thể.
        // 2. Act: Gọi phương thức Handle với GetUserProfileByIdQuery chứa Id đó.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chứa UserProfileDto khớp với hồ sơ người dùng đã thêm.
        var userProfileId = Guid.NewGuid();
        var existingUserProfile = new UserProfile
        {
            Id = userProfileId,
            ExternalId = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            Name = "Test User"
        };
        _context.UserProfiles.Add(existingUserProfile);
        await _context.SaveChangesAsync();

        var query = new GetUserProfileByIdQuery { Id = userProfileId };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(userProfileId.ToString());
        result.Value.ExternalId.Should().Be(existingUserProfile.ExternalId);
        result.Value.Email.Should().Be("test@example.com");
        result.Value.Name.Should().Be("Test User");
        // 💡 Giải thích: Handler phải truy xuất và ánh xạ đúng hồ sơ người dùng dựa trên Id.
    }
}
