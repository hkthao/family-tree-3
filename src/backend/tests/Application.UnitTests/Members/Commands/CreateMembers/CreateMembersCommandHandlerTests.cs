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
        _mockMediator = new Mock<IMediator>();

        _fixture.Customize<AIMemberDto>(c => c.With(x => x.Gender, "Male").With(x => x.ValidationErrors, (List<string>?)null));

        _handler = new CreateMembersCommandHandler(
            _mockAIMemberDtoValidator.Object,
            _mockMediator.Object
        );
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một danh sách rỗng các ID
    /// khi tất cả các thành viên trong command đều không hợp lệ (validation thất bại).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateMembersCommand chứa nhiều AIMemberDto.
    ///               Thiết lập _mockAIMemberDtoValidator để trả về một ValidationResult chứa lỗi
    ///               cho tất cả các AIMemberDto.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công (IsSuccess = true) nhưng danh sách Value là rỗng.
    ///              Kiểm tra rằng thuộc tính ValidationErrors của mỗi AIMemberDto không rỗng.
    ///              Xác minh rằng phương thức Send của _mockMediator không bao giờ được gọi (vì không có thành viên hợp lệ nào để tạo).
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng nếu không có thành viên nào
    /// vượt qua bước validation ban đầu, hệ thống sẽ không cố gắng tạo chúng và trả về một kết quả thành công
    /// với danh sách ID rỗng, đồng thời ghi nhận lỗi validation vào từng thành viên.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnSuccessWithEmptyList_WhenAllMembersAreInvalid()
    {
        var members = _fixture.CreateMany<AIMemberDto>(3).ToList();
        var command = new CreateMembersCommand(members);

        _mockAIMemberDtoValidator.Setup(v => v.ValidateAsync(It.IsAny<AIMemberDto>(), It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new ValidationResult(new List<ValidationFailure> { new("Prop", "Error") }));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        members.Should().AllSatisfy(m => m.ValidationErrors.Should().NotBeEmpty());
        _mockMediator.Verify(m => m.Send(It.IsAny<CreateMemberCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về tất cả các ID của thành viên
    /// khi tất cả các thành viên trong command đều hợp lệ và được tạo thành công.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateMembersCommand chứa nhiều AIMemberDto.
    ///               Thiết lập _mockAIMemberDtoValidator để trả về một ValidationResult thành công
    ///               cho tất cả các AIMemberDto.
    ///               Thiết lập _mockMediator để trả về một Result.Success với một Guid mới
    ///               mỗi khi CreateMemberCommand được gửi đi.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công (IsSuccess = true) và danh sách Value
    ///              chứa tất cả các ID đã được tạo. Kiểm tra rằng thuộc tính ValidationErrors của
    ///              mỗi AIMemberDto là rỗng hoặc null. Xác minh rằng phương thức Send của _mockMediator
    ///              được gọi đúng số lần bằng số lượng thành viên.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng nếu tất cả các thành viên
    /// đều hợp lệ, chúng sẽ được xử lý để tạo và tất cả các ID của thành viên được tạo thành công
    /// sẽ được trả về trong kết quả.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnSuccessWithAllIds_WhenAllMembersAreValid()
    {
        var members = _fixture.CreateMany<AIMemberDto>(3).ToList();
        var command = new CreateMembersCommand(members);

        _mockAIMemberDtoValidator.Setup(v => v.ValidateAsync(It.IsAny<AIMemberDto>(), It.IsAny<CancellationToken>()))
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
        result.Value.Should().HaveCount(members.Count);
        result.Value.Should().BeEquivalentTo(createdIds);
        _mockMediator.Verify(m => m.Send(It.IsAny<CreateMemberCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(members.Count));
        members.Should().AllSatisfy(m => m.ValidationErrors.Should().BeNullOrEmpty());
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một danh sách ID một phần
    /// khi một số thành viên trong command không hợp lệ (validation thất bại) và một số khác hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateMembersCommand chứa nhiều AIMemberDto.
    ///               Thiết lập _mockAIMemberDtoValidator để trả về lỗi cho một số AIMemberDto
    ///               và thành công cho những AIMemberDto còn lại.
    ///               Thiết lập _mockMediator để trả về một Result.Success với một Guid mới
    ///               chỉ cho các CreateMemberCommand hợp lệ.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công (IsSuccess = true) và danh sách Value
    ///              chứa các ID của chỉ những thành viên hợp lệ được tạo. Kiểm tra rằng thuộc tính
    ///              ValidationErrors của các AIMemberDto không hợp lệ là không rỗng và của các AIMemberDto
    ///              hợp lệ là rỗng hoặc null. Xác minh rằng phương thức Send của _mockMediator
    ///              được gọi đúng số lần bằng số lượng thành viên hợp lệ.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống chỉ cố gắng tạo
    /// các thành viên đã vượt qua validation. Các thành viên không hợp lệ sẽ được bỏ qua và lỗi của chúng
    /// sẽ được ghi nhận, trong khi các thành viên hợp lệ sẽ được tạo và ID của chúng sẽ được trả về.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnSuccessWithPartialIds_WhenSomeMembersAreInvalid()
    {
        var members = _fixture.CreateMany<AIMemberDto>(3).ToList();
        var command = new CreateMembersCommand(members);
        var invalidMember = members[0];
        var validMembers = members.Skip(1).ToList();

        _mockAIMemberDtoValidator.Setup(v => v.ValidateAsync(invalidMember, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new ValidationResult(new List<ValidationFailure> { new("Prop", "Error") }));
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
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một danh sách ID một phần
    /// khi một số thành viên hợp lệ nhưng việc tạo chúng thông qua Mediator thất bại.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateMembersCommand chứa nhiều AIMemberDto.
    ///               Thiết lập _mockAIMemberDtoValidator để trả về ValidationResult thành công
    ///               cho tất cả các AIMemberDto.
    ///               Thiết lập _mockMediator để trả về Result.Success với một Guid mới
    ///               cho một số CreateMemberCommand và Result.Failure cho những CreateMemberCommand khác.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công (IsSuccess = true) và danh sách Value
    ///              chứa các ID của chỉ những thành viên được tạo thành công. Kiểm tra rằng thuộc tính
    ///              ValidationErrors của các AIMemberDto mà việc tạo thất bại là không rỗng và của các
    ///              AIMemberDto được tạo thành công là rỗng hoặc null. Xác minh rằng phương thức Send
    ///              của _mockMediator được gọi đúng số lần bằng số lượng thành viên.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống vẫn tiếp tục
    /// xử lý các thành viên khác ngay cả khi việc tạo một số thành viên thất bại. Chỉ các ID của
    /// những thành viên được tạo thành công mới được trả về, và lỗi của các thành viên thất bại
    /// sẽ được ghi nhận vào thuộc tính ValidationErrors của chúng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnSuccessWithPartialIds_WhenSomeMembersFailCreation()
    {
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
                                 .ReturnsAsync(new ValidationResult());

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
        result.Value.Should().HaveCount(2);
        result.Value.Should().BeEquivalentTo(expectedCreatedIds);
        member1.ValidationErrors.Should().BeNullOrEmpty();
        member2.ValidationErrors.Should().NotBeEmpty();
        member3.ValidationErrors.Should().BeNullOrEmpty();
        _mockMediator.Verify(m => m.Send(It.IsAny<CreateMemberCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(members.Count));
    }
}
