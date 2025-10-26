using backend.Application.Members.Commands.CreateMember;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.CreateMember;

public class CreateMemberCommandValidatorTests
{
    private readonly CreateMemberCommandValidator _validator;

    public CreateMemberCommandValidatorTests()
    {
        _validator = new CreateMemberCommandValidator();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi trường LastName của CreateMemberCommand là chuỗi rỗng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateMemberCommand với LastName được đặt là chuỗi rỗng.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính LastName với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: LastName là trường bắt buộc
    /// để xác định một thành viên và không được phép để trống.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenLastNameIsEmpty()
    {
        var command = new CreateMemberCommand { LastName = string.Empty, FirstName = "Test", FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.LastName)
              .WithErrorMessage("Last Name cannot be empty.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi trường FirstName của CreateMemberCommand là chuỗi rỗng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateMemberCommand với FirstName được đặt là chuỗi rỗng.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính FirstName với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: FirstName là trường bắt buộc
    /// để xác định một thành viên và không được phép để trống.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenFirstNameIsEmpty()
    {
        var command = new CreateMemberCommand { LastName = "Test", FirstName = string.Empty, FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
              .WithErrorMessage("First Name cannot be empty.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi trường FamilyId của CreateMemberCommand là Guid.Empty.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateMemberCommand với FamilyId được đặt là Guid.Empty.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính FamilyId với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: FamilyId là trường bắt buộc
    /// để liên kết thành viên với một gia đình và không được phép để trống.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenFamilyIdIsEmpty()
    {
        var command = new CreateMemberCommand { LastName = "Test", FirstName = "Test", FamilyId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FamilyId)
              .WithErrorMessage("FamilyId cannot be empty.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi DateOfDeath xảy ra trước DateOfBirth.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateMemberCommand với DateOfBirth sau DateOfDeath.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính DateOfDeath với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Logic nghiệp vụ yêu cầu ngày mất
    /// không thể xảy ra trước ngày sinh, đảm bảo tính hợp lệ của dữ liệu thời gian.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenDateOfDeathIsBeforeDateOfBirth()
    {
        var command = new CreateMemberCommand
        {
            LastName = "Test",
            FirstName = "Test",
            FamilyId = Guid.NewGuid(),
            DateOfBirth = new DateTime(2000, 1, 1),
            DateOfDeath = new DateTime(1999, 1, 1)
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.DateOfDeath)
              .WithErrorMessage("DateOfDeath cannot be before DateOfBirth.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi trường Gender của CreateMemberCommand chứa giá trị không hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateMemberCommand với Gender được đặt là một chuỗi không hợp lệ (ví dụ: "InvalidGender").
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính Gender với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Gender phải là một trong các giá trị được định nghĩa
    /// trước ('Male', 'Female', 'Other') để đảm bảo tính nhất quán và chính xác của dữ liệu giới tính.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenGenderIsInvalid()
    {
        var command = new CreateMemberCommand
        {
            LastName = "Test",
            FirstName = "Test",
            FamilyId = Guid.NewGuid(),
            Gender = "InvalidGender"
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Gender)
              .WithErrorMessage("Gender must be 'Male', 'Female', or 'Other'.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi tất cả các trường của CreateMemberCommand đều hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateMemberCommand với tất cả các trường được điền đầy đủ và hợp lệ.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng không có bất kỳ lỗi validation nào được báo cáo.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Khi tất cả dữ liệu đầu vào tuân thủ
    /// các quy tắc validation, command phải được coi là hợp lệ và không có lỗi nào được trả về.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenCommandIsValid()
    {
        var command = new CreateMemberCommand
        {
            LastName = "Valid",
            FirstName = "Member",
            FamilyId = Guid.NewGuid(),
            DateOfBirth = new DateTime(1990, 1, 1),
            DateOfDeath = new DateTime(2020, 1, 1),
            Gender = "Male",
            AvatarUrl = "http://example.com/avatar.jpg",
            Occupation = "Engineer",
            Biography = "A valid biography.",
            Nickname = "Nick",
            PlaceOfBirth = "City A",
            PlaceOfDeath = "City B"
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
