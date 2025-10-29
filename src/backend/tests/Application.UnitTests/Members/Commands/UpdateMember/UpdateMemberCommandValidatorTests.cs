using AutoFixture;
using backend.Application.Members.Commands.UpdateMember;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.UpdateMember;

public class UpdateMemberCommandValidatorTests
{
    private readonly UpdateMemberCommandValidator _validator;
    private readonly IFixture _fixture;

    public UpdateMemberCommandValidatorTests()
    {
        _validator = new UpdateMemberCommandValidator();
        _fixture = new Fixture();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi trường Id của UpdateMemberCommand là Guid.Empty.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateMemberCommand với Id được đặt là Guid.Empty.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính Id với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Id là trường định danh duy nhất và bắt buộc
    /// cho mỗi thành viên, không được phép để trống (Guid.Empty) khi cập nhật.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenIdIsEmpty()
    {
        var command = _fixture.Build<UpdateMemberCommand>()
            .With(c => c.Id, Guid.Empty)
            .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Id)
            .WithErrorMessage("Id cannot be empty.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi trường LastName của UpdateMemberCommand là chuỗi rỗng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateMemberCommand với LastName được đặt là chuỗi rỗng.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính LastName với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: LastName là trường bắt buộc
    /// để xác định một thành viên và không được phép để trống.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenLastNameIsEmpty()
    {
        var command = _fixture.Build<UpdateMemberCommand>()
            .With(c => c.LastName, string.Empty)
            .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.LastName)
            .WithErrorMessage("Last Name cannot be empty.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi trường FirstName của UpdateMemberCommand là chuỗi rỗng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateMemberCommand với FirstName được đặt là chuỗi rỗng.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính FirstName với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: FirstName là trường bắt buộc
    /// để xác định một thành viên và không được phép để trống.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenFirstNameIsEmpty()
    {
        var command = _fixture.Build<UpdateMemberCommand>()
            .With(c => c.FirstName, string.Empty)
            .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.FirstName)
            .WithErrorMessage("First Name cannot be empty.");
    }
    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi trường FamilyId của UpdateMemberCommand là Guid.Empty.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateMemberCommand với FamilyId được đặt là Guid.Empty.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính FamilyId với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: FamilyId là trường bắt buộc
    /// để liên kết thành viên với một gia đình và không được phép để trống.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenFamilyIdIsEmpty()
    {
        var command = _fixture.Build<UpdateMemberCommand>()
            .With(c => c.FamilyId, Guid.Empty)
            .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.FamilyId)
            .WithErrorMessage("FamilyId cannot be empty.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi DateOfDeath xảy ra trước DateOfBirth.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateMemberCommand với DateOfBirth sau DateOfDeath.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính DateOfDeath với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Logic nghiệp vụ yêu cầu ngày mất
    /// không thể xảy ra trước ngày sinh, đảm bảo tính hợp lệ của dữ liệu thời gian.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenDateOfDeathIsBeforeDateOfBirth()
    {
        var command = _fixture.Build<UpdateMemberCommand>()
            .With(c => c.DateOfBirth, new DateTime(2000, 1, 1))
            .With(c => c.DateOfDeath, new DateTime(1999, 1, 1))
            .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.DateOfDeath)
            .WithErrorMessage("DateOfDeath cannot be before DateOfBirth.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi trường Gender của UpdateMemberCommand chứa giá trị không hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateMemberCommand với Gender được đặt là một chuỗi không hợp lệ (ví dụ: "InvalidGender").
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính Gender với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Gender phải là một trong các giá trị được định nghĩa
    /// trước ('Male', 'Female', 'Other') để đảm bảo tính nhất quán và chính xác của dữ liệu giới tính.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenGenderIsInvalid()
    {
        var command = _fixture.Build<UpdateMemberCommand>()
            .With(c => c.Gender, "InvalidGender")
            .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Gender)
            .WithErrorMessage("Gender must be 'Male', 'Female', or 'Other'.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi trường AvatarUrl của UpdateMemberCommand không phải là một URL hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateMemberCommand với AvatarUrl được đặt là một chuỗi không phải URL hợp lệ (ví dụ: "invalid-url").
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính AvatarUrl với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: AvatarUrl cần phải là một URL hợp lệ
    /// để đảm bảo rằng hình ảnh đại diện có thể được truy cập và hiển thị đúng cách.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenAvatarUrlIsInvalid()
    {
        var command = _fixture.Build<UpdateMemberCommand>()
            .With(c => c.AvatarUrl, "invalid-url")
            .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.AvatarUrl)
            .WithErrorMessage("Avatar URL must be a valid URL.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi tất cả các trường của UpdateMemberCommand đều hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateMemberCommand với tất cả các trường được điền đầy đủ và hợp lệ.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng không có bất kỳ lỗi validation nào được báo cáo.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Khi tất cả dữ liệu đầu vào tuân thủ
    /// các quy tắc validation, command phải được coi là hợp lệ và không có lỗi nào được trả về.
    /// </summary>
    [Fact]
    public void ShouldNotHaveErrorWhenAllFieldsAreValid()
    {
        var command = _fixture.Build<UpdateMemberCommand>()
            .With(c => c.Id, Guid.NewGuid())
            .With(c => c.LastName, "ValidLastName")
            .With(c => c.FirstName, "ValidFirstName")
            .With(c => c.FamilyId, Guid.NewGuid())
            .With(c => c.DateOfBirth, new DateTime(1990, 1, 1))
            .With(c => c.DateOfDeath, new DateTime(2020, 1, 1))
            .With(c => c.Gender, "Male")
            .With(c => c.AvatarUrl, "https://valid.url/avatar.jpg")
            .With(c => c.Occupation, "Engineer")
            .With(c => c.Biography, "A short biography.")
            .With(c => c.Nickname, "Nick")
            .With(c => c.PlaceOfBirth, "City, Country")
            .With(c => c.PlaceOfDeath, "City, Country")
            .Create();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
