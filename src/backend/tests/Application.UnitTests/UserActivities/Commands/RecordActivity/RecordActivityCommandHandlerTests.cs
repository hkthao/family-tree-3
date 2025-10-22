using AutoFixture;
using backend.Application.Common.Models;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace backend.Application.UnitTests.UserActivities.Commands.RecordActivity;

public class RecordActivityCommandHandlerTests : TestBase
{
    private readonly RecordActivityCommandHandler _handler;

    public RecordActivityCommandHandlerTests()
    {
        _handler = new RecordActivityCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldRecordActivitySuccessfully_WhenTargetTypeIsFamily()
    {
        // 🎯 Mục tiêu của test: Xác minh hoạt động được ghi lại thành công khi TargetType là Family.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Tạo một RecordActivityCommand với TargetType.Family và một TargetId hợp lệ (là một Guid của Family).
        var familyId = Guid.NewGuid();
        var command = _fixture.Build<RecordActivityCommand>()
            .With(c => c.TargetType, TargetType.Family)
            .With(c => c.TargetId, familyId.ToString())
            .Without(c => c.Metadata)
            .Create();

        // 2. Act: Gọi phương thức Handle của handler.
        var result = await _handler.Handle(command, CancellationToken.None);

        // 3. Assert: Kiểm tra rằng Result trả về là thành công, và một UserActivity mới đã được thêm vào DB với GroupId chính xác.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var recordedActivity = _context.UserActivities.FirstOrDefault(ua => ua.Id == result.Value);
        recordedActivity.Should().NotBeNull();
        recordedActivity!.UserProfileId.Should().Be(command.UserProfileId);
        recordedActivity.ActionType.Should().Be(command.ActionType);
        recordedActivity.TargetType.Should().Be(command.TargetType);
        recordedActivity.TargetId.Should().Be(command.TargetId);
        recordedActivity.GroupId.Should().Be(familyId);
        recordedActivity.Metadata.Should().Be(command.Metadata);
        recordedActivity.ActivitySummary.Should().Be(command.ActivitySummary);
        // 💡 Giải thích: Handler phải tạo một UserActivity và gán GroupId từ TargetId của Family.
    }

    [Fact]
    public async Task Handle_ShouldRecordActivitySuccessfully_WhenTargetTypeIsMember()
    {
        // 🎯 Mục tiêu của test: Xác minh hoạt động được ghi lại thành công khi TargetType là Member.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Tạo một Family và một Member thuộc về Family đó. Thêm Family và Member vào DB.

        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        var family = new Family { Id = familyId, Name = "Test Family", Code = "FAM001" };
        var member = new Member { Id = memberId, FamilyId = familyId, LastName = "Test", FirstName = "Member", Code = "MEM001" };

        _context.Families.Add(family);
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var command = _fixture.Build<RecordActivityCommand>()
            .With(c => c.TargetType, TargetType.Member)
            .With(c => c.TargetId, memberId.ToString())
            .Without(c => c.Metadata)
            .Create();

        // 2. Act: Gọi phương thức Handle của handler.
        var result = await _handler.Handle(command, CancellationToken.None);

        // 3. Assert: Kiểm tra rằng Result trả về là thành công, và một UserActivity mới đã được thêm vào DB với GroupId là FamilyId của Member.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var recordedActivity = _context.UserActivities.FirstOrDefault(ua => ua.Id == result.Value);
        recordedActivity.Should().NotBeNull();
        recordedActivity!.UserProfileId.Should().Be(command.UserProfileId);
        recordedActivity.ActionType.Should().Be(command.ActionType);
        recordedActivity.TargetType.Should().Be(command.TargetType);
        recordedActivity.TargetId.Should().Be(command.TargetId);
        recordedActivity.GroupId.Should().Be(family.Id);
        recordedActivity.Metadata.Should().Be(command.Metadata);
        recordedActivity.ActivitySummary.Should().Be(command.ActivitySummary);
        // 💡 Giải thích: Handler phải tìm Member, lấy FamilyId của Member đó và gán cho GroupId của UserActivity.
    }

    [Fact]
    public async Task Handle_ShouldRecordActivitySuccessfully_WhenTargetTypeIsEvent()
    {
        // 🎯 Mục tiêu của test: Xác minh hoạt động được ghi lại thành công khi TargetType là Event.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Tạo một Family và một Event thuộc về Family đó. Thêm Family và Event vào DB.

        var family = _fixture.Create<Family>();
        var @event = _fixture.Build<Event>()
            .With(e => e.FamilyId, family.Id)
            .Create();
        _context.Families.Add(family);
        _context.Events.Add(@event);
        await _context.SaveChangesAsync();

        var command = _fixture.Build<RecordActivityCommand>()
            .With(c => c.TargetType, TargetType.Event)
            .With(c => c.TargetId, @event.Id.ToString())
            .Without(c => c.Metadata)
            .Create();

        // 2. Act: Gọi phương thức Handle của handler.
        var result = await _handler.Handle(command, CancellationToken.None);

        // 3. Assert: Kiểm tra rằng Result trả về là thành công, và một UserActivity mới đã được thêm vào DB với GroupId là FamilyId của Event.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var recordedActivity = _context.UserActivities.FirstOrDefault(ua => ua.Id == result.Value);
        recordedActivity.Should().NotBeNull();
        recordedActivity!.UserProfileId.Should().Be(command.UserProfileId);
        recordedActivity.ActionType.Should().Be(command.ActionType);
        recordedActivity.TargetType.Should().Be(command.TargetType);
        recordedActivity.TargetId.Should().Be(command.TargetId);
        recordedActivity.GroupId.Should().Be(family.Id);
        recordedActivity.Metadata.Should().Be(command.Metadata);
        recordedActivity.ActivitySummary.Should().Be(command.ActivitySummary);
        // 💡 Giải thích: Handler phải tìm Event, lấy FamilyId của Event đó và gán cho GroupId của UserActivity.
    }
    [Fact]
    public async Task Handle_ShouldRecordActivitySuccessfully_WhenTargetTypeIsRelationship()
    {

        // 🎯 Mục tiêu của test: Xác minh hoạt động được ghi lại thành công khi TargetType là Relationship.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Tạo một Family, hai Member thuộc về Family đó, và một Relationship giữa hai Member. Thêm vào DB.

        var familyId = Guid.NewGuid();
        var member1Id = Guid.NewGuid();
        var member2Id = Guid.NewGuid();
        var relationshipId = Guid.NewGuid();

        var family = new Family { Id = familyId, Name = "Test Family", Code = "FAM002" };
        var member1 = new Member { Id = member1Id, FamilyId = familyId, LastName = "Test", FirstName = "Member1", Code = "MEM002" };
        var member2 = new Member { Id = member2Id, FamilyId = familyId, LastName = "Test", FirstName = "Member2", Code = "MEM003" };
        var relationship = new Relationship { Id = relationshipId, SourceMemberId = member1Id, TargetMemberId = member2Id, Type = RelationshipType.Father };

        _context.Families.Add(family);
        _context.Members.AddRange(member1, member2);
        _context.Relationships.Add(relationship);
        await _context.SaveChangesAsync();

        var command = _fixture.Build<RecordActivityCommand>()
            .With(c => c.TargetType, TargetType.Relationship)
            .With(c => c.TargetId, relationshipId.ToString())
            .Without(c => c.Metadata)
            .Create();

        // 2. Act: Gọi phương thức Handle của handler.
        var result = await _handler.Handle(command, CancellationToken.None);

        // 3. Assert: Kiểm tra rằng Result trả về là thành công, và một UserActivity mới đã được thêm vào DB với GroupId là FamilyId của SourceMember của Relationship.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var recordedActivity = _context.UserActivities.FirstOrDefault(ua => ua.Id == result.Value);
        recordedActivity.Should().NotBeNull();
        recordedActivity!.UserProfileId.Should().Be(command.UserProfileId);
        recordedActivity.ActionType.Should().Be(command.ActionType);
        recordedActivity.TargetType.Should().Be(command.TargetType);
        recordedActivity.TargetId.Should().Be(command.TargetId);
        recordedActivity.GroupId.Should().Be(family.Id);
        recordedActivity.Metadata.Should().Be(command.Metadata);
        recordedActivity.ActivitySummary.Should().Be(command.ActivitySummary);
        // 💡 Giải thích: Handler phải tìm Relationship, sau đó tìm SourceMember của Relationship, lấy FamilyId của SourceMember đó và gán cho GroupId của UserActivity.
    }
    [Fact]
    public async Task Handle_ShouldRecordActivitySuccessfully_WhenTargetTypeIsUserProfile()
    {

        // 🎯 Mục tiêu của test: Xác minh hoạt động được ghi lại thành công khi TargetType là UserProfile.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Tạo một RecordActivityCommand với TargetType.UserProfile và một TargetId hợp lệ.

        var userProfileId = Guid.NewGuid();
        var command = _fixture.Build<RecordActivityCommand>()
            .With(c => c.TargetType, TargetType.UserProfile)
            .With(c => c.TargetId, userProfileId.ToString())
            .Without(c => c.Metadata)
            .Create();

        // 2. Act: Gọi phương thức Handle của handler.
        var result = await _handler.Handle(command, CancellationToken.None);

        // 3. Assert: Kiểm tra rằng Result trả về là thành công, và một UserActivity mới đã được thêm vào DB với GroupId là null.

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var recordedActivity = _context.UserActivities.FirstOrDefault(ua => ua.Id == result.Value);
        recordedActivity.Should().NotBeNull();
        recordedActivity!.UserProfileId.Should().Be(command.UserProfileId);
        recordedActivity.ActionType.Should().Be(command.ActionType);
        recordedActivity.TargetType.Should().Be(command.TargetType);
        recordedActivity.TargetId.Should().Be(command.TargetId);
        recordedActivity.GroupId.Should().BeNull();
        recordedActivity.Metadata.Should().Be(command.Metadata);
        recordedActivity.ActivitySummary.Should().Be(command.ActivitySummary);
        // 💡 Giải thích: Khi TargetType là UserProfile, GroupId phải là null.

    }

    [Fact]
    public async Task Handle_ShouldRecordActivitySuccessfully_WhenTargetIdIsNull()

    {

        // 🎯 Mục tiêu của test: Xác minh hoạt động được ghi lại thành công khi TargetId là null.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Tạo một RecordActivityCommand với TargetId là null.

        var command = _fixture.Build<RecordActivityCommand>()
            .With(c => c.TargetId, (string?)null)
            .Without(c => c.Metadata)
            .Create();

        // 2. Act: Gọi phương thức Handle của handler.
        var result = await _handler.Handle(command, CancellationToken.None);

        // 3. Assert: Kiểm tra rằng Result trả về là thành công, và một UserActivity mới đã được thêm vào DB với GroupId là null.

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var recordedActivity = _context.UserActivities.FirstOrDefault(ua => ua.Id == result.Value);
        recordedActivity.Should().NotBeNull();
        recordedActivity!.UserProfileId.Should().Be(command.UserProfileId);
        recordedActivity.ActionType.Should().Be(command.ActionType);

        recordedActivity.TargetType.Should().Be(command.TargetType);

        recordedActivity.TargetId.Should().Be(command.TargetId);

        recordedActivity.GroupId.Should().BeNull();

        recordedActivity.Metadata.Should().Be(command.Metadata);

        recordedActivity.ActivitySummary.Should().Be(command.ActivitySummary);

        // 💡 Giải thích: Khi TargetId là null, GroupId phải là null.

    }
}
