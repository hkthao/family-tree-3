namespace backend.Application.Identity.Commands.CreateNovuSubscriber;

/// <summary>
/// Command để tạo một subscriber trên Novu
/// </summary>
public class CreateNovuSubscriberCommand : IRequest
{
    public Guid UserId { get; set; }
}
