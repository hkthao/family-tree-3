using System;
using System.Collections.Generic;
using backend.Application.Relationships.Commands.CreateRelationships;
using backend.Application.Relationships.Commands.Inputs;
using backend.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Commands.CreateRelationships;

public class CreateRelationshipsCommandValidatorTests
{
    private readonly CreateRelationshipsCommandValidator _validator;

    public CreateRelationshipsCommandValidatorTests()
    {
        _validator = new CreateRelationshipsCommandValidator();
    }

    // Concrete implementation for testing abstract RelationshipInput
    private record TestRelationshipInput : RelationshipInput;

    [Fact]
    public void ShouldHaveErrorWhenRelationshipsListIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi danh sách Relationships trống.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một CreateRelationshipsCommand với danh sách Relationships rỗng.
        // 2. Act: Gọi Validate trên validator.
        // 3. Assert: Kiểm tra có lỗi validation cho Relationships.
        var command = new CreateRelationshipsCommand
        {
            Relationships = []
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Relationships);
        result.Errors.Should().Contain(e => e.ErrorMessage == "Danh sách mối quan hệ không được để trống.");
        // 💡 Giải thích: Danh sách mối quan hệ không được phép trống.
    }

    [Fact]
    public void ShouldHaveErrorWhenSourceMemberIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi SourceMemberId trong RelationshipInput trống.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một CreateRelationshipsCommand với một RelationshipInput có SourceMemberId là Guid.Empty.
        // 2. Act: Gọi Validate trên validator.
        // 3. Assert: Kiểm tra có lỗi validation cho SourceMemberId.
        var command = new CreateRelationshipsCommand
        {
            Relationships =
            [
                new TestRelationshipInput { SourceMemberId = Guid.Empty, TargetMemberId = Guid.NewGuid(), Type = RelationshipType.Father }
            ]
        };

        var result = _validator.TestValidate(command);

        result.Errors.Should().Contain(e => e.PropertyName == "Relationships[0].SourceMemberId" && e.ErrorMessage == "ID thành viên nguồn không được để trống.");
        // 💡 Giải thích: SourceMemberId là bắt buộc.
    }

    [Fact]
    public void ShouldHaveErrorWhenTargetMemberIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi TargetMemberId trong RelationshipInput trống.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một CreateRelationshipsCommand với một RelationshipInput có TargetMemberId là Guid.Empty.
        // 2. Act: Gọi Validate trên validator.
        // 3. Assert: Kiểm tra có lỗi validation cho TargetMemberId.
        var command = new CreateRelationshipsCommand
        {
            Relationships =
            [
                new TestRelationshipInput { SourceMemberId = Guid.NewGuid(), TargetMemberId = Guid.Empty, Type = RelationshipType.Father }
            ]
        };

        var result = _validator.TestValidate(command);

        result.Errors.Should().Contain(e => e.PropertyName == "Relationships[0].TargetMemberId" && e.ErrorMessage == "ID thành viên đích không được để trống.");
        // 💡 Giải thích: TargetMemberId là bắt buộc.
    }

    [Fact]
    public void ShouldHaveErrorWhenRelationshipTypeIsInvalid()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi Type trong RelationshipInput không hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một CreateRelationshipsCommand với một RelationshipInput có Type là một giá trị không hợp lệ.
        // 2. Act: Gọi Validate trên validator.
        // 3. Assert: Kiểm tra có lỗi validation cho Type.
        var command = new CreateRelationshipsCommand
        {
            Relationships =
            [
                new TestRelationshipInput { SourceMemberId = Guid.NewGuid(), TargetMemberId = Guid.NewGuid(), Type = (RelationshipType)999 }
            ]
        };

        var result = _validator.TestValidate(command);

        result.Errors.Should().Contain(e => e.PropertyName == "Relationships[0].Type" && e.ErrorMessage == "Loại mối quan hệ không hợp lệ.");
        // 💡 Giải thích: Type phải là một giá trị hợp lệ của enum RelationshipType.
    }

    [Fact]
    public void ShouldHaveErrorWhenSourceAndTargetMembersAreSame()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi SourceMemberId và TargetMemberId trong RelationshipInput giống nhau.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một CreateRelationshipsCommand với một RelationshipInput có SourceMemberId và TargetMemberId giống nhau.
        // 2. Act: Gọi Validate trên validator.
        // 3. Assert: Kiểm tra có lỗi validation cho mối quan hệ.
        var memberId = Guid.NewGuid();
        var command = new CreateRelationshipsCommand
        {
            Relationships =
            [
                new TestRelationshipInput { SourceMemberId = memberId, TargetMemberId = memberId, Type = RelationshipType.Father }
            ]
        };

        var result = _validator.TestValidate(command);

        result.Errors.Should().Contain(e => e.PropertyName == "Relationships[0]" && e.ErrorMessage == "Thành viên nguồn và thành viên đích không được giống nhau.");
        // 💡 Giải thích: Thành viên nguồn và đích không được giống nhau.
    }

    [Fact]
    public void ShouldNotHaveErrorWhenCommandIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi CreateRelationshipsCommand hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một CreateRelationshipsCommand hợp lệ.
        // 2. Act: Gọi Validate trên validator.
        // 3. Assert: Kiểm tra không có lỗi validation.
        var command = new CreateRelationshipsCommand
        {
            Relationships =
            [
                new TestRelationshipInput { SourceMemberId = Guid.NewGuid(), TargetMemberId = Guid.NewGuid(), Type = RelationshipType.Father, Order = 1 }
            ]
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
        // 💡 Giải thích: Command hợp lệ phải vượt qua validation.
    }
}
