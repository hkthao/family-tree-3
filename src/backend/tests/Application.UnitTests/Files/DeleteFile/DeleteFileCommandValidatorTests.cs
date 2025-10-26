using AutoFixture;
using backend.Application.Files.DeleteFile;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Files.DeleteFile;

public class DeleteFileCommandValidatorTests
{
    private readonly DeleteFileCommandValidator _validator;
    private readonly IFixture _fixture;

    public DeleteFileCommandValidatorTests()
    {
        _validator = new DeleteFileCommandValidator();
        _fixture = new Fixture();
    }

            /// <summary>

            /// 🎯 Mục tiêu của test: Xác minh rằng validator báo lỗi khi FileId của DeleteFileCommand là Guid.Empty.

            /// ⚙️ Các bước (Arrange, Act, Assert):

            ///    - Arrange: Tạo một DeleteFileCommand với FileId được đặt thành Guid.Empty.

            ///    - Act: Gọi phương thức TestValidate của validator trên command đã tạo.

            ///    - Assert: Kiểm tra rằng có một lỗi xác thực cho thuộc tính FileId với thông báo lỗi cụ thể "FileId cannot be empty.".

            /// 💡 Giải thích vì sao kết quả mong đợi là đúng: FileId là một trường bắt buộc và không được phép có giá trị rỗng để đảm bảo tính hợp lệ của yêu cầu xóa tệp.

            /// </summary>

            [Fact]

            public void ShouldHaveErrorWhenFileIdIsEmpty()

            {

                var command = new DeleteFileCommand { FileId = Guid.Empty };

        

                var result = _validator.TestValidate(command);

        

                result.ShouldHaveValidationErrorFor(c => c.FileId)

                    .WithErrorMessage("FileId cannot be empty.");

            }

            /// <summary>

            /// 🎯 Mục tiêu của test: Xác minh rằng validator không báo lỗi khi FileId của DeleteFileCommand được cung cấp hợp lệ.

            /// ⚙️ Các bước (Arrange, Act, Assert):

            ///    - Arrange: Tạo một DeleteFileCommand với FileId được đặt thành một Guid hợp lệ (không phải Guid.Empty).

            ///    - Act: Gọi phương thức TestValidate của validator trên command đã tạo.

            ///    - Assert: Kiểm tra rằng không có lỗi xác thực nào cho thuộc tính FileId.

            /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Một FileId hợp lệ nên vượt qua quá trình xác thực mà không có bất kỳ lỗi nào.

            /// </summary>

            [Fact]

            public void ShouldNotHaveErrorWhenFileIdIsProvided()

            {

                var command = new DeleteFileCommand { FileId = Guid.NewGuid() };

        

                var result = _validator.TestValidate(command);

        

                result.ShouldNotHaveValidationErrorFor(c => c.FileId);

            }
}
