namespace backend.Application.SystemConfigurations.Commands.DeleteSystemConfiguration;

public class DeleteSystemConfigurationCommandValidator : AbstractValidator<DeleteSystemConfigurationCommand>
{
    public DeleteSystemConfigurationCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Id is required.");
    }
}
