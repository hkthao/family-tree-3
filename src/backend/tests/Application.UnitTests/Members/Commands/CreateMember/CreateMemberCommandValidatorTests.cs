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

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi trường LastName của CreateMemberCommand vượt quá 100 ký tự.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateMemberCommand với LastName có độ dài lớn hơn 100 ký tự.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính LastName với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: LastName có giới hạn độ dài tối đa là 100 ký tự
    /// để đảm bảo tính nhất quán của dữ liệu và tránh tràn bộ đệm.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenLastNameExceedsMaxLength()
    {
        var command = new CreateMemberCommand
        {
            LastName = new string('a', 101),
            FirstName = "Test",
            FamilyId = Guid.NewGuid()
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.LastName)
              .WithErrorMessage("Last Name must not exceed 100 characters.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi trường FirstName của CreateMemberCommand vượt quá 100 ký tự.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateMemberCommand với FirstName có độ dài lớn hơn 100 ký tự.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính FirstName với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: FirstName có giới hạn độ dài tối đa là 100 ký tự
    /// để đảm bảo tính nhất quán của dữ liệu và tránh tràn bộ đệm.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenFirstNameExceedsMaxLength()
    {
        var command = new CreateMemberCommand
        {
            LastName = "Test",
            FirstName = new string('a', 101),
            FamilyId = Guid.NewGuid()
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
              .WithErrorMessage("First Name must not exceed 100 characters.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi trường AvatarUrl của CreateMemberCommand vượt quá 2048 ký tự.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateMemberCommand với AvatarUrl có độ dài lớn hơn 2048 ký tự.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính AvatarUrl với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: AvatarUrl có giới hạn độ dài tối đa là 2048 ký tự
    /// để đảm bảo tính nhất quán của dữ liệu và tránh tràn bộ đệm.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenAvatarUrlExceedsMaxLength()
    {
        var command = new CreateMemberCommand
        {
            LastName = "Test",
            FirstName = "Test",
            FamilyId = Guid.NewGuid(),
            AvatarUrl = "http://example.com/" + new string('a', 2048 - "http://example.com/".Length + 1) // Total length > 2048
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.AvatarUrl)
              .WithErrorMessage("Avatar URL must not exceed 2048 characters.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi trường AvatarUrl của CreateMemberCommand có định dạng không hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateMemberCommand với AvatarUrl có định dạng không phải là URL hợp lệ.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính AvatarUrl với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: AvatarUrl phải là một URL hợp lệ
    /// để đảm bảo rằng nó có thể được truy cập và hiển thị đúng cách.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenAvatarUrlIsInvalidFormat()
    {
        var command = new CreateMemberCommand
        {
            LastName = "Test",
            FirstName = "Test",
            FamilyId = Guid.NewGuid(),
            AvatarUrl = "invalid-url"
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.AvatarUrl)
              .WithErrorMessage("Avatar URL must be a valid URL.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi trường Occupation của CreateMemberCommand vượt quá 100 ký tự.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateMemberCommand với Occupation có độ dài lớn hơn 100 ký tự.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính Occupation với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Occupation có giới hạn độ dài tối đa là 100 ký tự
    /// để đảm bảo tính nhất quán của dữ liệu và tránh tràn bộ đệm.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenOccupationExceedsMaxLength()
    {
        var command = new CreateMemberCommand
        {
            LastName = "Test",
            FirstName = "Test",
            FamilyId = Guid.NewGuid(),
            Occupation = new string('a', 101)
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Occupation)
              .WithErrorMessage("Occupation must not exceed 100 characters.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi trường Biography của CreateMemberCommand vượt quá 2000 ký tự.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateMemberCommand với Biography có độ dài lớn hơn 2000 ký tự.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính Biography với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Biography có giới hạn độ dài tối đa là 2000 ký tự
    /// để đảm bảo tính nhất quán của dữ liệu và tránh tràn bộ đệm.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenBiographyExceedsMaxLength()
    {
        var command = new CreateMemberCommand
        {
            LastName = "Test",
            FirstName = "Test",
            FamilyId = Guid.NewGuid(),
            Biography = new string('a', 2001)
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Biography)
              .WithErrorMessage("Biography must not exceed 2000 characters.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi trường Nickname của CreateMemberCommand vượt quá 100 ký tự.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateMemberCommand với Nickname có độ dài lớn hơn 100 ký tự.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính Nickname với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Nickname có giới hạn độ dài tối đa là 100 ký tự
    /// để đảm bảo tính nhất quán của dữ liệu và tránh tràn bộ đệm.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenNicknameExceedsMaxLength()
    {
        var command = new CreateMemberCommand
        {
            LastName = "Test",
            FirstName = "Test",
            FamilyId = Guid.NewGuid(),
            Nickname = new string('a', 101)
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Nickname)
              .WithErrorMessage("Nickname must not exceed 100 characters.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi trường PlaceOfBirth của CreateMemberCommand vượt quá 200 ký tự.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateMemberCommand với PlaceOfBirth có độ dài lớn hơn 200 ký tự.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính PlaceOfBirth với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: PlaceOfBirth có giới hạn độ dài tối đa là 200 ký tự
    /// để đảm bảo tính nhất quán của dữ liệu và tránh tràn bộ đệm.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenPlaceOfBirthExceedsMaxLength()
    {
        var command = new CreateMemberCommand
        {
            LastName = "Test",
            FirstName = "Test",
            FamilyId = Guid.NewGuid(),
            PlaceOfBirth = new string('a', 201)
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PlaceOfBirth)
              .WithErrorMessage("Place of Birth must not exceed 200 characters.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi trường PlaceOfDeath của CreateMemberCommand vượt quá 200 ký tự.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateMemberCommand với PlaceOfDeath có độ dài lớn hơn 200 ký tự.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính PlaceOfDeath với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: PlaceOfDeath có giới hạn độ dài tối đa là 200 ký tự
    /// để đảm bảo tính nhất quán của dữ liệu và tránh tràn bộ đệm.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenPlaceOfDeathExceedsMaxLength()
    {
        var command = new CreateMemberCommand
        {
            LastName = "Test",
            FirstName = "Test",
            FamilyId = Guid.NewGuid(),
            PlaceOfDeath = new string('a', 201)
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PlaceOfDeath)
              .WithErrorMessage("Place of Death must not exceed 200 characters.");
    }
}
