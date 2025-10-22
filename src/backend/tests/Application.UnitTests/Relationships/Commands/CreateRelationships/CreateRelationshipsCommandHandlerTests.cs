using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Services;
using backend.Application.Relationships.Commands.CreateRelationship;
using backend.Application.Relationships.Commands.CreateRelationships;
using backend.Application.Relationships.Commands.Inputs;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Commands.CreateRelationships;

public class CreateRelationshipsCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _mockAuthorizationService;
    private readonly Mock<FamilyAuthorizationService> _mockFamilyAuthorizationService;
    private readonly Mock<IMediator> _mockMediator;
    private readonly CreateRelationshipsCommandHandler _handler;

    public CreateRelationshipsCommandHandlerTests()
    {
        _mockAuthorizationService = new Mock<IAuthorizationService>();
        _mockFamilyAuthorizationService = new Mock<FamilyAuthorizationService>(_context, _mockUser.Object, _mockAuthorizationService.Object);
        _mockMediator = new Mock<IMediator>();
        _fixture.Customize(new AutoMoqCustomization());

        _handler = new CreateRelationshipsCommandHandler(
            _context,
            _mockUser.Object,
            _mockAuthorizationService.Object,
            _mockFamilyAuthorizationService.Object,
            _mockMediator.Object
        );
    }

    // Concrete implementation for testing abstract RelationshipInput
    private record TestRelationshipInput : RelationshipInput;

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenAllRelationshipsAreCreatedSuccessfully()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về thành công khi tất cả các mối quan hệ con được tạo thành công.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockMediator để trả về Result<Guid>.Success cho mỗi CreateRelationshipCommand được gửi.
        // 2. Act: Gọi phương thức Handle với một danh sách các RelationshipInput hợp lệ.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chứa danh sách các Guid của các mối quan hệ đã tạo.
        var relationshipInputs = new List<TestRelationshipInput>
            {
                new TestRelationshipInput { SourceMemberId = Guid.NewGuid(), TargetMemberId = Guid.NewGuid(), Type = RelationshipType.Father, Order = 1 },
                new TestRelationshipInput { SourceMemberId = Guid.NewGuid(), TargetMemberId = Guid.NewGuid(), Type = RelationshipType.Mother, Order = 2 }
            };

        _mockMediator.Setup(m => m.Send(It.IsAny<CreateRelationshipCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));

        var command = new CreateRelationshipsCommand { Relationships = relationshipInputs.Cast<RelationshipInput>().ToList() };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(relationshipInputs.Count);
        _mockMediator.Verify(m => m.Send(It.IsAny<CreateRelationshipCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(relationshipInputs.Count));
        // 💡 Giải thích: Handler phải tổng hợp các kết quả thành công từ các lệnh con.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAnyRelationshipCreationFails()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về thất bại ngay lập tức khi bất kỳ mối quan hệ con nào không tạo được.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockMediator để trả về Result<Guid>.Success cho lệnh đầu tiên và Result<Guid>.Failure cho lệnh thứ hai.
        // 2. Act: Gọi phương thức Handle với một danh sách các RelationshipInput.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và chứa thông báo lỗi từ lệnh con thất bại.
        var relationshipInputs = new List<TestRelationshipInput>
                        {
                            new TestRelationshipInput { SourceMemberId = Guid.NewGuid(), TargetMemberId = Guid.NewGuid(), Type = RelationshipType.Father, Order = 1 },
                            new TestRelationshipInput { SourceMemberId = Guid.NewGuid(), TargetMemberId = Guid.NewGuid(), Type = RelationshipType.Mother, Order = 2 }
                        };

        var firstRelationshipId = Guid.NewGuid();
        var errorMessage = "Failed to create second relationship.";

        _mockMediator.SetupSequence(m => m.Send(It.IsAny<CreateRelationshipCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(firstRelationshipId))
            .ReturnsAsync(Result<Guid>.Failure(errorMessage, "Validation"));

        var command = new CreateRelationshipsCommand { Relationships = relationshipInputs.Cast<RelationshipInput>().ToList() };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(errorMessage);
        result.ErrorSource.Should().Be("Validation");
        _mockMediator.Verify(m => m.Send(It.IsAny<CreateRelationshipCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        // 💡 Giải thích: Handler phải dừng lại và trả về lỗi ngay khi một lệnh con thất bại.
    }

    [Fact]
    public async Task Handle_ShouldCallCreateRelationshipCommandForEachInput()
    {
        // 🎯 Mục tiêu của test: Xác minh handler gọi _mediator.Send cho mỗi RelationshipInput trong danh sách.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockMediator để trả về Result<Guid>.Success cho mỗi CreateRelationshipCommand.
        // 2. Act: Gọi phương thức Handle với một danh sách nhiều RelationshipInput.
        // 3. Assert: Kiểm tra _mockMediator.Verify được gọi đúng số lần với It.IsAny<CreateRelationshipCommand>().
        var relationshipInputs = new List<TestRelationshipInput>
                                    {
                                        new TestRelationshipInput { SourceMemberId = Guid.NewGuid(), TargetMemberId = Guid.NewGuid(), Type = RelationshipType.Father, Order = 1 },
                                        new TestRelationshipInput { SourceMemberId = Guid.NewGuid(), TargetMemberId = Guid.NewGuid(), Type = RelationshipType.Mother, Order = 2 },
                                        new TestRelationshipInput { SourceMemberId = Guid.NewGuid(), TargetMemberId = Guid.NewGuid(), Type = RelationshipType.Wife, Order = 3 }
                                    };

        _mockMediator.Setup(m => m.Send(It.IsAny<CreateRelationshipCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));

        var command = new CreateRelationshipsCommand { Relationships = relationshipInputs.Cast<RelationshipInput>().ToList() };

        await _handler.Handle(command, CancellationToken.None);

        _mockMediator.Verify(m => m.Send(It.IsAny<CreateRelationshipCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(relationshipInputs.Count));
        // 💡 Giải thích: Handler phải xử lý từng RelationshipInput bằng cách gửi một CreateRelationshipCommand riêng biệt.
    }
}
