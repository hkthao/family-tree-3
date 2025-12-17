using backend.Application.ExportImport.Commands;
using backend.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.ExportImport;

public class ImportFamilyCommandValidatorTests
{
    private readonly ImportFamilyCommandValidator _validator;

    public ImportFamilyCommandValidatorTests()
    {
        _validator = new ImportFamilyCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenFamilyDataIsNull()
    {
        var command = new ImportFamilyCommand { FamilyData = null! };
        var result = _validator.TestValidate(command);
        result.Errors.Should().Contain(e => e.PropertyName == "FamilyData");
    }

    [Fact]
    public void ShouldHaveError_WhenFamilyNameIsEmpty()
    {
        var familyData = new FamilyExportDto { Name = "", Code = "FAM1", Visibility = "Private" };
        var command = new ImportFamilyCommand { FamilyData = familyData };
        var result = _validator.TestValidate(command);
        result.Errors.Should().Contain(e => e.PropertyName == "FamilyData.Name");
    }

    [Fact]
    public void ShouldHaveError_WhenFamilyCodeIsEmpty()
    {
        var familyData = new FamilyExportDto { Name = "Test Family", Code = string.Empty, Visibility = "Private" };
        var command = new ImportFamilyCommand { FamilyData = familyData };
        var result = _validator.TestValidate(command);
        result.Errors.Should().Contain(e => e.PropertyName == "FamilyData.Code");
    }

    [Fact]
    public void ShouldHaveError_WhenFamilyVisibilityIsInvalid()
    {
        var familyData = new FamilyExportDto { Name = "Test Family", Code = "FAM1", Visibility = "Invalid" };
        var command = new ImportFamilyCommand { FamilyData = familyData };
        var result = _validator.TestValidate(command);
        result.Errors.Should().Contain(e => e.PropertyName == "FamilyData.Visibility");
    }

    [Fact]
    public void ShouldNotHaveError_WhenFamilyDataIsValid()
    {
        var familyData = new FamilyExportDto { Name = "Test Family", Code = "FAM1", Visibility = "Private" };
        var command = new ImportFamilyCommand { FamilyData = familyData };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveError_WhenMemberFirstNameIsEmpty()
    {
        var familyData = new FamilyExportDto
        {
            Name = "Test Family",
            Code = "FAM1",
            Visibility = "Private",
            Members = new List<MemberExportDto>
            {
                new MemberExportDto { FirstName = "", LastName = "Doe", Code = "MEM1", Gender = Gender.Male }
            }
        };
        var command = new ImportFamilyCommand { FamilyData = familyData };
        var result = _validator.TestValidate(command);
        result.Errors.Should().Contain(e => e.PropertyName == "FamilyData.Members[0].FirstName");
    }

    [Fact]
    public void ShouldHaveError_WhenMemberLastNameIsEmpty()
    {
        var familyData = new FamilyExportDto
        {
            Name = "Test Family",
            Code = "FAM1",
            Visibility = "Private",
            Members = new List<MemberExportDto>
            {
                new MemberExportDto { FirstName = "John", LastName = "", Code = "MEM1", Gender = Gender.Male }
            }
        };
        var command = new ImportFamilyCommand { FamilyData = familyData };
        var result = _validator.TestValidate(command);
        result.Errors.Should().Contain(e => e.PropertyName == "FamilyData.Members[0].LastName");
    }

    [Fact]
    public void ShouldHaveError_WhenMemberCodeIsEmpty()
    {
        var familyData = new FamilyExportDto
        {
            Name = "Test Family",
            Code = "FAM1",
            Visibility = "Private",
            Members = new List<MemberExportDto>
            {
                new MemberExportDto { FirstName = "John", LastName = "Doe", Code = "", Gender = Gender.Male }
            }
        };
        var command = new ImportFamilyCommand { FamilyData = familyData };
        var result = _validator.TestValidate(command);
        result.Errors.Should().Contain(e => e.PropertyName == "FamilyData.Members[0].Code");
    }

    [Fact]
    public void ShouldHaveError_WhenRelationshipSourceMemberIdIsEmpty()
    {
        var familyData = new FamilyExportDto
        {
            Name = "Test Family",
            Code = "FAM1",
            Visibility = "Private",
            Members = new List<MemberExportDto>
            {
                new MemberExportDto { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Code = "MEM1", Gender = Gender.Male }
            },
            Relationships = new List<RelationshipExportDto>
            {
                new RelationshipExportDto { SourceMemberId = Guid.Empty, TargetMemberId = Guid.NewGuid(), Type = RelationshipType.Husband }
            }
        };
        var command = new ImportFamilyCommand { FamilyData = familyData };
        var result = _validator.TestValidate(command);
        result.Errors.Should().Contain(e => e.PropertyName == "FamilyData.Relationships[0].SourceMemberId");
    }

    [Fact]
    public void ShouldHaveError_WhenRelationshipTargetMemberIdIsEmpty()
    {
        var familyData = new FamilyExportDto
        {
            Name = "Test Family",
            Code = "FAM1",
            Visibility = "Private",
            Members = new List<MemberExportDto>
            {
                new MemberExportDto { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Code = "MEM1", Gender = Gender.Male }
            },
            Relationships = new List<RelationshipExportDto>
            {
                new RelationshipExportDto { SourceMemberId = Guid.NewGuid(), TargetMemberId = Guid.Empty, Type = RelationshipType.Husband }
            }
        };
        var command = new ImportFamilyCommand { FamilyData = familyData };
        var result = _validator.TestValidate(command);
        result.Errors.Should().Contain(e => e.PropertyName == "FamilyData.Relationships[0].TargetMemberId");
    }

    [Fact]
    public void ShouldHaveError_WhenEventNameIsEmpty()
    {
        var familyData = new FamilyExportDto
        {
            Name = "Test Family",
            Code = "FAM1",
            Visibility = "Private",
            Events = new List<EventExportDto>
            {
                new EventExportDto { Name = "", Code = "EVT1", Type = EventType.Birth }
            }
        };
        var command = new ImportFamilyCommand { FamilyData = familyData };
        var result = _validator.TestValidate(command);
        result.Errors.Should().Contain(e => e.PropertyName == "FamilyData.Events[0].Name");
    }

    [Fact]
    public void ShouldHaveError_WhenEventCodeIsEmpty()
    {
        var familyData = new FamilyExportDto
        {
            Name = "Test Family",
            Code = "FAM1",
            Visibility = "Private",
            Events = new List<EventExportDto>
            {
                new EventExportDto { Name = "Birth", Code = "", Type = EventType.Birth }
            }
        };
        var command = new ImportFamilyCommand { FamilyData = familyData };
        var result = _validator.TestValidate(command);
        result.Errors.Should().Contain(e => e.PropertyName == "FamilyData.Events[0].Code");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi Type của EventExportDto không hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một EventExportDto với Type không hợp lệ.
    ///    - Act: Gọi phương thức TestValidate của validator.
    ///    - Assert: Kiểm tra xem có lỗi xác thực cho thuộc tính Type.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Type của EventExportDto phải là một giá trị hợp lệ của enum.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenEventTypeIsInvalid()
    {
        // Arrange
        var familyData = new FamilyExportDto
        {
            Name = "Test Family",
            Code = "FAM1",
            Visibility = "Private",
            Events = new List<EventExportDto>
            {
                new EventExportDto { Name = "Invalid Type Event", Code = "EVT-INV", Type = (EventType)99 } // Invalid enum value
            }
        };
        var command = new ImportFamilyCommand { FamilyData = familyData };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.Errors.Should().Contain(e => e.PropertyName == "FamilyData.Events[0].Type");
    }
}
