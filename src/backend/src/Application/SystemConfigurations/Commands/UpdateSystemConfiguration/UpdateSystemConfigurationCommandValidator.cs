namespace backend.Application.SystemConfigurations.Commands.UpdateSystemConfiguration;

public class UpdateSystemConfigurationCommandValidator : AbstractValidator<UpdateSystemConfigurationCommand>
{
    public UpdateSystemConfigurationCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(v => v.Key)
            .NotEmpty().WithMessage("Key is required.")
            .MaximumLength(200).WithMessage("Key must not exceed 200 characters.");

        RuleFor(v => v.Value)
            .NotEmpty().WithMessage("Value is required.");

        RuleFor(v => v.ValueType)
            .NotEmpty().WithMessage("ValueType is required.")
            .MaximumLength(50).WithMessage("ValueType must not exceed 50 characters.");

        RuleFor(v => v.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");
    }
}
