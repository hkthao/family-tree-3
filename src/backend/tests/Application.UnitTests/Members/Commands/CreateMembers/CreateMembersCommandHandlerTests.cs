using AutoFixture;
using backend.Application.Common.Models;
using backend.Application.Members.Commands.CreateMember;
using backend.Application.Members.Commands.CreateMembers;
using backend.Application.Members.Queries;
using backend.Application.UnitTests.Common;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.CreateMembers;

public class CreateMembersCommandHandlerTests : TestBase
{
    private readonly Mock<IValidator<AIMemberDto>> _mockAIMemberDtoValidator;
    private readonly Mock<IMediator> _mockMediator;
    private readonly CreateMembersCommandHandler _handler;

    public CreateMembersCommandHandlerTests()

    {

        _mockAIMemberDtoValidator = new Mock<IValidator<AIMemberDto>>();

        _mockMediator = new Mock<IMediator>(); // Khởi tạo _mockMediator



        _fixture.Customize<AIMemberDto>(c => c.With(x => x.Gender, "Male").With(x => x.ValidationErrors, new List<string>())); // Ensure valid gender and empty ValidationErrors for AIMemberDto

        _handler = new CreateMembersCommandHandler(

            _mockAIMemberDtoValidator.Object,

            _mockMediator.Object

        );

    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithEmptyList_WhenAllMembersAreInvalid()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về danh sách rỗng khi tất cả thành viên đều không hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một CreateMembersCommand với nhiều thành viên.
        //    Mock _mockAIMemberDtoValidator để trả về lỗi validation cho tất cả thành viên.
        // 2. Act: Gọi phương thức Handle của handler.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và danh sách ID rỗng.
        var members = _fixture.CreateMany<AIMemberDto>(3).ToList();
        var command = new CreateMembersCommand(members);

        _mockAIMemberDtoValidator.Setup(v => v.ValidateAsync(It.IsAny<AIMemberDto>(), It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new ValidationResult(new List<ValidationFailure> { new ValidationFailure("Prop", "Error") }));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        members.Should().AllSatisfy(m => m.ValidationErrors.Should().NotBeEmpty());
        _mockMediator.Verify(m => m.Send(It.IsAny<CreateMemberCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        // 💡 Giải thích: Nếu các thành viên không vượt qua validation ban đầu, chúng sẽ không được gửi đi để tạo.
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithAllIds_WhenAllMembersAreValid()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về tất cả ID khi tất cả thành viên đều hợp lệ và được tạo thành công.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một CreateMembersCommand với nhiều thành viên.
        //    Mock _mockAIMemberDtoValidator để trả về thành công cho tất cả thành viên.
        //    Mock _mockMediator để trả về thành công với ID hợp lệ cho mỗi CreateMemberCommand.
        // 2. Act: Gọi phương thức Handle của handler.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và danh sách ID chứa tất cả ID đã tạo.
        var members = _fixture.CreateMany<AIMemberDto>(3).ToList();
        var command = new CreateMembersCommand(members);

        _mockAIMemberDtoValidator.Setup(v => v.ValidateAsync(It.IsAny<AIMemberDto>(), It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new ValidationResult()); // Validation thành công

        var createdIds = new List<Guid>();
        _mockMediator.Setup(m => m.Send(It.IsAny<CreateMemberCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(() =>
                     {
                         var newId = Guid.NewGuid();
                         createdIds.Add(newId);
                         return Result<Guid>.Success(newId);
                     });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(members.Count);
        result.Value.Should().BeEquivalentTo(createdIds);
        _mockMediator.Verify(m => m.Send(It.IsAny<CreateMemberCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(members.Count));
        members.Should().AllSatisfy(m => m.ValidationErrors.Should().BeNullOrEmpty());
        // 💡 Giải thích: Tất cả thành viên hợp lệ sẽ được gửi đi để tạo và ID của chúng sẽ được trả về.
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithPartialIds_WhenSomeMembersAreInvalid()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về một phần ID khi một số thành viên không hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một CreateMembersCommand với 3 thành viên.
        //    Mock _mockAIMemberDtoValidator để trả về lỗi cho thành viên đầu tiên, thành công cho hai thành viên còn lại.
        //    Mock _mockMediator để trả về thành công cho các CreateMemberCommand hợp lệ.
        // 2. Act: Gọi phương thức Handle của handler.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và danh sách ID chỉ chứa ID của các thành viên hợp lệ.
        var members = _fixture.CreateMany<AIMemberDto>(3).ToList();
        var command = new CreateMembersCommand(members); // Thêm dòng này
        var invalidMember = members[0];
        var validMembers = members.Skip(1).ToList();

        _mockAIMemberDtoValidator.Setup(v => v.ValidateAsync(invalidMember, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new ValidationResult(new List<ValidationFailure> { new ValidationFailure("Prop", "Error") }));
        _mockAIMemberDtoValidator.Setup(v => v.ValidateAsync(It.Is<AIMemberDto>(m => validMembers.Contains(m)), It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new ValidationResult());

        var createdIds = new List<Guid>();
        _mockMediator.Setup(m => m.Send(It.IsAny<CreateMemberCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(() =>
                     {
                         var newId = Guid.NewGuid();
                         createdIds.Add(newId);
                         return Result<Guid>.Success(newId);
                     });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(validMembers.Count);
        result.Value.Should().BeEquivalentTo(createdIds);
        invalidMember.ValidationErrors.Should().NotBeEmpty();
        validMembers.Should().AllSatisfy(m => m.ValidationErrors.Should().BeNullOrEmpty());
        _mockMediator.Verify(m => m.Send(It.IsAny<CreateMemberCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(validMembers.Count));
        // 💡 Giải thích: Chỉ các thành viên hợp lệ mới được gửi đi để tạo, và ID của chúng sẽ được trả về.
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithPartialIds_WhenSomeMembersFailCreation()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về một phần ID khi một số thành viên hợp lệ nhưng việc tạo thất bại.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một CreateMembersCommand với 3 thành viên.
        //    Mock _mockAIMemberDtoValidator để trả về thành công cho tất cả thành viên.
        //    Mock _mockMediator để trả về thành công cho thành viên đầu tiên, thất bại cho thành viên thứ hai, và thành công cho thành viên thứ ba.
        // 2. Act: Gọi phương thức Handle của handler.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và danh sách ID chỉ chứa ID của các thành viên được tạo thành công.
        var member1 = _fixture.Build<AIMemberDto>()
            .With(x => x.FirstName, "Member1")
            .With(x => x.Gender, "Male")
            .With(x => x.ValidationErrors, (List<string>?)null)
            .Create();
        var member2 = _fixture.Build<AIMemberDto>()
            .With(x => x.FirstName, "Member2")
            .With(x => x.Gender, "Female")
            .With(x => x.ValidationErrors, (List<string>?)null)
            .Create();
        var member3 = _fixture.Build<AIMemberDto>()
            .With(x => x.FirstName, "Member3")
            .With(x => x.Gender, "Other")
            .With(x => x.ValidationErrors, (List<string>?)null)
            .Create();

        var members = new List<AIMemberDto> { member1, member2, member3 };
        var command = new CreateMembersCommand(members);

        _mockAIMemberDtoValidator.Setup(v => v.ValidateAsync(It.IsAny<AIMemberDto>(), It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new ValidationResult()); // Mặc định là thành công

        var expectedCreatedIds = new List<Guid>();
        _mockMediator.Setup(m => m.Send(It.Is<CreateMemberCommand>(cmd => cmd.FirstName == member1.FirstName), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(() =>
                     {
                         var newId = Guid.NewGuid();
                         expectedCreatedIds.Add(newId);
                         return Result<Guid>.Success(newId);
                     });
        _mockMediator.Setup(m => m.Send(It.Is<CreateMemberCommand>(cmd => cmd.FirstName == member2.FirstName), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<Guid>.Failure("Creation failed."));
        _mockMediator.Setup(m => m.Send(It.Is<CreateMemberCommand>(cmd => cmd.FirstName == member3.FirstName), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(() =>
                     {
                         var newId = Guid.NewGuid();
                         expectedCreatedIds.Add(newId);
                         return Result<Guid>.Success(newId);
                     });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2); // Chỉ 2 thành viên được tạo thành công
        result.Value.Should().BeEquivalentTo(expectedCreatedIds);
        member1.ValidationErrors.Should().BeNullOrEmpty();
        member2.ValidationErrors.Should().NotBeEmpty(); // Thành viên thứ 2 có lỗi tạo
        member3.ValidationErrors.Should().BeNullOrEmpty();
        _mockMediator.Verify(m => m.Send(It.IsAny<CreateMemberCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(members.Count));
        // 💡 Giải thích: Chỉ các thành viên được tạo thành công mới có ID trong danh sách trả về. Các thành viên thất bại sẽ có lỗi được ghi lại.
    }
}
