using FluentValidation;

namespace TaskFlow.Application.Features.Projects.Commands.CreateProject;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(4000);
    }
}
